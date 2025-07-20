using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Models;
using Msyu9Gates.Lib;
using Msyu9Gates.Utils;
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

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate(); // Applies existing migrations from the Migrations folder. Be sure to build and commit migrations before deploying.
                                           // DO NOT COMMIT DATABASE FILES.  .db, .db-wal, and .db-shm (added to gitignore)
                    logger.LogInformation($"Database Migration Completed in: Running in {environment}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration.");
                throw; // Re-throw the exception to ensure the application does not start if migration fails.
            }

            CheckAndRebuildKeyData(app, builder, app.Configuration, app.Logger);

            logger.LogInformation($"Application Started: Running in {environment}");
            BuildGatesAndRegisterApis(app, builder);

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

        private static void BuildGatesAndRegisterApis(WebApplication app, WebApplicationBuilder builder)
        {
            var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            KeyManager keyManager = new KeyManager(builder.Configuration, app.Logger, dbContextFactory);
            GateManager gate3 = new GateManager(builder.Configuration, app.Logger, dbContextFactory, keyManager);
            gate3.Name = "Gate 3";
            gate3.GateDifficulty = GateManager.Difficulty.Challenging;
            gate3.Keys = new List<string>()
            {
                "0007", "0008", "0009"
            };

            // Key Checks
            app.MapPost("/api/CheckKey", ([FromBody] GateRequest request) =>
            {
                switch(request.Gate)
                {
                    case 3:
                        return Results.Ok(gate3.CheckKey(request.Key ?? "", request.Chapter));
                }
                return Results.Ok();
            });

            app.MapPost("/api/SaveKey", (Key key) =>
            {
                return Results.Ok(keyManager.UpdateOrAddKey(key));
            });

            app.MapGet("api/GetKeys", async () =>
            {
                await keyManager.LoadKeys();
                List<string?> keys = keyManager.Keys.Where(x => x.Discovered == true).Select(x => x.KeyValue).ToList();
                return Results.Ok(keys);
            });

            // Attempt Logs
            app.MapPost("/api/GetAttempts", ([FromBody] GateRequest request) =>
            {
                switch (request.Gate)
                {                    
                    case 3:
                        return Results.Ok(gate3.GetHistory(request.Chapter));
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            app.MapPost("/api/ResetAttempts", ([FromBody] GateRequest request) =>
            {
                switch (request.Gate)
                {
                    case 3:
                        gate3.ResetHistory(request.Gate);
                        return Results.Ok();
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            app.MapPost("api/GetDifficulty", ([FromBody] GateRequest request) =>
            {   
                switch (request.Gate)
                {
                    case 3:
                        GateResponse response = new GateResponse(key: null, chapter: request.Chapter, success: true, message: gate3.GetDifficulty());
                        return Results.Ok(response);
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            // Other
            app.MapGet("api/GetGate3Narrative", () =>
            {
                string narrative = string.Empty;
                string filePath = Path.Combine(app.Environment.ContentRootPath, "Data", "Misc", "Gate3HomeNarrative.txt");
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            narrative = reader.ReadToEnd();
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    return Results.NotFound($"Narrative file not found: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred while reading the narrative file: {ex.Message}");
                }
                return Results.Ok(narrative);
            });
        }

        private static void CheckAndRebuildKeyData(WebApplication app, WebApplicationBuilder builder, IConfiguration config, ILogger logger)
        {
            IConfiguration _config = config;
            var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            KeyManager keyManager = new KeyManager(_config, app.Logger, dbContextFactory);

            bool triggerRebuild = _config.GetValue<bool>("Keys:TriggerRebuild");
            logger.LogWarning($"TriggerRebuild is set to: {triggerRebuild}");

            if (!triggerRebuild
                && keyManager.LoadKeys().GetAwaiter().GetResult()
                && keyManager.Keys.Count > 0)
            {
                app.Logger.LogInformation("Keys already exist in the database or rebuilt flag disabled. Rebuild aborted.");
                return;
            }

            using (var db = dbContextFactory.CreateDbContext())
            {
                db.Database.ExecuteSqlRaw("DELETE FROM KeysDb; DELETE FROM sqlite_sequence WHERE name=''");
                logger.LogInformation("Cleared existing keys from the database.");
            }
            app.Logger.LogInformation("No keys found in the database. Rebuilding key data...");
            
            for (int i = 1; i <= 6; i++)
            {
                keyManager.Keys.Add(new Key(id: i, keyValue: config.GetValue<string>($"Keys:000{i}") ?? "", dateDiscovered: DateTime.Now, discovered: true));
                Console.WriteLine($"Discovered Key 000{i} added with value: {config.GetValue<string>($"Keys:000{i}") ?? ""}");
            }
            for (int i = 7; i <= 9; i++)
            {
                keyManager.Keys.Add(new Key(id: i, keyValue: config.GetValue<string>($"Keys:000{i}") ?? "", dateDiscovered: DateTime.Now, discovered: false));
                Console.WriteLine($"Undiscovered Key 000{i} added with value: {config.GetValue<string>($"Keys:000{i}") ?? ""}");
            }
            
            foreach (var key in keyManager.Keys)
            {
                keyManager.UpdateOrAddKey(key).GetAwaiter().GetResult();
            }
            app.Logger.LogInformation("Key data rebuilt successfully.");
        }
    }
}
