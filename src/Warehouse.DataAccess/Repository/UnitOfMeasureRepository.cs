using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class UnitOfMeasureRepository(WarehouseContext context)
        : GenericRepository<UnitOfMeasure>(context), IUnitOfMeasureRepository
    {
        public async Task<IEnumerable<UnitOfMeasure>> GetAllFiltered(UnitFilter filter)
        {
            var query = _context.UnitOfMeasures.AsQueryable().Where(x => !x.IsDeleted);

            if (!filter.IncludeArchived)
            {
                query = query.Where(x => !x.IsArchived);
            }

            if (filter.Ids.Any())
            {
                query = query.Where(x => filter.Ids.Contains(x.Id));
            }

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                var pattern = $"%{filter.NameContains.Trim()}%";
                query = query.Where(x => EF.Functions.Like(x.Name, pattern));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> IsInUseAsync(Guid unitId)
        {
            var usageIds = _context.Balances
                .Where(b => b.UnitOfMeasureId == unitId)
                .Select(b => b.Id)
                .Concat(
                    _context.SupplyItems
                        .Where(si => si.UnitOfMeasureId == unitId)
                        .Select(si => si.Id)
                )
                .Concat(
                    _context.ShipmentItems
                        .Where(si => si.UnitOfMeasureId == unitId)
                        .Select(si => si.Id)
                );

            return await usageIds.AnyAsync();
        }
    }
}
