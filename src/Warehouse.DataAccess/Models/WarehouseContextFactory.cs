using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Warehouse.DataAccess.Models
{
    public class WarehouseContextFactory : IDesignTimeDbContextFactory<WarehouseContext>
    {
        public WarehouseContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("WAREHOUSE_DB_CONNECTION_STRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Environment variable 'WAREHOUSE_DB_CONNECTION_STRING' is not set.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<WarehouseContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new WarehouseContext(optionsBuilder.Options);
        }
    }
}
