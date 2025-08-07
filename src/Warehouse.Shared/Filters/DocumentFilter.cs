namespace Warehouse.Shared.Filters
{
    public class DocumentFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> Numbers { get; set; } = new();
        public List<Guid> ResourceIds { get; set; } = new();
        public List<Guid> UnitIds { get; set; } = new();
    }
}
