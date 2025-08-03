using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class ShipmentDocumentRepository(WarehouseContext context)
        : GenericRepository<ShipmentDocument>(context), IShipmentDocumentRepository
    {
        public async Task<IEnumerable<ShipmentDocument>> GetAllFiltered(DocumentFilter filter)
        {
            var q = _context.ShipmentDocuments
                .Include(d => d.Client)
                .Include(d => d.Items).ThenInclude(i => i.Resource)
                .Include(d => d.Items).ThenInclude(i => i.Unit)
                .AsQueryable();

            var hasDate = filter.DateFrom.HasValue || filter.DateTo.HasValue;
            var hasNumber = filter.Numbers?.Any() == true;
            var hasRes = filter.ResourceIds?.Any() == true;
            var hasUnit = filter.UnitIds?.Any() == true;

            if (!hasDate && !hasNumber && !hasRes && !hasUnit)
            {
                return await q.ToListAsync();
            }

            q = q.Where(d =>
                // 1) попадает в период (только если период задан)
                (hasDate
                    && (!filter.DateFrom.HasValue || d.Date >= filter.DateFrom.Value)
                    && (!filter.DateTo.HasValue || d.Date <= filter.DateTo.Value))
                // 2) или номер документа
                || (hasNumber && filter.Numbers.Contains(d.Number))
                // 3) или есть хотя бы один ресурс из фильтра
                || (hasRes && d.Items.Any(i => filter.ResourceIds.Contains(i.ResourceId)))
                // 4) или есть хотя бы одна единица из фильтра
                || (hasUnit && d.Items.Any(i => filter.UnitIds.Contains(i.UnitOfMeasureId)))
            );

            return await q.ToListAsync();
        }

        public async Task<ShipmentDocument?> GetWithItemsAsync(Guid id)
        {
            return await _context.ShipmentDocuments
                .Include(d => d.Client)
                .Include(d => d.Items).ThenInclude(i => i.Resource)
                .Include(d => d.Items).ThenInclude(i => i.Unit)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }
    }
}
