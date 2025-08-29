using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OAuth.Discord;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


using Msyu9Gates.Components;
using Msyu9Gates.Data;
using Msyu9Gates.Utils;
using Msyu9Gates.Lib.Models;

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
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "Discord";
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/login/discord";
            options.LogoutPath = "/logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        })
        .AddDiscord("Discord", options =>
        {
            options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
            options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
            options.CallbackPath = "/signin-discord";
            options.Scope.Clear();
            options.Scope.Add("identify");
            options.SaveTokens = true;

            options.Events.OnCreatingTicket = async context =>
            {
                string discordId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!;
                string username = context.User.GetProperty("global_name").GetString() 
                ?? context.User.GetProperty("username").GetString()!
                ?? string.Empty;
                string? avatar = context.User.TryGetProperty("avatar", out var avatarProperty) && avatarProperty.ValueKind != System.Text.Json.JsonValueKind.Null
                    ? avatarProperty.GetString()
                    : null;

                // Save/Update User
                var scopeFactory = context.HttpContext.RequestServices.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
                await using var dbContext = await scopeFactory.CreateDbContextAsync();

                var existing = await dbContext.UsersDb.FirstOrDefaultAsync(u => u.DiscordId == discordId);
                if (existing is null)
                {
                    dbContext.UsersDb.Add(new UserModel
                    {
                        DiscordId = discordId,
                        Username = username,
                        Avatar = avatar,
                        CreatedDateUtc = DateTimeOffset.UtcNow,
                        LastLoginUtc = DateTimeOffset.UtcNow,
                        IsActive = true
                    });
                }
                else
                {
                    existing.Username = username;
                    existing.Avatar = avatar;
                    existing.LastLoginUtc = DateTimeOffset.UtcNow;
                }
                await dbContext.SaveChangesAsync();

                ClaimsIdentity identity = (ClaimsIdentity)context?.Principal?.Identity!;
                identity.AddClaim(new Claim("discord:avatar", avatar ?? string.Empty));
            };
        });

        builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.MapGet("/login/discord", (HttpContext context, string? returnUrl) =>
        {
            var properties = new AuthenticationProperties { RedirectUri = string.IsNullOrWhiteSpace(returnUrl ) ? "/" : returnUrl };
            return Results.Challenge(properties, new[] { "Discord" });
        });

        app.MapPost("/logout", async(HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/");
        }).RequireAuthorization();

        app.MapGet("/user/me", (ClaimsPrincipal user) =>
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();
            return Results.Ok(new
            {
                Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = user?.Identity?.Name ?? string.Empty,
                Avatar = user?.FindFirstValue("discord:avatar") ?? string.Empty
            });
        });

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
