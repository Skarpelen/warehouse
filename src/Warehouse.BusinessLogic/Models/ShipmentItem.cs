using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("shipment_item")]
    public class ShipmentItem : BaseModel
    {
        [Column("shipment_document_id")]
        public Guid ShipmentDocumentId { get; set; }

        [Column("resource_id")]
        public Guid ResourceId { get; set; }

        [Column("unit_id")]
        public Guid UnitOfMeasureId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        public ShipmentDocument Document { get; set; }

        public Resource Resource { get; set; }

        public UnitOfMeasure Unit { get; set; }
    }
}
