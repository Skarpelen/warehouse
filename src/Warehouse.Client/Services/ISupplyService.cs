namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    public readonly record struct GetSuppliesResult(ResultCode Result, IEnumerable<SupplyDocumentDTO>? Response);
    public readonly record struct GetSupplyResult(ResultCode Result, SupplyDocumentDTO? Response);
    public readonly record struct CreateSupplyResult(ResultCode Result, SupplyDocumentDTO? Supply);

    public interface ISupplyService
    {
        Task<GetSuppliesResult> GetAllSupplies(DocumentFilter filter);
        Task<GetSupplyResult> GetSupply(Guid id);
        Task<CreateSupplyResult> Create(SupplyDocumentDTO dto);
        Task<ActionResult> Update(SupplyDocumentDTO dto);
        Task<ActionResult> Delete(Guid id);
    }
}
