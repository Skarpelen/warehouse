using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;
    using Warehouse.Shared.Filters;

    public class ClientRepository(WarehouseContext context)
        : GenericRepository<Client>(context), IClientRepository
    {
        public async Task<IEnumerable<Client>> GetAllFiltered(ClientFilter filter)
        {
            var query = _context.Clients.AsQueryable().Where(x => !x.IsDeleted);

            if (!filter.IncludeArchived)
            {
                query = query.Where(client => !client.IsArchived);
            }

            if (filter.Ids != null && filter.Ids.Any())
            {
                query = query.Where(client => filter.Ids.Contains(client.Id));
            }

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                var pattern = $"%{filter.NameContains.Trim()}%";
                query = query.Where(client => EF.Functions.Like(client.Name, pattern));
            }

            return await query.ToListAsync();
        }
    }
}
