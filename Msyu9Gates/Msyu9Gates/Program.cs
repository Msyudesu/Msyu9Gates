using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OAuth.Discord;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Utils;
using Msyu9Gates.Lib.Models;
using Msyu9Gates.Discord;

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

        //Authentication
        DiscordManager.ConfigureDiscordAuthentication(builder);

        builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

        builder.Services.AddResponseCompression();

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var environment = app.Environment;

        app.UseResponseCompression();

        DbUtils.DatabaseMigrations(app, builder, args, logger);
        //DbUtils.ApplyMigrationsAsync(app, CancellationToken.None).GetAwaiter().GetResult();

        logger.LogInformation($"Application Started: Running in {environment}");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        APIManager.AddAuthenticationAPIs(app);

        app.Run();
    }
}
