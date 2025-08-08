namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    public readonly record struct GetShipmentsResult(ResultCode Result, IEnumerable<ShipmentDocumentDTO>? Response);
    public readonly record struct GetShipmentResult(ResultCode Result, ShipmentDocumentDTO? Response);
    public readonly record struct CreateShipmentResult(ResultCode Result, ShipmentDocumentDTO? Response);

    public interface IShipmentService
    {
        Task<GetShipmentsResult> GetAllShipments(DocumentFilter filter);
        Task<GetShipmentResult> GetShipment(Guid id);
        Task<CreateShipmentResult> CreateShipment(ShipmentDocumentDTO dto);
        Task<ActionResult> UpdateShipment(ShipmentDocumentDTO dto);
        Task<ActionResult> DeleteShipment(Guid id);
        Task<ActionResult> SignShipment(Guid id);
        Task<ActionResult> RevokeShipment(Guid id);
    }
}
