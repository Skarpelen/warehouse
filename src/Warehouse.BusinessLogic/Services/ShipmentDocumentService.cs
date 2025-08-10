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
        Task DeleteAsync(Guid id);
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
                throw new InvalidOperationException($"Отгрузка с номером '{document.Number}' уже существует.");
            }

            if (document.Items == null || !document.Items.Any())
            {
                throw new InvalidOperationException("Отгрузка не может быть пустой.");
            }

            var client = await unitOfWork.Clients.Get(document.ClientId);

            if (client.IsArchived)
            {
                throw new InvalidOperationException($"Нельзя использовать архивированного клиента '{client.Name}'.");
            }

            foreach (var item in document.Items)
            {
                var res = await unitOfWork.Resources.Get(item.ResourceId);

                if (res.IsArchived)
                {
                    throw new InvalidOperationException($"Нельзя использовать архивированный ресурс '{res.Name}'.");
                }

                var unit = await unitOfWork.UnitsOfMeasure.Get(item.UnitOfMeasureId);

                if (unit.IsArchived)
                {
                    throw new InvalidOperationException($"Нельзя использовать архивированную единицу измерения '{unit.Name}'.");
                }
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
                throw new KeyNotFoundException($"Отгрузка '{id}' не найдена.");
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
            if (updated.Items == null || !updated.Items.Any())
            {
                throw new InvalidOperationException("Отгрузка не может быть пустой.");
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var doc = await unitOfWork.ShipmentDocuments.GetWithItemsAsync(id)
                          ?? throw new KeyNotFoundException($"Отгрузка '{id}' не найдена.");

                if (doc.Status == ShipmentStatus.Signed)
                {
                    throw new InvalidOperationException("Нельзя редактировать подписанную отгрузку.");
                }

                if (doc.ClientId != updated.ClientId)
                {
                    var client = await unitOfWork.Clients.Get(updated.ClientId);

                    if (client.IsArchived)
                    {
                        throw new InvalidOperationException($"Нельзя использовать архивированного клиента '{client.Name}'.");
                    }
                }

                var existingIds = doc.Items.Select(i => i.Id).ToHashSet();

                foreach (var item in updated.Items)
                {
                    var isNewLine = !existingIds.Contains(item.Id);
                    var changedRes = !isNewLine && doc.Items.First(i => i.Id == item.Id).ResourceId != item.ResourceId;
                    var changedUnit = !isNewLine && doc.Items.First(i => i.Id == item.Id).UnitOfMeasureId != item.UnitOfMeasureId;

                    if (isNewLine || changedRes || changedUnit)
                    {
                        var res = await unitOfWork.Resources.Get(item.ResourceId);

                        if (res.IsArchived)
                        {
                            throw new InvalidOperationException($"Нельзя использовать архивированный ресурс '{res.Name}'.");
                        }

                        var unit = await unitOfWork.UnitsOfMeasure.Get(item.UnitOfMeasureId);

                        if (unit.IsArchived)
                        {
                            throw new InvalidOperationException($"Нельзя использовать архивированную единицу измерения '{unit.Name}'.");
                        }
                    }
                }

                if (!doc.Number.Equals(updated.Number, StringComparison.OrdinalIgnoreCase))
                {
                    var all = await unitOfWork.ShipmentDocuments.GetAll();

                    if (all.Any(x => x.Number == updated.Number && x.Id != id))
                    {
                        _log.Warn($"Duplicate number '{updated.Number}'.");
                        throw new InvalidOperationException($"Отгрузка с номером '{updated.Number}' уже существует.");
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
                var adjustments = document.Items
                    .Select(i => new BalanceAdjustment(i.ResourceId, i.UnitOfMeasureId, -i.Quantity));

                await balanceService.ValidateBatchAsync(adjustments);
                await balanceService.AdjustBatchAsync(adjustments);
            }
            else if (newStatus == ShipmentStatus.Revoked)
            {
                if (document.Status != ShipmentStatus.Signed)
                {
                    throw new InvalidOperationException("Можно отозвать только подписанную отгрузку.");
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
                throw new InvalidOperationException("Недопустимое изменение статуса отгрузки.");
            }

            document.Status = newStatus;

            await unitOfWork.CompleteAsync();

            _log.Info($"Shipment status changed [Id={id}].");
        }

        public async Task DeleteAsync(Guid id)
        {
            await unitOfWork.ShipmentDocuments.SoftDelete(id);
            await unitOfWork.CompleteAsync();
            _log.Info($"Deleted shipment document [Id={id}].");
        }
    }
}
