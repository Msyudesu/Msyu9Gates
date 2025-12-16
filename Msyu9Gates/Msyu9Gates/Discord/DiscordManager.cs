using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Security.Claims;

using Msyu9Gates.Data;
using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Discord;

public static class DiscordManager
{
    public static void ConfigureDiscordAuthentication(WebApplicationBuilder builder)
    {
        var discordOptions = builder.Configuration.GetSection(DiscordAuthOptions.ConfigSection).Get<DiscordAuthOptions>()!;

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
            options.LoginPath = discordOptions.LoginPath;
            options.LogoutPath = discordOptions.LogoutPath;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = ".NineGates.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        })
        .AddDiscord("Discord", options =>
        {
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
    }
}
