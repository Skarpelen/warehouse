namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.Filters;

    public interface IShipmentDocumentRepository : IGenericRepository<ShipmentDocument>
    {
        Task<IEnumerable<ShipmentDocument>> GetAllFiltered(DocumentFilter filter);
        Task<ShipmentDocument?> GetWithItemsAsync(Guid id);
    }
}
