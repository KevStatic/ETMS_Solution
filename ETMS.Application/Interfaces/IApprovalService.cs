namespace ETMS.Application.Interfaces
{
    public interface IApprovalService
    {
        Task<bool> ProcessApprovalAsync(
            int requestId, int approverId, string approverRole,
            string decision, string comments);

        Task<string> FinaliseAndGenerateLetterAsync(
            int requestId, int hrEmployeeId, string comments, string wwwRootPath);
    }
}