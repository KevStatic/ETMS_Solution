using Dapper;
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ETMS.Infrastructure.Repositories
{
    public class ApprovalService : IApprovalService
    {
        private readonly DapperContext _ctx;
        private readonly INotificationRepository _notificationRepo;

        private static readonly Dictionary<string, string> _nextStatus = new()
        {
            { "Manager", "ManagerApproved" },
            { "HOD",     "HODApproved"     },
            { "HR",      "Approved"        }
        };

        public ApprovalService(DapperContext ctx, INotificationRepository notificationRepo)
        {
            _ctx = ctx;
            _notificationRepo = notificationRepo;
        }

        public async Task<bool> ProcessApprovalAsync(
            int requestId, int approverId, string approverRole,
            string decision, string comments)
        {
            using var conn = _ctx.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                string expectedStatus = approverRole switch
                {
                    "Manager" => "Pending",
                    "HOD" => "ManagerApproved",
                    "HR" => "HODApproved",
                    _ => throw new ArgumentException("Unknown role")
                };

                var currentStatus = await conn.ExecuteScalarAsync<string>(
                    "SELECT Status FROM TransferRequests WHERE TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                if (currentStatus != expectedStatus) return false;

                await conn.ExecuteAsync(@"
                    MERGE TransferApprovals AS target
                    USING (SELECT @RequestId AS TransferRequestId,
                                  @Role AS ApproverRole) AS source
                    ON target.TransferRequestId = source.TransferRequestId
                       AND target.ApproverRole = source.ApproverRole
                    WHEN MATCHED THEN
                        UPDATE SET ApprovalStatus = @Decision,
                                   ApproverId     = @ApproverId,
                                   Comments       = @Comments,
                                   ActionDate     = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (TransferRequestId, ApproverId, ApproverRole,
                                ApprovalStatus, Comments, ActionDate)
                        VALUES (@RequestId, @ApproverId, @Role,
                                @Decision, @Comments, GETDATE());",
                    new
                    {
                        RequestId = requestId,
                        ApproverId = approverId,
                        Role = approverRole,
                        Decision = decision,
                        Comments = comments
                    }, tx);

                string newStatus = decision == "Approved"
                    ? _nextStatus[approverRole]
                    : "Rejected";

                await conn.ExecuteAsync(
                    "UPDATE TransferRequests SET Status = @Status WHERE TransferRequestId = @Id;",
                    new { Status = newStatus, Id = requestId }, tx);

                // Notify the employee of the status change
                var empId = await conn.ExecuteScalarAsync<int>(
                    "SELECT EmployeeId FROM TransferRequests WHERE TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                string notifTitle = decision == "Approved"
                    ? $"Transfer Request {(newStatus == "Approved" ? "Fully Approved" : "Moving Forward")}"
                    : "Transfer Request Rejected";

                string notifMsg = newStatus switch
                {
                    "ManagerApproved" => $"Your transfer request TR-{requestId:D5} has been approved by your Manager and is now with the HOD.",
                    "HODApproved" => $"Your transfer request TR-{requestId:D5} has been approved by the HOD and is now with HR.",
                    "Approved" => $"Your transfer request TR-{requestId:D5} has been fully approved. Your transfer letter is ready.",
                    "Rejected" => $"Your transfer request TR-{requestId:D5} has been rejected. Please contact HR for more details.",
                    _ => $"Your transfer request TR-{requestId:D5} status has been updated to {newStatus}."
                };

                await _notificationRepo.AddAsync(empId, notifTitle, notifMsg, requestId);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<string> FinaliseAndGenerateLetterAsync(
            int requestId, int hrEmployeeId, string comments, string wwwRootPath)
        {
            bool ok = await ProcessApprovalAsync(
                requestId, hrEmployeeId, "HR", "Approved", comments);
            if (!ok) return null;

            using var conn = _ctx.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var req = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT tr.*,
                   lFrom.City + ', ' + lFrom.State  AS FromLocationName,
                   lTo.City   + ', ' + lTo.State    AS TargetLocationName,
                   dFrom.DepartmentName              AS FromDeptName,
                   dTo.DepartmentName                AS TargetDeptName,
                   CONCAT(e.FirstName,' ',e.LastName) AS FullName,
                   e.EmployeeCode,
                   e.Grade,
                   e.Company
            FROM TransferRequests tr
            INNER JOIN Locations   lFrom ON lFrom.LocationId   = tr.FromLocationId
            INNER JOIN Locations   lTo   ON lTo.LocationId     = tr.ToLocationId
            INNER JOIN Departments dFrom ON dFrom.DepartmentId = tr.FromDepartmentId
            INNER JOIN Departments dTo   ON dTo.DepartmentId   = tr.ToDepartmentId
            INNER JOIN Employee    e     ON e.EmployeeId       = tr.EmployeeId
            WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                // Remove one matching open position
                await conn.ExecuteAsync(@"
            DELETE TOP(1) FROM OpenPositions
            WHERE LocationName   = @Location
              AND DepartmentName = @Dept;",
                    new
                    {
                        Location = (string)req.TargetLocationName,
                        Dept = (string)req.TargetDeptName
                    }, tx);

                // Generate PDF
                string fileName = $"TL_{req.EmployeeCode}_{requestId}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                string lettersDir = Path.Combine(wwwRootPath, "letters");
                Directory.CreateDirectory(lettersDir);
                string fullPath = Path.Combine(lettersDir, fileName);
                string letterPath = $"/letters/{fileName}";

                GenerateTransferLetter(fullPath, req, comments);

                // Store letter path
                await conn.ExecuteAsync(
                    "UPDATE TransferRequests SET LetterPath = @Path WHERE TransferRequestId = @Id;",
                    new { Path = letterPath, Id = requestId }, tx);

                // Log to TransferHistory
                await conn.ExecuteAsync(@"
            INSERT INTO TransferHistory
                (EmployeeId, OldDepartmentId, NewDepartmentId,
                 OldLocationId, NewLocationId, EffectiveDate)
            SELECT tr.EmployeeId, tr.FromDepartmentId, tr.ToDepartmentId,
                   tr.FromLocationId, tr.ToLocationId, GETDATE()
            FROM TransferRequests tr
            WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                // Update Employee record
                await conn.ExecuteAsync(@"
            UPDATE e
            SET e.DepartmentId = tr.ToDepartmentId,
                e.LocationId   = tr.ToLocationId
            FROM Employee e
            INNER JOIN TransferRequests tr ON tr.EmployeeId = e.EmployeeId
            WHERE tr.TransferRequestId = @Id;",
                    new { Id = requestId }, tx);

                tx.Commit();
                return letterPath;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static void GenerateTransferLetter(string outputPath, dynamic req, string hrComments)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            string employeeName = (string)req.FullName;
            string employeeCode = (string)req.EmployeeCode;
            string fromLocation = (string)req.FromLocationName;
            string toLocation = (string)req.TargetLocationName;
            string fromDept = (string)req.FromDeptName;
            string toDept = (string)req.TargetDeptName;
            string transferType = (string)req.TransferType ?? "Transfer";
            string grade = (string)req.Grade ?? "—";
            string company = (string)req.Company ?? "Larsen & Toubro Limited";
            string effectiveDate = DateTime.UtcNow.ToString("dd MMMM yyyy");
            string refNo = $"L&T/HR/TR/{employeeCode}/{DateTime.UtcNow:yyyyMMdd}";

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(company)
                                    .Bold().FontSize(14).FontColor("#1A1D2E");
                                c.Item().Text("Human Resources Department")
                                    .FontSize(9).FontColor("#8B90A7");
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text("TRANSFER LETTER")
                                    .Bold().FontSize(11).FontColor("#4F7EFF");
                                c.Item().Text(refNo)
                                    .FontSize(8).FontColor("#8B90A7");
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1.5f)
                            .LineColor("#4F7EFF");
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Date + Ref
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Date: {effectiveDate}")
                                .FontSize(9).FontColor("#8B90A7");
                            row.RelativeItem().AlignRight()
                                .Text($"Ref: {refNo}")
                                .FontSize(9).FontColor("#8B90A7");
                        });

                        col.Item().PaddingTop(16).Text("Dear " + employeeName + ",")
                            .FontSize(11).Bold();

                        col.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("Subject: ").Bold();
                            text.Span($"Transfer Order — {transferType}");
                        });

                        col.Item().PaddingTop(12).Text(
                            $"With reference to your transfer request, we are pleased to inform you that " +
                            $"your transfer has been approved by the Management. The details of your transfer are as follows:")
                            .FontSize(10).LineHeight(1.5f);

                        // Details table
                        col.Item().PaddingTop(16).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                            });

                            void AddRow(string label, string value, bool shaded = false)
                            {
                                var bg = shaded ? "#F4F6FB" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(8)
                                    .Text(label).Bold().FontSize(9).FontColor("#8B90A7");
                                table.Cell().Background(bg).Padding(8)
                                    .Text(value).FontSize(10).FontColor("#1A1D2E");
                            }

                            AddRow("Employee Name", employeeName, false);
                            AddRow("Employee Code", employeeCode, true);
                            AddRow("Grade", grade, false);
                            AddRow("Transfer Type", transferType, true);
                            AddRow("From Location", fromLocation, false);
                            AddRow("From Department", fromDept, true);
                            AddRow("To Location", toLocation, false);
                            AddRow("To Department", toDept, true);
                            AddRow("Effective Date", effectiveDate, false);
                        });

                        // HR Comments
                        if (!string.IsNullOrWhiteSpace(hrComments))
                        {
                            col.Item().PaddingTop(16).Background("#EEF2FF")
                                .Padding(12).Column(c =>
                                {
                                    c.Item().Text("HR Remarks").Bold().FontSize(9).FontColor("#4F7EFF");
                                    c.Item().PaddingTop(4).Text(hrComments).FontSize(10).LineHeight(1.4f);
                                });
                        }

                        col.Item().PaddingTop(20).Text(
                            "You are requested to report to your new place of posting on the effective date. " +
                            "Please ensure proper handover of your current responsibilities before relieving.")
                            .FontSize(10).LineHeight(1.5f);

                        col.Item().PaddingTop(8).Text(
                            "We wish you all the best in your new role.")
                            .FontSize(10);

                        // Signature block
                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("_______________________").FontColor("#8B90A7");
                                c.Item().PaddingTop(4).Text("HR Authorised Signatory").Bold().FontSize(9);
                                c.Item().Text(company).FontSize(8).FontColor("#8B90A7");
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("_______________________").FontColor("#8B90A7");
                                c.Item().PaddingTop(4).Text("Employee Acknowledgement").Bold().FontSize(9);
                                c.Item().Text("Signature & Date").FontSize(8).FontColor("#8B90A7");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("This is a system-generated transfer letter. | Page ")
                            .FontSize(8).FontColor("#8B90A7");
                        text.CurrentPageNumber().FontSize(8).FontColor("#8B90A7");
                        text.Span(" of ").FontSize(8).FontColor("#8B90A7");
                        text.TotalPages().FontSize(8).FontColor("#8B90A7");
                    });
                });
            }).GeneratePdf(outputPath);
        }
    }
}