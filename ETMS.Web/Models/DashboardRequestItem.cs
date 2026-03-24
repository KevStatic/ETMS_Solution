namespace ETMS.Web.Models
{
    public class DashboardRequestItem
    {
        public int TransferRequestId { get; set; }

        public string TargetLocation { get; set; } = string.Empty;

        public string TargetDepartment { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}