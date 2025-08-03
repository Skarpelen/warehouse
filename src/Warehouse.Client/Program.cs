using Blazored.LocalStorage;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace Warehouse.Client
{
    using Warehouse.Client.Services;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddOptions();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazoredToast();
            builder.Services.AddMudServices();

            builder.Services.AddHttpClient(ApiAccess.ApiClientName, options =>
            {
                options.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}api/v1/");
            });

            builder.Services.AddScoped<IApiAccess, ApiAccess>();

            await builder.Build().RunAsync();
        }
    }
}
