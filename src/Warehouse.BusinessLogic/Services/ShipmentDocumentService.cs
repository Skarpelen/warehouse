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
        Task<ShipmentDocument> GetByIdAsync(Guid id, bool shouldTrack = true);
        Task<IEnumerable<ShipmentDocument>> GetAllAsync();
        Task<IEnumerable<ShipmentDocument>> GetFilteredAsync(DocumentFilter filter);
        Task UpdateAsync(Guid id, ShipmentDocument document);
        Task ChangeStatusAsync(Guid id, ShipmentStatus newStatus);
    }

    public class ShipmentDocumentService(IUnitOfWork unitOfWork, IBalanceService balanceService) : IShipmentDocumentService
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

        public async Task<ShipmentDocument> GetByIdAsync(Guid id, bool shouldTrack = true)
        {
            ShipmentDocument? document;

            if (shouldTrack)
            {
                document = await unitOfWork.ShipmentDocuments.GetWithItemsAsync(id);
            }
            else
            {
                document = await unitOfWork.ShipmentDocuments.GetWithItemsNoTrackingAsync(id);
            }

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

        public async Task UpdateAsync(Guid id, ShipmentDocument updated)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                var doc = await unitOfWork.ShipmentDocuments.GetWithItemsAsync(id)
                          ?? throw new KeyNotFoundException($"Shipment '{id}' not found.");

                if (!doc.Number.Equals(updated.Number, StringComparison.OrdinalIgnoreCase))
                {
                    var all = await unitOfWork.ShipmentDocuments.GetAll();

                    if (all.Any(x => x.Number == updated.Number && x.Id != id))
                    {
                        _log.Warn($"Duplicate number '{updated.Number}'.");
                        throw new InvalidOperationException($"Number '{updated.Number}' already exists.");
                    }
                }

                doc.Number = updated.Number;
                doc.Date = updated.Date;
                doc.ClientId = updated.ClientId;
                await unitOfWork.CompleteAsync();

                var toRemove = doc.Items
                    .Where(oldItem => !updated.Items.Any(newItem => newItem.Id == oldItem.Id))
                    .ToList();

                foreach (var oldItem in toRemove)
                {
                    await unitOfWork.ShipmentItems.HardDelete(oldItem.Id);
                }

                var toAdd = updated.Items
                    .Where(newItem => newItem.Id == Guid.Empty)
                    .ToList();

                foreach (var newItem in toAdd)
                {
                    var ni = new ShipmentItem
                    {
                        Id = Guid.NewGuid(),
                        ShipmentDocumentId = id,
                        ResourceId = newItem.ResourceId,
                        UnitOfMeasureId = newItem.UnitOfMeasureId,
                        Quantity = newItem.Quantity
                    };
                    await unitOfWork.ShipmentItems.Add(ni);
                }

                foreach (var newItem in updated.Items.Where(i => i.Id != Guid.Empty))
                {
                    var existingItem = doc.Items.FirstOrDefault(i => i.Id == newItem.Id);

                    if (existingItem != null)
                    {
                        existingItem.ResourceId = newItem.ResourceId;
                        existingItem.UnitOfMeasureId = newItem.UnitOfMeasureId;
                        existingItem.Quantity = newItem.Quantity;
                    }
                }

                await unitOfWork.CompleteAsync();

                await unitOfWork.CommitTransactionAsync();

                _log.Info($"Updated shipment [Id={id}] with item changes.");
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
                var adjusts = document.Items
                    .Select(i => new BalanceAdjustment(
                        i.ResourceId,
                        i.UnitOfMeasureId,
                        -i.Quantity));
                await balanceService.AdjustBatchAsync(adjusts);
            }
            else if (newStatus == ShipmentStatus.Revoked)
            {
                if (document.Status != ShipmentStatus.Signed)
                {
                    throw new InvalidOperationException(
                        "Can revoke only signed.");
                }

                var adjusts = document.Items
                    .Select(i => new BalanceAdjustment(
                        i.ResourceId,
                        i.UnitOfMeasureId,
                        i.Quantity));
                await balanceService.AdjustBatchAsync(adjusts);
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid status transition.");
            }

            document.Status = newStatus;

            //await unitOfWork.ShipmentDocuments.Update(document);
            await unitOfWork.CompleteAsync();

            _log.Info($"Shipment status changed [Id={id}].");
        }
    }
}
