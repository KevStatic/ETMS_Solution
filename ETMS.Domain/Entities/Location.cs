namespace ETMS.Domain.Entities
{
    public class Location
    {
        public int LocationId { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
