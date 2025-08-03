using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared;
    using Warehouse.Shared.Filters;

    public interface IClientService
    {
        Task<Client> CreateAsync(Client newClient);
        Task<Client> GetByIdAsync(Guid id);
        Task<IEnumerable<Client>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<Client>> GetFilteredAsync(ClientFilter filter);
        Task UpdateAsync(Guid id, Client newClient);
        Task ArchiveAsync(Guid id);
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
            existing.IsDeleted = newClient.IsDeleted;

            await unitOfWork.Clients.Update(existing);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated client [Id={id}].");
        }

        public async Task ArchiveAsync(Guid id)
        {
            var client = await GetByIdAsync(id);

            if (client.IsDeleted)
            {
                _log.Warn($"Client already archived [Id={id}].");
                return;
            }

            await unitOfWork.Clients.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Archived client [Id={id}].");
        }
    }
}
