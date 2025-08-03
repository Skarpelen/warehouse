using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    using Warehouse.Shared;

    [Table("shipment_document")]
    public class ShipmentDocument : BaseModel
    {
        [Column("number")]
        public string Number { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("status")]
        public ShipmentStatus Status { get; set; }

        public Client Client { get; set; }

        public List<ShipmentItem> Items { get; set; }
    }
}
