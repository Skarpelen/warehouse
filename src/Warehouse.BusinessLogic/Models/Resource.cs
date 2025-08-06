using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("resource")]
    public class Resource : BaseModel
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("is_archived")]
        public bool IsArchived { get; set; }
    }
}
