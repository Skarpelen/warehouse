namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;

    public interface ISupplyItemRepository : IGenericRepository<SupplyItem>
    {
        Task AddRangeAsync(IEnumerable<SupplyItem> items);
    }
}
