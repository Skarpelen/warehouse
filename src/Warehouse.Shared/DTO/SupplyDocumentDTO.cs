namespace Warehouse.Shared.DTO
{
    public class SupplyDocumentDTO
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public List<SupplyItemDTO> Items { get; set; }
    }
}
