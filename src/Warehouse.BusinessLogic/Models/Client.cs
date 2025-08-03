using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("client")]
    public class Client : BaseModel
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }
    }
}
