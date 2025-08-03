namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;

    public readonly record struct GetUnitsResult(ResultCode Result, IEnumerable<UnitDTO>? Response);
    public readonly record struct GetUnitResult(ResultCode Result, UnitDTO? Response);
    public readonly record struct CreateUnitResult(ResultCode Result, UnitDTO? Unit);

    public interface IUnitService
    {
        Task<GetUnitsResult> GetAllUnits(bool includeArchived = false);
        Task<GetUnitResult> GetUnit(Guid id);
        Task<CreateUnitResult> Create(UnitDTO dto);
        Task<ActionResult> Update(UnitDTO dto);
        Task<ActionResult> Archive(Guid id);
    }
}
