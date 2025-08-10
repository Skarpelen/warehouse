using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface ISupplyDocumentService
    {
        Task<SupplyDocument> CreateAsync(SupplyDocument doc);
        Task<SupplyDocument> GetByIdAsync(Guid id, bool shouldTrack = true);
        Task<IEnumerable<SupplyDocument>> GetAllAsync();
        Task<IEnumerable<SupplyDocument>> GetFilteredAsync(DocumentFilter filter);
        Task UpdateAsync(Guid id, SupplyDocument doc);
        Task DeleteAsync(Guid id);
    }

    public class SupplyDocumentService(IUnitOfWork unitOfWork, IBalanceService balanceService) : ISupplyDocumentService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<SupplyDocument> CreateAsync(SupplyDocument doc)
        {
            doc.Items ??= Enumerable.Empty<SupplyItem>().ToList();

            foreach (var item in doc.Items)
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

            var exists = await unitOfWork.SupplyDocuments.GetAll();

            if (exists.Any(x => x.Number == doc.Number))
            {
                _log.Warn($"Duplicate supply number '{doc.Number}'.");
                throw new InvalidOperationException($"Поступление с номером '{doc.Number}' уже существует.");
            }

            await unitOfWork.SupplyDocuments.Add(doc);

            var adds = doc.Items
               .Select(i => new BalanceAdjustment(
                   i.ResourceId,
                   i.UnitOfMeasureId,
                   i.Quantity));

            await balanceService.AdjustBatchAsync(adds);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created supply doc [Id={doc.Id}].");
            return doc;
        }

        public async Task<SupplyDocument> GetByIdAsync(Guid id, bool shouldTrack = true)
        {
            SupplyDocument? document;

            if (shouldTrack)
            {
                document = await unitOfWork.SupplyDocuments.GetWithItemsAsync(id);
            }
            else
            {
                document = await unitOfWork.SupplyDocuments.GetWithItemsNoTrackingAsync(id);
            }

            if (document == null)
            {
                _log.Warn($"Supply doc not found [Id={id}].");
                throw new KeyNotFoundException($"Поступление '{id}' не найдено.");
            }

            return document;
        }

        public async Task<IEnumerable<SupplyDocument>> GetAllAsync()
        {
            return await unitOfWork.SupplyDocuments.GetAll();
        }

        public async Task<IEnumerable<SupplyDocument>> GetFilteredAsync(DocumentFilter filter)
        {
            return await unitOfWork.SupplyDocuments.GetAllFiltered(filter);
        }

        public async Task UpdateAsync(Guid id, SupplyDocument updated)
        {
            var allDocs = await unitOfWork.SupplyDocuments.GetAll();

            if (allDocs.Any(d => d.Number == updated.Number && d.Id != id))
            {
                _log.Warn($"Duplicate supply number '{updated.Number}'.");
                throw new InvalidOperationException($"Поступление с номером '{updated.Number}' уже существует.");
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var doc = await unitOfWork.SupplyDocuments.GetWithItemsAsync(id)
                          ?? throw new KeyNotFoundException($"Поступление '{id}' не найдено.");

                doc.Number = updated.Number;
                doc.Date = updated.Date;

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

                await unitOfWork.CompleteAsync();

                var toRemove = doc.Items
                   .Where(it => !updated.Items.Any(u => u.Id == it.Id))
                   .ToList();

                var adjustments = new List<BalanceAdjustment>();

                foreach (var oldItem in toRemove)
                {
                    adjustments.Add(new BalanceAdjustment(
                        oldItem.ResourceId,
                        oldItem.UnitOfMeasureId,
                        -oldItem.Quantity));

                    await unitOfWork.SupplyItems.HardDelete(oldItem.Id);
                }

                foreach (var newItem in updated.Items)
                {
                    var existing = doc.Items.FirstOrDefault(it => it.Id == newItem.Id);

                    if (existing == null)
                    {
                        var ni = new SupplyItem
                        {
                            Id = Guid.NewGuid(),
                            SupplyDocumentId = id,
                            ResourceId = newItem.ResourceId,
                            UnitOfMeasureId = newItem.UnitOfMeasureId,
                            Quantity = newItem.Quantity
                        };

                        adjustments.Add(new BalanceAdjustment(
                            ni.ResourceId,
                            ni.UnitOfMeasureId,
                            ni.Quantity));
                    }
                    else
                    {
                        var delta = newItem.Quantity - existing.Quantity;

                        if (delta != 0)
                        {
                            adjustments.Add(new BalanceAdjustment(
                                existing.ResourceId,
                                existing.UnitOfMeasureId,
                                delta));

                            existing.Quantity = newItem.Quantity;
                            existing.ResourceId = newItem.ResourceId;
                            existing.UnitOfMeasureId = newItem.UnitOfMeasureId;
                        }
                    }
                }

                await unitOfWork.CompleteAsync();

                var adds = updated.Items.Select(i => new SupplyItem
                {
                    Id = Guid.NewGuid(),
                    SupplyDocumentId = id,
                    ResourceId = i.ResourceId,
                    UnitOfMeasureId = i.UnitOfMeasureId,
                    Quantity = i.Quantity
                });

                await unitOfWork.SupplyItems.AddRangeAsync(adds);
                await unitOfWork.CompleteAsync();

                await balanceService.ValidateBatchAsync(adjustments);
                await balanceService.AdjustBatchAsync(adjustments);

                await unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }

            _log.Info($"Updated supply [Id={id}] with balance adjustments.");
        }

        public async Task DeleteAsync(Guid id)
        {
            var document = await GetByIdAsync(id);
            var items = document.Items ?? Enumerable.Empty<SupplyItem>();

            var balances = await unitOfWork.Balances.GetAll();

            var insuff = balances
                .Where(b => items.Any(i =>
                    i.ResourceId == b.ResourceId
                    && i.UnitOfMeasureId == b.UnitOfMeasureId
                    && b.Quantity < i.Quantity))
                .ToList();

            if (insuff.Any())
            {
                var b = insuff.First();
                var needItem = items.First(i => i.ResourceId == b.ResourceId && i.UnitOfMeasureId == b.UnitOfMeasureId);

                var res = await unitOfWork.Resources.Get(b.ResourceId);
                var unit = await unitOfWork.UnitsOfMeasure.Get(b.UnitOfMeasureId);

                var resName = res?.Name ?? b.ResourceId.ToString();
                var unitName = unit?.Name ?? b.UnitOfMeasureId.ToString();

                throw new InvalidOperationException(
                    $"Недостаточно остатков для удаления поступления: ресурс '{resName}', ед. изм. '{unitName}'. " +
                    $"Доступно {b.Quantity}, требуется {needItem.Quantity}.");
            }

            var removes = document.Items
                .Select(i => new BalanceAdjustment(
                    i.ResourceId,
                    i.UnitOfMeasureId,
                    -i.Quantity));

            await balanceService.AdjustBatchAsync(removes);

            await unitOfWork.SupplyDocuments.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Deleted supply document [Id={id}].");
        }
    }
}
