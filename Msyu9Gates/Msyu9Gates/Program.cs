using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Utils;
using Msyu9Gates.Lib;

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
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            var environment = app.Environment;

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
            Gate gate3 = new Gate(builder.Configuration);
            gate3.Name = "Gate 3";
            gate3.GateDifficulty = Gate.Difficulty.Challenge;


            // Key Checks
            app.MapPost("/api/CheckKey", ([FromBody] GateRequest request) =>
            {
                switch(request.Gate)
                {
                    case 3:
                        return Results.Ok(gate3.CheckKey(request.Key ?? "", request?.KeyID));
                }
                return Results.Ok();
            });

            // Attempt Logs
            app.MapPost("/api/GetAttempts", ([FromBody] GateRequest request) =>
            {
                switch (request.Gate)
                {
                    case 3:
                        return Results.Ok(gate3.GetHistory());
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            app.MapPost("/api/ResetAttempts", ([FromBody] GateRequest request) =>
            {
                switch (request.Gate)
                {
                    case 3:
                        gate3.ResetHistory();
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
                        return Results.Ok(gate3.GetDifficult());
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
    }
}
