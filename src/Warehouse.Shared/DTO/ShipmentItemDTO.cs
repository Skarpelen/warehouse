namespace Warehouse.Shared.DTO
{
    public class ShipmentItemDTO
    {
        public Guid Id { get; set; }
        public Guid ShipmentDocumentId { get; set; }
        public Guid ResourceId { get; set; }
        public Guid UnitOfMeasureId { get; set; }
        public decimal Quantity { get; set; }
        public ResourceDTO Resource { get; set; }
        public UnitDTO Unit { get; set; }
    }
}
