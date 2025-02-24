using Abstractions;
using ActualLab.DependencyInjection;
using ActualLab.Fusion;
using ActualLab.Fusion.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorApp4.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var baseUri = new Uri(builder.HostEnvironment.BaseAddress);

        // Fusion
        var fusion = builder.Services.AddFusion();
        fusion.Rpc.AddWebSocketClient(baseUri);

        //fusion.AddAuthClient();

        // Fusion services
        fusion.AddClient<ICountryService>();

        var host = builder.Build();
        await host.Services.HostedServices().Start();
        await host.RunAsync();
    }
}
