using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("unit_of_measure")]
    public class UnitOfMeasure : BaseModel
    {
        [Column("name")]
        public string Name { get; set; }
    }
}
