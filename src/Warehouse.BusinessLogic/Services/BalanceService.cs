using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface IBalanceService
    {
        Task<IEnumerable<Balance>> GetAllAsync(BalanceFilter filter);
    }

    public class BalanceService(IUnitOfWork unitOfWork) : IBalanceService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<Balance>> GetAllAsync(BalanceFilter filter)
        {
            var list = await unitOfWork.Balances.GetAllFiltered(filter);
            return list;
        }
    }
}
