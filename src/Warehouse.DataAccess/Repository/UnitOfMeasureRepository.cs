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
            var query = _context.UnitOfMeasures.AsQueryable();

            if (!filter.IncludeArchived)
            {
                query = query.Where(x => !x.IsDeleted);
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
    }
}
