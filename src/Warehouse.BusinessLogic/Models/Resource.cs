using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("resource")]
    public class Resource : BaseModel
    {
        [Column("name")]
        public string Name { get; set; }
    }
}
