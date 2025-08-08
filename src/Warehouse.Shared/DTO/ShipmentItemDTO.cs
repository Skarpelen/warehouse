using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [Range(0, double.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public decimal Quantity { get; set; }

        [JsonIgnore]
        public decimal Available { get; set; }

        public ResourceDTO Resource { get; set; } = new();
        public UnitDTO Unit { get; set; } = new();
    }
}
