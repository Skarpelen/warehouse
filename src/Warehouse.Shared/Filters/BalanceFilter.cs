namespace Warehouse.Shared.Filters
{
    public class BalanceFilter
    {
        public List<Guid> ResourceIds { get; set; } = new();
        public List<Guid> UnitIds { get; set; } = new();
    }
}
