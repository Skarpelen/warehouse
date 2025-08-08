using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class ResourceRepository(WarehouseContext context)
        : GenericRepository<Resource>(context), IResourceRepository
    {
        public async Task<IEnumerable<Resource>> GetAllFiltered(ResourceFilter filter)
        {
            var query = _context.Resources.AsQueryable().Where(x => !x.IsDeleted);

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

        public async Task<bool> IsInUseAsync(Guid resourceId)
        {
            var usageIds = _context.Balances
                .Where(b => b.ResourceId == resourceId)
                .Select(b => b.Id)
                .Concat(
                    _context.SupplyItems
                        .Where(si => si.ResourceId == resourceId)
                        .Select(si => si.Id)
                )
                .Concat(
                    _context.ShipmentItems
                        .Where(si => si.ResourceId == resourceId)
                        .Select(si => si.Id)
                );

            return await usageIds.AnyAsync();
        }
    }
}
