using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class BalanceRepository(WarehouseContext context)
        : GenericRepository<Balance>(context), IBalanceRepository
    {
        public async Task<IEnumerable<Balance>> GetAllFiltered(BalanceFilter filter)
        {
            var query = _context.Balances.AsQueryable();

            if (filter.ResourceIds.Any())
            {
                query = query.Where(b => filter.ResourceIds.Contains(b.Resource.Id));
            }

            query = query.Where(x => !x.IsDeleted);

            if (filter.UnitIds.Any())
            {
                query = query.Where(b => filter.UnitIds.Contains(b.Unit.Id));
            }

            return await query.Include(b => b.Resource)
                .Include(b => b.Unit)
                .ToListAsync();
        }

        public Task<Balance?> GetByResourceAndUnitAsync(Guid resourceId, Guid unitId)
        {
            return _context.Balances
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitOfMeasureId == unitId);
        }
    }
}
