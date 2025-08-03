namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    public readonly record struct GetBalancesResult(ResultCode Result, IEnumerable<BalanceDTO>? Response);

    public interface IBalanceService
    {
        Task<GetBalancesResult> GetAllBalances(BalanceFilter filter);
    }
}
