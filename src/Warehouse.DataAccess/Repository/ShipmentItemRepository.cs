namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;

    public class ShipmentItemRepository(WarehouseContext context)
        : GenericRepository<ShipmentItem>(context), IShipmentItemRepository
    {
    }
}
