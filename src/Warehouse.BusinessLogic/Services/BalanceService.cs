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
    }

    public record struct BalanceAdjustment(Guid ResourceId, Guid UnitId, decimal Quantity);
}
