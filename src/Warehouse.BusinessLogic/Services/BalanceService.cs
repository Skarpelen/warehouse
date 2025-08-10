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
                    var res = await unitOfWork.Resources.Get(adj.ResourceId);
                    var unit = await unitOfWork.UnitsOfMeasure.Get(adj.UnitId);

                    var resName = res?.Name ?? adj.ResourceId.ToString();
                    var unitName = unit?.Name ?? adj.UnitId.ToString();

                    throw new InvalidOperationException(
                        $"Невозможно списать ресурс '{resName}' (ед. изм. '{unitName}'): остаток отсутствует.");
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
                        var need = -adj.Quantity;

                        var res = await unitOfWork.Resources.Get(adj.ResourceId);
                        var unit = await unitOfWork.UnitsOfMeasure.Get(adj.UnitId);

                        var resName = res?.Name ?? adj.ResourceId.ToString();
                        var unitName = unit?.Name ?? adj.UnitId.ToString();

                        throw new InvalidOperationException(
                            $"Недостаточно ресурса '{resName}' (ед. изм. '{unitName}'): доступно {available}, требуется {need}.");
                    }
                }
            }
        }
    }

    public record struct BalanceAdjustment(Guid ResourceId, Guid UnitId, decimal Quantity);
}
