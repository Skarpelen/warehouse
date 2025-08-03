namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    public readonly record struct GetShipmentsResult(ResultCode Result, IEnumerable<ShipmentDocumentDTO>? Response);
    public readonly record struct GetShipmentResult(ResultCode Result, ShipmentDocumentDTO? Response);
    public readonly record struct CreateShipmentResult(ResultCode Result, ShipmentDocumentDTO? Shipment);

    public interface IShipmentService
    {
        Task<GetShipmentsResult> GetAllShipments(DocumentFilter filter);
        Task<GetShipmentResult> GetShipment(Guid id);
        Task<CreateShipmentResult> Create(ShipmentDocumentDTO dto);
        Task<ActionResult> Update(ShipmentDocumentDTO dto);
        Task<ActionResult> Delete(Guid id);
        Task<ActionResult> Sign(Guid id);
        Task<ActionResult> Revoke(Guid id);
    }
}
