using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class UnitDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название единицы обязательно")]
        public string Name { get; set; } = string.Empty;

        public bool IsArchived { get; set; }
    }
}
