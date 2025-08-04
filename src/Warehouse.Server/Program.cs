using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using NLog;

namespace Warehouse.Server
{
    using Warehouse.BusinessLogic.Middlewares;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.BusinessLogic.Services;
    using Warehouse.DataAccess.Repository;
    using Warehouse.WebApi.V1;

    public class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            try
            {
                await RunApp(args);
            }
            catch (HostAbortedException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _log.Fatal(ex, "Application terminated unexpectedly");
            }
        }

        private static async Task RunApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

            // необходимо, чтобы легаси фильтр увидел IConfiguration
            AppDomain.CurrentDomain.SetData("HostBuilderContextConfiguration",
                builder.Configuration);

            builder.ConfigureDatabase();

            builder.Logging.ClearProviders();

            var controllersBuilder = builder.Services.AddControllers();
            controllersBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(typeof(ClientController).Assembly));
            controllersBuilder
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                })
                .AddMvcOptions(options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IBalanceService, BalanceService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IResourceService, ResourceService>();
            builder.Services.AddScoped<IShipmentDocumentService, ShipmentDocumentService>();
            builder.Services.AddScoped<ISupplyDocumentService, SupplyDocumentService>();
            builder.Services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            builder.Services.AddApiVersioning(opt =>
                {
                    opt.ReportApiVersions = true;
                    opt.DefaultApiVersion = new ApiVersion(1, 0);
                    opt.AssumeDefaultVersionWhenUnspecified = true;
                    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(opt =>
                {
                    opt.GroupNameFormat = "'v'VVV";
                    opt.SubstituteApiVersionInUrl = true;
                });

            builder.Services.AddCors(
                options => options.AddPolicy(
                    "client",
                    policy => policy.AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials()));

            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

            builder.ConfigureMapping();

            var app = builder.Build();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseCors("client");
            app.UseRouting();

            await DataAccessConfiguration.EnsureSeedData(app);

            app.UseDeveloperExceptionPage();
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api/{documentName}/swagger.json";
            });

            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                app.UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint($"/api/{description.GroupName}/swagger.json", $"Warehouse API {description.GroupName}");
                    opt.RoutePrefix = $"api/{description.GroupName.ToLower()}";
                    _log.Info("Hosted swagger at {RoutePrefix}", opt.RoutePrefix);
                });
            }

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.MapFallbackToFile("index.html");

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Server", "WarehouseServer");
                await next.Invoke();
            });

            await app.RunAsync();
        }
    }
}
