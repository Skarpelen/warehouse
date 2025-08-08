using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface IClientService
    {
        Task<Client> CreateAsync(Client newClient);
        Task<Client> GetByIdAsync(Guid id);
        Task<IEnumerable<Client>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<Client>> GetFilteredAsync(ClientFilter filter);
        Task UpdateAsync(Guid id, Client newClient);
        Task DeleteAsync(Guid id);
        Task ArchiveAsync(Guid id);
        Task UnarchiveAsync(Guid id);
    }

    public class ClientService(IUnitOfWork unitOfWork) : IClientService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<Client> CreateAsync(Client newClient)
        {
            var clients = await unitOfWork.Clients.GetAll();

            if (clients.Any(c => c.Name.Equals(newClient.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _log.Warn($"Attempt to create duplicate client with name '{newClient.Name}'.");
                throw new InvalidOperationException($"Client with name '{newClient.Name}' already exists.");
            }

            await unitOfWork.Clients.Add(newClient);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created client [Id={newClient.Id}, Name='{newClient.Name}'].");

            return newClient;
        }

        public async Task<Client> GetByIdAsync(Guid id)
        {
            var client = await unitOfWork.Clients.Get(id);

            if (client == null)
            {
                _log.Warn($"Client not found [Id={id}].");
                throw new KeyNotFoundException($"Client with id '{id}' not found.");
            }

            return client;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(bool includeArchived = false)
        {
            var filter = new ClientFilter { IncludeArchived = includeArchived };
            var clients = await unitOfWork.Clients.GetAllFiltered(filter);
            return clients;
        }

        public async Task<IEnumerable<Client>> GetFilteredAsync(ClientFilter filter)
        {
            var clients = await unitOfWork.Clients.GetAllFiltered(filter);
            return clients;
        }

        public async Task UpdateAsync(Guid id, Client newClient)
        {
            var existing = await GetByIdAsync(id);

            if (!existing.Name.Equals(newClient.Name, StringComparison.OrdinalIgnoreCase))
            {
                var clients = await unitOfWork.Clients.GetAll();

                if (clients.Any(c => c.Name.Equals(newClient.Name, StringComparison.OrdinalIgnoreCase) && c.Id != id))
                {
                    _log.Warn($"Attempt to rename client to duplicate name '{newClient.Name}'.");
                    throw new InvalidOperationException($"Client with name '{newClient.Name}' already exists.");
                }
            }

            existing.Name = newClient.Name;
            existing.Address = newClient.Address;

            await unitOfWork.Clients.Update(existing);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated client [Id={id}].");
        }

        public async Task DeleteAsync(Guid id)
        {
            var client = await GetByIdAsync(id);

            if (client.IsDeleted)
            {
                _log.Warn($"Client already deleted [Id={id}].");
                return;
            }

            var isInUse = await unitOfWork.Clients.IsInUseAsync(id);

            if (isInUse)
            {
                throw new InvalidOperationException(
                    "Client is in use and cannot be deleted. Consider archiving instead.");
            }

            await unitOfWork.Clients.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Deleted client [Id={id}].");
        }

        public async Task ArchiveAsync(Guid id)
        {
            var client = await GetByIdAsync(id);

            if (client.IsArchived)
            {
                _log.Warn($"Client already archived [Id={id}].");
                return;
            }

            client.IsArchived = true;
            client.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Archived client [Id={id}].");
        }

        public async Task UnarchiveAsync(Guid id)
        {
            var client = await GetByIdAsync(id);

            if (!client.IsArchived)
            {
                _log.Warn($"Client already unarchived [Id={id}].");
                return;
            }

            client.IsArchived = false;
            client.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Unarchived client [Id={id}].");
        }
    }
}
