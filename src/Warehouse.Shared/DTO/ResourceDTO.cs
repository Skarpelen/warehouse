using System.ComponentModel.DataAnnotations;

namespace Warehouse.Shared.DTO
{
    public class ResourceDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название ресурса обязательно")]
        public string Name { get; set; } = string.Empty;

        public bool IsArchived { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
