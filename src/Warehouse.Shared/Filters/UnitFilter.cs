namespace Warehouse.Shared.Filters
{
    public class UnitFilter
    {
        public List<Guid> Ids { get; set; } = new();
        public string NameContains { get; set; } = string.Empty;
        public bool IncludeArchived { get; set; }
    }
}
