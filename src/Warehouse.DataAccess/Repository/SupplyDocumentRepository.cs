using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class SupplyDocumentRepository(WarehouseContext context)
        : GenericRepository<SupplyDocument>(context), ISupplyDocumentRepository
    {
        public async Task<IEnumerable<SupplyDocument>> GetAllFiltered(DocumentFilter filter)
        {
            var query = _context.SupplyDocuments
                .Include(d => d.Items).ThenInclude(i => i.Resource)
                .Include(d => d.Items).ThenInclude(i => i.Unit)
                .AsQueryable();

            if (filter.DateFrom.HasValue || filter.DateTo.HasValue)
            {
                var from = filter.DateFrom;
                var to = filter.DateTo;
                query = query.Where(d => (!from.HasValue || d.Date >= from.Value)
                                        && (!to.HasValue || d.Date <= to.Value));
            }

            if (filter.Numbers?.Any() == true)
            {
                query = query.Where(d => filter.Numbers.Contains(d.Number));
            }

            if (filter.ResourceIds?.Any() == true)
            {
                query = query.Where(d => d.Items.Any(i => filter.ResourceIds.Contains(i.ResourceId)));
            }

            if (filter.UnitIds?.Any() == true)
            {
                query = query.Where(d => d.Items.Any(i => filter.UnitIds.Contains(i.UnitOfMeasureId)));
            }

            return await query.ToListAsync();
        }

        public async Task<SupplyDocument?> GetWithItemsAsync(Guid id)
        {
            return await _context.SupplyDocuments
                .Include(d => d.Items).ThenInclude(i => i.Resource)
                .Include(d => d.Items).ThenInclude(i => i.Unit)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<SupplyDocument?> GetWithItemsNoTrackingAsync(Guid id)
        {
            return await _context.SupplyDocuments
                .Include(d => d.Items).ThenInclude(i => i.Resource)
                .Include(d => d.Items).ThenInclude(i => i.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }
    }
}
