using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class ShipmentItemDTO
    {
        public Guid Id { get; set; }

        public Guid ShipmentDocumentId { get; set; }

        [Required(ErrorMessage = "Ресурс обязателен")]
        public Guid ResourceId { get; set; }

        [Required(ErrorMessage = "Единица измерения обязательна")]
        public Guid UnitOfMeasureId { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public decimal Quantity { get; set; }

        public ResourceDTO Resource { get; set; } = new ResourceDTO();
        public UnitDTO Unit { get; set; } = new UnitDTO();
    }
}
