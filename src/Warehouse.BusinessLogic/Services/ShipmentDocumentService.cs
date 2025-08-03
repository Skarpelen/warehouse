using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared;
    using Warehouse.Shared.Filters;

    public interface IShipmentDocumentService
    {
        Task<ShipmentDocument> CreateAsync(ShipmentDocument document);
        Task<ShipmentDocument> GetByIdAsync(Guid id);
        Task<IEnumerable<ShipmentDocument>> GetAllAsync();
        Task<IEnumerable<ShipmentDocument>> GetFilteredAsync(DocumentFilter filter);
        Task UpdateAsync(Guid id, ShipmentDocument document);
        Task ChangeStatusAsync(Guid id, ShipmentStatus newStatus);
    }

    public class ShipmentDocumentService(IUnitOfWork unitOfWork) : IShipmentDocumentService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<ShipmentDocument> CreateAsync(ShipmentDocument document)
        {
            var exist = await unitOfWork.ShipmentDocuments.GetAll();

            if (exist.Any(x => x.Number == document.Number))
            {
                _log.Warn($"Duplicate shipment number '{document.Number}'.");
                throw new InvalidOperationException($"Number '{document.Number}' already exists.");
            }

            if (document.Items == null || !document.Items.Any())
            {
                throw new InvalidOperationException("Shipment cannot be empty.");
            }

            await unitOfWork.ShipmentDocuments.Add(document);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created shipment [Id={document.Id}].");
            return document;
        }

        public async Task<ShipmentDocument> GetByIdAsync(Guid id)
        {
            var document = await unitOfWork.ShipmentDocuments.GetWithItemsAsync(id);

            if (document == null)
            {
                _log.Warn($"Shipment not found [Id={id}].");
                throw new KeyNotFoundException($"Shipment '{id}' not found.");
            }

            return document;
        }

        public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
        {
            return await unitOfWork.ShipmentDocuments.GetAll();
        }

        public async Task<IEnumerable<ShipmentDocument>> GetFilteredAsync(DocumentFilter filter)
        {
            return await unitOfWork.ShipmentDocuments.GetAllFiltered(filter);
        }

        public async Task UpdateAsync(Guid id, ShipmentDocument document)
        {
            var old = await GetByIdAsync(id);

            if (!old.Number.Equals(document.Number, StringComparison.OrdinalIgnoreCase))
            {
                var all = await unitOfWork.ShipmentDocuments.GetAll();

                if (all.Any(x => x.Number == document.Number && x.Id != id))
                {
                    _log.Warn($"Duplicate number '{document.Number}'.");
                    throw new InvalidOperationException($"Number '{document.Number}' already exists.");
                }
            }

            old.Number = document.Number;
            old.Date = document.Date;
            old.ClientId = document.ClientId;
            old.Items = document.Items;

            await unitOfWork.ShipmentDocuments.Update(old);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated shipment [Id={id}].");
        }

        public async Task ChangeStatusAsync(Guid id, ShipmentStatus newStatus)
        {
            var document = await GetByIdAsync(id);

            if (document.Status == newStatus)
            {
                return;
            }

            if (newStatus == ShipmentStatus.Signed)
            {
                await AdjustBalancesAsync(document.Items, decrease: true);
            }
            else if (newStatus == ShipmentStatus.Revoked)
            {
                if (document.Status != ShipmentStatus.Signed)
                {
                    throw new InvalidOperationException("Can revoke only signed.");
                }
                await AdjustBalancesAsync(document.Items, decrease: false);
            }
            else
            {
                throw new InvalidOperationException("Invalid status transition.");
            }

            document.Status = newStatus;

            await unitOfWork.ShipmentDocuments.Update(document);
            await unitOfWork.CompleteAsync();
        }

        private async Task AdjustBalancesAsync(IEnumerable<ShipmentItem> items, bool decrease)
        {
            foreach (var item in items)
            {
                var balance = await unitOfWork.Balances.GetByResourceAndUnitAsync(item.ResourceId, item.UnitOfMeasureId);

                if (balance == null || balance.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock.");
                }

                balance.Quantity += decrease ? -item.Quantity : item.Quantity;
                await unitOfWork.Balances.Update(balance);
            }
        }
    }
}
