namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<IEnumerable<Client>> GetAllFiltered(ClientFilter filter);
    }
}
