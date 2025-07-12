using Microsoft.AspNetCore.Mvc;

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
            Gate gate2 = new Gate(builder.Configuration);
            gate2.Name = "Gate 2";

            Gate gate3 = new Gate(builder.Configuration);
            gate3.Name = "Gate 3";


            // Key Checks
            app.MapPost("/api/CheckKey", ([FromBody] GateRequest request) =>
            {
                return Results.Ok();
            });

            // Attempt Logs
            app.MapPost("/api/GetAttempts", ([FromBody] int gate) =>
            {
                switch (gate)
                {
                    case 2:
                        return Results.Ok(gate2.history);
                    case 3:
                        return Results.Ok(gate3.history);
                    default:
                        return Results.BadRequest("Invalid gate number");
                }
            });

            // Other
            app.MapGet("/api/Is2CClueEnabled", () =>
            {
                return Results.Ok(GateFlags.Gate2C_ClueEnabled);
            });
        }
    }
}
