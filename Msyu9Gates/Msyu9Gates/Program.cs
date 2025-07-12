using Microsoft.AspNetCore.Mvc;
using Msyu9Gates.Components;
using Msyu9Gates.Utils;
using Msyu9Gates.Data;

namespace Msyu9Gates
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();
            AddAPIs(app, builder);

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

        private static void AddAPIs(WebApplication app, WebApplicationBuilder builder)
        {
            #region Gate 2 APIs

            // Key Checks
            app.MapPost("/api/0002", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.Check2AKeyIsCorrect(builder.Configuration, key));
            });

            app.MapPost("/api/0003", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.Check2BKeyIsCorrect(builder.Configuration, key, "0003"));
            });

            app.MapPost("/api/0004", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.Check2BKeyIsCorrect(builder.Configuration, key, "0004"));
            });

            app.MapPost("/api/0005", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.Check2CKeyIsCorrect(builder.Configuration, key));
            });
            
            app.MapPost("/api/0006", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.Check2BKeyIsCorrect(builder.Configuration, key, "0006"));
            });

            app.MapGet("/api/Is2CClueEnabled", () =>
            {
                return Results.Ok(Gate2Utils.displayGate2CClue);
            });

            // Attempt Logs
            app.MapGet("/api/Gate2A_Attempts", () =>
            {
                return Gate2Data.Gate2A_AttemptLog;
            });

            app.MapGet("/api/Gate2B_Attempts", () =>
            {
                return Gate2Data.Gate2B_AttemptLog;
            });

            app.MapGet("/api/Gate2C_Attempts", () =>
            {
                return Gate2Data.Gate2C_AttemptLog;
            });
            #endregion
        }
    }
}
