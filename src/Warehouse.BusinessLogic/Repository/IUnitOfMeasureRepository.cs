namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface IUnitOfMeasureRepository : IGenericRepository<UnitOfMeasure>
    {
        Task<IEnumerable<UnitOfMeasure>> GetAllFiltered(UnitFilter filter);
        Task<bool> IsInUseAsync(Guid unitId);
    }
}
