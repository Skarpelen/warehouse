namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface IResourceRepository : IGenericRepository<Resource>
    {
        Task<IEnumerable<Resource>> GetAllFiltered(ResourceFilter filter);
        Task<bool> IsInUseAsync(Guid resourceId);
    }
}
