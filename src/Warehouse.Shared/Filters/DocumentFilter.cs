namespace Warehouse.Shared.Filters
{
    public class DocumentFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public IEnumerable<string> Numbers { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<Guid> ResourceIds { get; set; } = Enumerable.Empty<Guid>();
        public IEnumerable<Guid> UnitIds { get; set; } = Enumerable.Empty<Guid>();
    }
}
