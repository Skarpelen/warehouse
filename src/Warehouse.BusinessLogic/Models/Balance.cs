using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("balance")]
    public class Balance : BaseModel
    {
        [Column("resource_id")]
        public Guid ResourceId { get; set; }

        [Column("unit_id")]
        public Guid UnitOfMeasureId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        public Resource Resource { get; set; }

        public UnitOfMeasure Unit { get; set; }
    }
}
