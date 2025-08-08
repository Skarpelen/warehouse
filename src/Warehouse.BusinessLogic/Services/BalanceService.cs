using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface IBalanceService
    {
        Task<IEnumerable<Balance>> GetAllAsync(BalanceFilter filter);

        /// <summary>
        /// Пакетная корректировка остатков. Бросает, если добавление не удалось.
        /// </summary>
        Task AdjustBatchAsync(IEnumerable<BalanceAdjustment> adjustments);

        /// <summary>
        /// Проверяет, что после применения корректировок не будет отрицательных остатков.
        /// Бросает InvalidOperationException при нехватке.
        /// </summary>
        Task ValidateBatchAsync(IEnumerable<BalanceAdjustment> adjustments);
    }

    public class BalanceService(IUnitOfWork unitOfWork) : IBalanceService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<Balance>> GetAllAsync(BalanceFilter filter)
        {
            var list = await unitOfWork.Balances.GetAllFiltered(filter);
            return list;
        }

        public async Task AdjustBatchAsync(IEnumerable<BalanceAdjustment> adjustments)
        {
            foreach (var adj in adjustments)
            {
                var bal = await unitOfWork.Balances
                    .GetByResourceAndUnitAsync(adj.ResourceId, adj.UnitId);

                if (bal != null)
                {
                    bal.Quantity += adj.Quantity;
                    bal.MarkUpdated();
                }
                else if (adj.Quantity > 0)
                {
                    var newBal = new Balance
                    {
                        ResourceId = adj.ResourceId,
                        UnitOfMeasureId = adj.UnitId,
                        Quantity = adj.Quantity
                    };

                    await unitOfWork.Balances.Add(newBal);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Balance entry missing or insufficient stock.");
                }
            }

            await unitOfWork.CompleteAsync();
        }

        public async Task ValidateBatchAsync(IEnumerable<BalanceAdjustment> adjustments)
        {
            foreach (var adj in adjustments)
            {
                if (adj.Quantity < 0)
                {
                    var bal = await unitOfWork.Balances
                        .GetByResourceAndUnitAsync(adj.ResourceId, adj.UnitId);

                    var available = bal?.Quantity ?? 0m;
                    if (available + adj.Quantity < 0m)
                    {
                        throw new InvalidOperationException(
                            $"Not enough resource '{adj.ResourceId}' " +
                            $"(unit of measure '{adj.UnitId}') for quantity {adj.Quantity}.");
                    }
                }
            }
        }
    }

    public record struct BalanceAdjustment(Guid ResourceId, Guid UnitId, decimal Quantity);
}
