namespace ETMS.Domain.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? HeadOfDepartmentId { get; set; }
    }
}
