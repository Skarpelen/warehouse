namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface ISupplyDocumentRepository : IGenericRepository<SupplyDocument>
    {
        Task<IEnumerable<SupplyDocument>> GetAllFiltered(DocumentFilter filter);
        Task<SupplyDocument?> GetWithItemsAsync(Guid id);
        Task<SupplyDocument?> GetWithItemsNoTrackingAsync(Guid id);
    }
}
