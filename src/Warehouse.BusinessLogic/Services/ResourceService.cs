using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface IResourceService
    {
        Task<Resource> CreateAsync(Resource newResource);
        Task<Resource> GetByIdAsync(Guid id);
        Task<IEnumerable<Resource>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<Resource>> GetFilteredAsync(ResourceFilter filter);
        Task UpdateAsync(Guid id, Resource newResource);
        Task DeleteAsync(Guid id);
        Task ArchiveAsync(Guid id);
        Task UnarchiveAsync(Guid id);
    }

    public class ResourceService(IUnitOfWork unitOfWork) : IResourceService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<Resource> CreateAsync(Resource newResource)
        {
            var exist = await unitOfWork.Resources.GetAll();

            if (exist.Any(x => x.Name.Equals(newResource.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _log.Warn($"Duplicate resource '{newResource.Name}'.");
                throw new InvalidOperationException($"Ресурс с наименованием '{newResource.Name}' уже существует.");
            }

            await unitOfWork.Resources.Add(newResource);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created resource [Id={newResource.Id}].");
            return newResource;
        }

        public async Task<Resource> GetByIdAsync(Guid id)
        {
            var resource = await unitOfWork.Resources.Get(id);

            if (resource == null)
            {
                _log.Warn($"Resource not found [Id={id}].");
                throw new KeyNotFoundException($"Ресурс с идентификатором '{id}' не найден.");
            }

            return resource;
        }

        public async Task<IEnumerable<Resource>> GetAllAsync(bool includeArchived = false)
        {
            var filter = new ResourceFilter { IncludeArchived = includeArchived };
            return await unitOfWork.Resources.GetAllFiltered(filter);
        }

        public async Task<IEnumerable<Resource>> GetFilteredAsync(ResourceFilter filter)
        {
            return await unitOfWork.Resources.GetAllFiltered(filter);
        }

        public async Task UpdateAsync(Guid id, Resource newResource)
        {
            var old = await GetByIdAsync(id);

            if (!old.Name.Equals(newResource.Name, StringComparison.OrdinalIgnoreCase))
            {
                var all = await unitOfWork.Resources.GetAll();

                if (all.Any(x => x.Name.Equals(newResource.Name, StringComparison.OrdinalIgnoreCase) && x.Id != id))
                {
                    _log.Warn($"Rename to duplicate '{newResource.Name}'.");
                    throw new InvalidOperationException($"Ресурс с наименованием '{newResource.Name}' уже существует.");
                }
            }

            old.Name = newResource.Name;

            await unitOfWork.Resources.Update(old);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated resource [Id={id}].");
        }

        public async Task DeleteAsync(Guid id)
        {
            var resource = await GetByIdAsync(id);

            if (resource.IsDeleted)
            {
                _log.Warn($"Resource already deleted [Id={id}].");
                return;
            }

            var isInUse = await unitOfWork.Resources.IsInUseAsync(id);

            if (isInUse)
            {
                throw new InvalidOperationException(
                    "Нельзя удалить ресурс: он используется в документах. Переведите ресурс в архив.");
            }

            await unitOfWork.Resources.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Deleted resource [Id={id}].");
        }

        public async Task ArchiveAsync(Guid id)
        {
            var resource = await GetByIdAsync(id);

            if (resource.IsArchived)
            {
                _log.Warn($"Already archived [Id={id}].");
                return;
            }

            resource.IsArchived = true;
            resource.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Archived resource [Id={id}].");
        }

        public async Task UnarchiveAsync(Guid id)
        {
            var resource = await GetByIdAsync(id);

            if (!resource.IsArchived)
            {
                _log.Warn($"Resource already unarchived [Id={id}].");
                return;
            }

            resource.IsArchived = false;
            resource.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Unarchived resource [Id={id}].");
        }
    }
}
