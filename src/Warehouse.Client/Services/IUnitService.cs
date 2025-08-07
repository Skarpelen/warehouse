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
        Task<CreateUnitResult> CreateUnit(UnitDTO dto);
        Task<ActionResult> UpdateUnit(UnitDTO dto);
        Task<ActionResult> DeleteUnit(Guid id);
        Task<ActionResult> ArchiveUnit(Guid id);
        Task<ActionResult> UnarchiveUnit(Guid id);
    }
}
