namespace Warehouse.Shared.DTO
{
    public class ShipmentDocumentDTO
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public Guid ClientId { get; set; }
        public DateTime Date { get; set; }
        public ShipmentStatus Status { get; set; }
        public ClientDTO Client { get; set; }
        public List<ShipmentItemDTO> Items { get; set; }
    }
}
