using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class BalanceDTO
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ResourceId { get; set; }

        [Required]
        public Guid UnitOfMeasureId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        public ResourceDTO Resource { get; set; } = new();
        public UnitDTO Unit { get; set; } = new();
    }
}
