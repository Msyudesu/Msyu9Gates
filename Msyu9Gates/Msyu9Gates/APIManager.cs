using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Msyu9Gates
{
    public static class APIManager
    {
        public static void AddAuthenticationAPIs(WebApplication app)
        {
            app.MapGet("/auth/login/discord", (HttpContext context, string? returnUrl) =>
            {
                var properties = new AuthenticationProperties { RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl };
                return Results.Challenge(properties, new[] { "Discord" });
            });

            app.MapPost("/auth/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/");
            }).RequireAuthorization();

            app.MapGet("/auth/user/me", (ClaimsPrincipal user) =>
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
        }
    }
}
