using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
            
            GateManager gate3 = new GateManager(builder.Configuration, app.Logger, dbContextFactory, keyManager, gateId: 3);
            gate3.Name = "Gate 3";
            gate3.GateDifficulty = GateManager.Difficulty.Challenging;
            gate3.Keys = new List<string>()
            {
                "0007", "0008", "0009"
            };

            List<GateManager> gates = new List<GateManager>();
            gates.Add(gate3);

            foreach(var gate in gates)
            {
                app.Logger.LogInformation($"Rebuilding Chapter Data for {gate.Name} with Gate ID: {gate.GateId}");
                CheckAndRebuildChapterData(app, builder, app.Configuration, app.Logger, gate3);
            }            

            // Key Checks
            app.MapPost("/api/CheckKey", (HttpContext httpContext, [FromBody] GateRequest request) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received CheckKey request from IP: {ip}, Key: {request.Key}, Chapter: {request.Chapter}, Gate: {request.Gate}");

                switch (request.Gate)
                {
                    case 3:
                        return Results.Ok(gate3.CheckKey(request.Key ?? "", request.Chapter));
                }
                return Results.Ok();
            });

            app.MapPost("/api/SaveKey", (HttpContext httpContext, Key key) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received SaveKey request from IP: {ip}, Key: {key.KeyValue}");

                return Results.Ok(keyManager.UpdateOrAddKey(key));
            });

            app.MapGet("api/GetKeys", async (HttpContext httpContext) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received GetKeys request from IP: {ip}");

                await keyManager.LoadKeys();
                List<string?> keys = keyManager.Keys.Where(x => x.Discovered == true).Select(x => x.KeyValue).ToList();
                return Results.Ok(keys);
            });

            // Attempt Logs
            app.MapPost("/api/GetAttempts", (HttpContext httpContext, [FromBody] GateRequest request) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received GetAttempts request from IP: {ip} for Gate: {request.Gate} Chapter: {request.Chapter}");

                switch (request.Gate)
                {                    
                    case 3:
                        return Results.Ok(gate3.GetHistory(request.Chapter));
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            app.MapPost("/api/ResetAttempts", (HttpContext httpContext, [FromBody] GateRequest request) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received ResetAttempts request from IP: {ip} for Gate: {request.Gate} Chapter: {request.Chapter}");

                switch (request.Gate)
                {
                    case 3:
                        gate3.ResetHistory(request.Gate);
                        return Results.Ok();
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            app.MapPost("api/GetDifficulty", (HttpContext httpContext,[FromBody] GateRequest request) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received GetDifficulty request from IP: {ip} for Gate: {request.Gate} Chapter: {request.Chapter}");

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
            app.MapPost("api/GetGateNarrative", (HttpContext httpContext, [FromBody] GateRequest request) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                app.Logger.LogInformation($"Received GetGateNarrative request from IP: {ip} for Gate: {request.Gate}, Chapter: {request.Chapter}");

                if (!IsAPIKeyValid(httpContext, app.Configuration))
                {
                    return Results.Unauthorized();
                }

                string fileName = string.Empty;
                
                if(request.Gate == 3)
                switch (request.Chapter)
                {
                    case 1:
                        fileName = "Gate3HomeNarrative.txt";
                        break;
                    case 2:
                        fileName = "Gate3Chapter2Narrative.txt";
                        break;
                    case 3:
                        fileName = "Gate3Chapter3Narrative.txt";
                        break;
                    default:
                        return Results.BadRequest("Invalid chapter number");
                }

                string narrativeText = string.Empty;

                if (TryRetrieveNarrativeText(app, fileName, out narrativeText))
                {
                    GateResponse response = new GateResponse(key: null, chapter: request.Chapter, success: true, message: narrativeText);

                    return Results.Ok(response);
                }
                else
                {
                    GateResponse response = new GateResponse(key: null, chapter: request.Chapter, success: false, message: "Failed to retrieve narrative text.");
                    return Results.Problem("Failed to retrieve narrative text for Gate 3 Chapter 2.");
                }
            });
        }

        private static bool IsAPIKeyValid(HttpContext context, IConfiguration config)
        {
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                string? validApiKey = config.GetValue<string>("ClientUnsecuredApiKey");
                return !string.IsNullOrEmpty(validApiKey) && apiKey == validApiKey;
            }
            return false;
        }

        private static bool TryRetrieveNarrativeText(WebApplication app, string fileName, out string text)
        {
            string filePath = Path.Combine(app.Environment.ContentRootPath, "Data", "Misc", fileName);
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(fs);
                text = reader.ReadToEnd();
                return true;
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, $"Error reading file {fileName}: {ex.Message}");
                text = string.Empty;
                return false;
            }
        }

        private static void CheckAndRebuildChapterData(WebApplication app, WebApplicationBuilder builder, IConfiguration config, ILogger logger, GateManager gateManager)
        {
            IConfiguration _config = config;
            var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            ChapterManager chapterManager = new ChapterManager(_config, app.Logger, dbContextFactory);

            bool triggerRebuild = _config.GetValue<bool>("Chapters:TriggerRebuild");
            logger.LogWarning($"[Chapters] TriggerRebuild is set to: {triggerRebuild}");

            if(!triggerRebuild
                && chapterManager.LoadChapters().GetAwaiter().GetResult()
                && chapterManager.Chapters.Count > 0)
            {
                app.Logger.LogInformation("Chapters already exist in the database or rebuilt flag disabled. Rebuild aborted.");
                return;
            }

            using (var db = dbContextFactory.CreateDbContext())
            {
                db.Database.ExecuteSqlRaw("DELETE FROM ChaptersDb; DELETE FROM sqlite_sequence WHERE name='ChaptersDb'");
                logger.LogInformation("Cleared existing chapters from the database.");
            }
            app.Logger.LogInformation("No chapters found in the database. Rebuilding key data...");

            chapterManager.Chapters.Add(new Chapter(gateId: gateManager.GateId, chapter: Chapter.GateChapter.I, isLocked: true, isCompleted: false, dateUnlocked: null, dateCompleted: null));
            chapterManager.Chapters.Add(new Chapter(gateId: gateManager.GateId, chapter: Chapter.GateChapter.II, isLocked: true, isCompleted: false, dateUnlocked: null, dateCompleted: null));
            chapterManager.Chapters.Add(new Chapter(gateId: gateManager.GateId, chapter: Chapter.GateChapter.III, isLocked: true, isCompleted: false, dateUnlocked: null, dateCompleted: null));

            foreach (var chapter in chapterManager.Chapters)
            {
                chapterManager.UpdateOrAddChapter(chapter).GetAwaiter().GetResult();
            }
            app.Logger.LogInformation("Chapter data rebuilt successfully.");
        }

        private static void CheckAndRebuildKeyData(WebApplication app, WebApplicationBuilder builder, IConfiguration config, ILogger logger)
        {
            IConfiguration _config = config;
            var dbContextFactory = app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

            KeyManager keyManager = new KeyManager(_config, app.Logger, dbContextFactory);

            bool triggerRebuild = _config.GetValue<bool>("Keys:TriggerRebuild");
            logger.LogWarning($"[Keys] TriggerRebuild is set to: {triggerRebuild}");

            if (!triggerRebuild
                && keyManager.LoadKeys().GetAwaiter().GetResult()
                && keyManager.Keys.Count > 0)
            {
                app.Logger.LogInformation("Keys already exist in the database or rebuilt flag disabled. Rebuild aborted.");
                return;
            }

            using (var db = dbContextFactory.CreateDbContext())
            {
                db.Database.ExecuteSqlRaw("DELETE FROM KeysDb; DELETE FROM sqlite_sequence WHERE name='KeysDb'");
                logger.LogInformation("Cleared existing keys from the database.");
            }
            app.Logger.LogInformation("No keys found in the database. Rebuilding key data...");
            
            for (int i = 1; i <= 6; i++)
            {
                keyManager.Keys.Add(new Key(id: i, keyValue: config.GetValue<string>($"Keys:000{i}") ?? "", dateDiscovered: DateTime.Now, discovered: true));
            }
            for (int i = 7; i <= 9; i++)
            {
                keyManager.Keys.Add(new Key(id: i, keyValue: config.GetValue<string>($"Keys:000{i}") ?? "", dateDiscovered: DateTime.Now, discovered: false));
            }
            
            foreach (var key in keyManager.Keys)
            {
                keyManager.UpdateOrAddKey(key).GetAwaiter().GetResult();
            }
            app.Logger.LogInformation("Key data rebuilt successfully.");
        }
    }
}
