using Microsoft.EntityFrameworkCore;

namespace Warehouse.DataAccess.Models
{
    using Warehouse.BusinessLogic.Models;

    public class WarehouseContext : DbContext
    {
        public WarehouseContext(DbContextOptions<WarehouseContext> options) : base(options)
        {
        }

        public DbSet<Balance> Balances { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<ShipmentDocument> ShipmentDocuments { get; set; } = null!;
        public DbSet<ShipmentItem> ShipmentItems { get; set; } = null!;
        public DbSet<SupplyDocument> SupplyDocuments { get; set; } = null!;
        public DbSet<SupplyItem> SupplyItems { get; set; } = null!;
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; } = null!;
    }
}
