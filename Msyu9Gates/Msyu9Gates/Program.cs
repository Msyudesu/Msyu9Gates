using Microsoft.AspNetCore.Mvc;
using Msyu9Gates.Client.Pages;
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


            app.MapPost("/api/0002", ([FromBody] string key) =>
            {
                return Results.Ok(Gate2Utils.CheckKey(builder.Configuration, key));
            });
            app.MapGet("/api/Gate2Attempts", () =>
            {
                return Gate2Data.Gate2AttemptLog;
            });

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
    }
}
