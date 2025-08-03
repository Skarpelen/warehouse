using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;

    public abstract class GenericRepository<T>(WarehouseContext context) : IGenericRepository<T>
        where T : BaseModel
    {
        protected readonly WarehouseContext _context = context;

        public async Task<IEnumerable<T>> GetAll(bool includeDeleted = false)
        {
            return includeDeleted
                ? await _context.Set<T>().ToListAsync()
                : await _context.Set<T>().Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllByIds(IEnumerable<Guid> ids, bool includeDeleted = false)
        {
            var idSet = ids.ToHashSet();
            return await _context.Set<T>().Where(e => (includeDeleted || !e.IsDeleted) && idSet.Contains(e.Id)).ToListAsync();
        }

        public async Task<T?> Get(Guid id, bool includeDeleted = false)
        {
            var entity = await _context.Set<T>().FindAsync(id);

            if (entity == null)
            {
                return null;
            }

            if (includeDeleted)
            {
                return entity;
            }

            return entity.IsDeleted == false ? entity : null;
        }

        public async Task Add(T entity)
        {
            entity.MarkCreated();
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            entity.MarkUpdated();
            _context.Set<T>().Update(entity);
        }

        public async Task SoftDelete(Guid id)
        {
            var entity = await Get(id);

            if (entity == null)
            {
                return;
            }

            entity.Delete();
            await Update(entity);
        }

        public async Task HardDelete(Guid id)
        {
            var entity = await Get(id);

            if (entity == null)
            {
                return;
            }

            _context.Set<T>().Remove(entity);
        }

        public Task<bool> Exists(Guid id, bool includeDeleted = false)
        {
            return includeDeleted
                ? _context.Set<T>().AnyAsync(e => e.Id == id)
                : _context.Set<T>().Where(e => !e.IsDeleted).AnyAsync(e => e.Id == id);
        }
    }
}
