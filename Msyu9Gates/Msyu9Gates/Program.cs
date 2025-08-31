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
        builder.Services.AddOptions<DiscordAuthOptions>()
            .Bind(builder.Configuration.GetSection(DiscordAuthOptions.ConfigSection))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "ClientId required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "ClientSecret required.")
            .ValidateOnStart();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "Discord";
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/auth/login/discord";
            options.LogoutPath = "/auth/logout";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = ".NineGates.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;            
        })
        .AddDiscord("Discord", options =>
        {
            var discordOptions = builder.Configuration.GetSection(DiscordAuthOptions.ConfigSection).Get<DiscordAuthOptions>()!;

            options.ClientId = discordOptions.ClientId!;
            options.ClientSecret = discordOptions.ClientSecret!;
            options.CallbackPath = discordOptions.CallbackPath!;
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
                
                if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                    identity.AddClaim(new Claim(ClaimTypes.Name, username));
            };
        });

        builder.Services.AddAuthorizationBuilder().AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

        builder.Services.AddResponseCompression();        

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var environment = app.Environment;

        app.UseResponseCompression();

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

        APIManager.AddAuthenticationAPIs(app);

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
