namespace Warehouse.Shared.Filters
{
    public class BalanceFilter
    {
        public IEnumerable<Guid> ResourceIds { get; set; } = Enumerable.Empty<Guid>();
        public IEnumerable<Guid> UnitIds { get; set; } = Enumerable.Empty<Guid>();
    }
}
