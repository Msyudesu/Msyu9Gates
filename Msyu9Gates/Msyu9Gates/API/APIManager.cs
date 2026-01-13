using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Contracts;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Utils;
using System.Collections.Generic;
using System.Security.Claims;

namespace Msyu9Gates.API;

public static class APIManager
{
    public static void AddAuthenticationAPIs(WebApplication app)
    {
        app.MapGet("/auth/login/discord", (HttpContext context, string? returnUrl) =>
        {
            var properties = new AuthenticationProperties { RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl };
            return Results.Challenge(properties, new[] { "Discord" });
        });

        app.MapPost("/auth/logout", async (context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/");
        }).RequireAuthorization();

        app.MapGet("/profile", (ClaimsPrincipal user) =>
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return Results.Redirect("/auth/login/discord");

            return Results.Ok(new
            {
                Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = user?.Identity?.Name ?? string.Empty,
                Avatar = user?.FindFirstValue("discord:avatar") ?? string.Empty,
                IsAdmin = user?.IsInRole("Admin")
            });
        });
    }

    public static void AddGateAPIs(WebApplication app)
    {
        app.MapGet("/api/gates/all", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await GateDbUtils.GetAllGatesAsync(db, ct));
        });

        app.MapGet("/api/gates/{gateNumber:int}", async (int gateNumber, ApplicationDbContext db, CancellationToken ct) =>
        {
            var gate = await GateDbUtils.GetGateByNumberAsync(db, gateNumber, ct);
            return gate is null ? Results.NotFound() : Results.Ok(gate);
        });

        app.MapPut("/api/gates/save/{gateNumber:int}", async (int gateNumber, GateDto gateDto, ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await GateDbUtils.SaveGateAsync(db, gateDto, ct));
        }).RequireAuthorization("Admin");
    }

    public static void AddChapterAPIs(WebApplication app)
    {
        app.MapGet("/api/chapters/all", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await ChapterDbUtils.GetAllChaptersAsync(db, ct));
        });

        app.MapGet("/api/chapters/{gateId:int}", async (int gateId, ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await ChapterDbUtils.GetChaptersByGateIdAsync(db, gateId, ct));
        });

        app.MapGet("/api/chapters/{gateId:int}/{chapterNumber:int}", async (int gateId, int chapterNumber, ApplicationDbContext db, CancellationToken ct) =>
        {
            var chapter = await ChapterDbUtils.GetChapterAsync(db, gateId, chapterNumber, ct);
            return chapter is null ? Results.NotFound() : Results.Ok(chapter);
        });
        app.MapPost("/api/chapters/save", async (ChapterDto chapterDto, ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await ChapterDbUtils.SaveChapterAsync(db, chapterDto, ct));
        }).RequireAuthorization("Admin");
    }

    public static void AddKeyAPIs(WebApplication app)
    {
        // Key APIs can be added here
    }
}
