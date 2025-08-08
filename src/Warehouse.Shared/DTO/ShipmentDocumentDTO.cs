using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class ShipmentDocumentDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Номер обязателен")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Клиент обязателен")]
        public Guid ClientId { get; set; }

        [Required(ErrorMessage = "Дата обязательна")]
        public DateTimeOffset Date { get; set; }

        public ShipmentStatus Status { get; set; }

        public ClientDTO Client { get; set; } = new();

        [MinLength(1, ErrorMessage = "Должна быть хотя бы одна позиция")]
        public List<ShipmentItemDTO> Items { get; set; } = new();
    }
}
