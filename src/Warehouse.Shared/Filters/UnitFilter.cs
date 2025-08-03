namespace Warehouse.Shared.Filters
{
    public class UnitFilter
    {
        public IEnumerable<Guid> Ids { get; set; } = Enumerable.Empty<Guid>();
        public string NameContains { get; set; } = string.Empty;
        public bool IncludeArchived { get; set; }
    }
}
