using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("supply_item")]
    public class SupplyItem : BaseModel
    {
        [Column("supply_document_id")]
        public Guid SupplyDocumentId { get; set; }

        [Column("resource_id")]
        public Guid ResourceId { get; set; }

        [Column("unit_id")]
        public Guid UnitOfMeasureId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        public SupplyDocument Document { get; set; }

        public Resource Resource { get; set; }

        public UnitOfMeasure Unit { get; set; }
    }
}
