using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    [Table("supply_document")]
    public class SupplyDocument : BaseModel
    {
        [Column("number")]
        public string Number { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        public List<SupplyItem> Items { get; set; }
    }
}
