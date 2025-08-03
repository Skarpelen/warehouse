namespace Warehouse.BusinessLogic.Repository
{
    using Warehouse.BusinessLogic.Models;

    public interface IGenericRepository<T> where T : BaseModel
    {
        Task<IEnumerable<T>> GetAll(bool includeDeleted = false);
        Task<IEnumerable<T>> GetAllByIds(IEnumerable<Guid> ids, bool includeDeleted = false);
        Task<T?> Get(Guid id, bool includeDeleted = false);
        Task Add(T entity);
        Task Update(T entity);
        Task SoftDelete(Guid id);
        Task HardDelete(Guid id);
        Task<bool> Exists(Guid id, bool includeDeleted = false);
    }
}
