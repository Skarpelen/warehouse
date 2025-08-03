using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface ISupplyDocumentService
    {
        Task<SupplyDocument> CreateAsync(SupplyDocument doc);
        Task<SupplyDocument> GetByIdAsync(Guid id);
        Task<IEnumerable<SupplyDocument>> GetAllAsync();
        Task<IEnumerable<SupplyDocument>> GetFilteredAsync(DocumentFilter filter);
        Task UpdateAsync(Guid id, SupplyDocument doc);
        Task DeleteAsync(Guid id);
    }

    public class SupplyDocumentService(IUnitOfWork unitOfWork) : ISupplyDocumentService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<SupplyDocument> CreateAsync(SupplyDocument doc)
        {
            doc.Items ??= Enumerable.Empty<SupplyItem>().ToList();

            var exists = await unitOfWork.SupplyDocuments.GetAll();

            if (exists.Any(x => x.Number == doc.Number))
            {
                _log.Warn($"Duplicate supply number '{doc.Number}'.");
                throw new InvalidOperationException($"Number '{doc.Number}' already exists.");
            }

            await unitOfWork.SupplyDocuments.Add(doc);
            await AdjustBalancesAsync(doc.Items, increase: true);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created supply doc [Id={doc.Id}].");
            return doc;
        }

        public async Task<SupplyDocument> GetByIdAsync(Guid id)
        {
            var document = await unitOfWork.SupplyDocuments.GetWithItemsAsync(id);

            if (document == null)
            {
                _log.Warn($"Supply doc not found [Id={id}].");
                throw new KeyNotFoundException($"Supply '{id}' not found.");
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

        public async Task UpdateAsync(Guid id, SupplyDocument doc)
        {
            var old = await GetByIdAsync(id);

            if (!old.Number.Equals(doc.Number, StringComparison.OrdinalIgnoreCase))
            {
                var all = await unitOfWork.SupplyDocuments.GetAll();

                if (all.Any(x => x.Number == doc.Number && x.Id != id))
                {
                    _log.Warn($"Duplicate number '{doc.Number}'.");
                    throw new InvalidOperationException($"Number '{doc.Number}' already exists.");
                }
            }

            await AdjustBalancesAsync(old.Items, increase: false);

            old.Number = doc.Number;
            old.Date = doc.Date;
            old.Items = doc.Items;
            await AdjustBalancesAsync(old.Items, increase: true);

            await unitOfWork.SupplyDocuments.Update(old);
            await unitOfWork.CompleteAsync();
            _log.Info($"Updated supply doc [Id={id}].");
        }

        public async Task DeleteAsync(Guid id)
        {
            var document = await GetByIdAsync(id);
            var items = document.Items ?? Enumerable.Empty<SupplyItem>();

            var insuff = (await unitOfWork.Balances.GetAll())
                .Where(b => items.Any(i => i.ResourceId == b.ResourceId
                                         && i.UnitOfMeasureId == b.UnitOfMeasureId
                                         && b.Quantity < i.Quantity))
                .ToList();

            if (insuff.Any())
            {
                throw new InvalidOperationException("Insufficient stock to delete supply.");
            }

            await AdjustBalancesAsync(document.Items, increase: false);
            await unitOfWork.SupplyDocuments.SoftDelete(id);
            await unitOfWork.CompleteAsync();
            _log.Info($"Deleted supply document [Id={id}].");
        }

        private async Task AdjustBalancesAsync(IEnumerable<SupplyItem> items, bool increase)
        {
            foreach (var item in items)
            {
                var balance = await unitOfWork.Balances.GetByResourceAndUnitAsync(item.ResourceId, item.UnitOfMeasureId);

                if (balance == null)
                {
                    if (!increase)
                    {
                        throw new InvalidOperationException("Balance entry missing on adjust.");
                    }

                    balance = new Balance
                    {
                        ResourceId = item.ResourceId,
                        UnitOfMeasureId = item.UnitOfMeasureId,
                        Quantity = 0
                    };

                    await unitOfWork.Balances.Add(balance);
                }

                var delta = increase ? item.Quantity : -item.Quantity;

                if (balance.Quantity + delta < 0)
                {
                    throw new InvalidOperationException("Insufficient stock for adjustment.");
                }

                balance.Quantity += delta;
                await unitOfWork.Balances.Update(balance);
            }
        }
    }
}
