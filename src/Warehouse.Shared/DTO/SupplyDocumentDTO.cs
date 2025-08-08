using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class SupplyDocumentDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Номер обязателен")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Дата обязателена")]
        public DateTimeOffset Date { get; set; }

        public List<SupplyItemDTO> Items { get; set; } = new();
    }
}
