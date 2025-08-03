using Microsoft.EntityFrameworkCore;
using NLog;

namespace Warehouse.Server
{
    using Warehouse.DataAccess.Models;

    public static class DataAccessConfiguration
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
            var usedDatabase = builder.Configuration["UsedDatabase"];
            var dbConnectConfig = builder.Configuration.GetRequiredSection("Databases").GetRequiredSection(usedDatabase!);

            if (usedDatabase == "Postgres")
            {
                var connectionString = dbConnectConfig["ConnectionString"];
                builder.Services.AddDbContext<WarehouseContext>(options =>
                    options
                        .EnableSensitiveDataLogging()
                        .UseNpgsql(connectionString));
            }
            //else if (usedDatabase == "InMemory")
            //{
            //    var databaseName = dbConnectConfig["DatabaseName"]!;
            //    builder.Services.AddDbContext<StationContext>(options =>
            //        options
            //            .EnableSensitiveDataLogging()
            //            .UseInMemoryDatabase(databaseName));
            //}
            else
            {
                throw new Exception("Unknown database type");
            }
        }

        public static async Task EnsureSeedData(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

            _log.Info("Getting context and services");
            var context = serviceScope.ServiceProvider.GetRequiredService<WarehouseContext>();

            if (context.Database.IsRelational())
            {
                _log.Info("Migrating database");
                await context.Database.MigrateAsync();
            }

            //_log.Info("Seeding data");
            // Здесь можно добавить логику на заполнение бд изначальными записями
        }
    }
}
