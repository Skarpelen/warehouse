namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface IBalanceRepository : IGenericRepository<Balance>
    {
        Task<IEnumerable<Balance>> GetAllFiltered(BalanceFilter filter);
        Task<Balance?> GetByResourceAndUnitAsync(Guid resourceId, Guid unitId);
    }
}
