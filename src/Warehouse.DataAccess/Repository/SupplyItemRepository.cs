namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;

    public class SupplyItemRepository(WarehouseContext context)
        : GenericRepository<SupplyItem>(context), ISupplyItemRepository
    {
    }
}
