using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Lib;
using Msyu9Gates.Lib.Models;
using Msyu9Gates.Utils;
using System;
using System.Drawing.Text;
using System.Security.Cryptography;

namespace Msyu9Gates
{
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

            DatabaseMigrations(app, builder, args, logger);            

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

        private static void DatabaseMigrations(WebApplication app, WebApplicationBuilder builder, string[] args, ILogger logger)
        {
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate(); // Applies existing migrations from the Migrations folder. Be sure to build and commit migrations before deploying.
                                           // DO NOT COMMIT DATABASE FILES.  .db, .db-wal, and .db-shm (added to gitignore)
                    app.Logger.LogInformation($"Database Migration Completed in: Running in {app.Environment}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration or data rebuild. Deleting Database file.");

                string db_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db");
                string db_wal_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db-wal");
                string db_shm_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db-shm");

                if (File.Exists(db_path))
                {
                    File.Delete(db_path);
                    logger.LogWarning("Database file found: msyu9gates.db -- Deleting.");
                }
                if (File.Exists(db_wal_path))
                {
                    File.Delete(db_wal_path);
                    logger.LogWarning("Database WAL file found: msyu9gates.db-wal -- Deleting.");
                }
                if (File.Exists(db_shm_path))
                {
                    File.Delete(db_shm_path);
                    logger.LogWarning("Database SHM file found: msyu9gates.db-shm -- Deleting.");
                }

                if (args.Length == 0)
                {
                    logger.LogError("Retrying migration after deleting database files.");
                    Main(new string[1] { "-retry" }); // Restart the application to attempt migration again.
                }
                else if (args.Length > 0 && args[0] == "-retry")
                {
                    logger.LogError("Retry failed. Exiting.");
                    return; // Exit the application if migration fails.
                }
            }
        }
    }
}
