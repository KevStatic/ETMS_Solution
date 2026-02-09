namespace ETMS.Domain.Entities
{
    public class Designation
    {
        public int DesignationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
}
