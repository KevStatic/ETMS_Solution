namespace ETMS.Web.Models
{
    public class DashboardRequestItem
    {
        public int TransferRequestId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string FromDepartment { get; set; } = string.Empty;

        public string TargetLocation { get; set; } = string.Empty;

        public string TargetDepartment { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }

        public string Status { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public string StageLabel => Status switch
        {
            "Approved" => "Letter Ready",
            "Rejected" => "Rejected",
            "Cancelled" => "Cancelled",
            "HODApproved" => "Awaiting HR",
            "ManagerApproved" => "Awaiting HOD",
            _ => "Awaiting Manager"
        };
    }
}
