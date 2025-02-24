using Abstractions;
using ActualLab.Fusion;
using ActualLab.Fusion.EntityFramework;
using ActualLab.Fusion.Server;
using ActualLab.IO;
using ActualLab.Rpc;
using ActualLab.Rpc.Server;
using BlazorApp4.Components;
using BlazorApp4.Services;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp4;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        // Database
        var appTempDir = FilePath.GetApplicationTempDirectory("", true);
        var dbPath = appTempDir & "BlazorApp.db";
        builder.Services.AddDbContextFactory<AppDbContext>(db => {
            db.UseSqlite($"Data Source={dbPath}");
        });
        builder.Services.AddDbContextServices<AppDbContext>(db => {
            db.AddEntityResolver<long, Country>();
            //db.AddOperations(operations => {
            //    operations.ConfigureOperationLogReader(_ => new()
            //    {
            //        // We use FileBasedDbOperationLogChangeTracking, so unconditional wake-up period
            //        // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
            //        // See what .ToRandom does - most timeouts in Fusion settings are RandomTimeSpan-s,
            //        // but you can provide a normal one too - there is an implicit conversion from it.
            //        CheckPeriod = TimeSpan.FromSeconds(builder.Environment.IsDevelopment() ? 60 : 5).ToRandom(0.05),
            //    });
            //    operations.AddFileSystemOperationLogWatcher();
            //});
        });

        // Fusion
        var fusion = builder.Services.AddFusion(RpcServiceMode.Server, true);
        var fusionServer = fusion.AddWebServer();

        // Fusion services
        fusion.AddService<ICountryService, CountryService>();
        builder.Services.AddSignalR();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseWebSockets(new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });
        //app.UseFusionSession();

        app.UseStaticFiles();

        app.UseRouting();
        
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        //app.UseAuthentication();
        //app.MapBlazorHub();
        app.MapRpcWebSocketServer();
        //app.MapFusionAuth();
        //app.MapFusionBlazorMode();

        // Ensure the DB is created
        var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        // dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        app.Run();
    }
}
