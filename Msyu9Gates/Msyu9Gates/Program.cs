using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Utils;

namespace Msyu9Gates;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
        });            

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var environment = app.Environment;

        DbUtils.DatabaseMigrations(app, builder, args, logger);            

        logger.LogInformation($"Application Started: Running in {environment}");

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

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }

    private static bool APIKeyIsValid(HttpContext context, IConfiguration config)
    {
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            string? validApiKey = config.GetValue<string>("ClientUnsecuredApiKey");
            return !string.IsNullOrEmpty(validApiKey) && apiKey == validApiKey;
        }
        return false;
    }    
}
