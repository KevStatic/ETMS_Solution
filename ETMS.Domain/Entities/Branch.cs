namespace ETMS.Domain.Entities
{
    public class Branch
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int LocationId { get; set; }
    }
}
