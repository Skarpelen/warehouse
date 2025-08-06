using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class ClientDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Наименование обязательно")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адрес обязателен")]
        public string Address { get; set; } = string.Empty;

        public bool IsArchived { get; set; }
    }
}
