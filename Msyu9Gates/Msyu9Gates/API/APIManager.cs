using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

using Msyu9Gates.Contracts;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Utils;
using Msyu9Gates.Lib;
using Msyu9Gates.Data.Models;
using Msyu9Gates.Lib.Contracts;

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

        app.MapGet("/api/gates/{gateNumber:int}/narrative", async (int gateNumber, ApplicationDbContext db, CancellationToken ct) =>
        {
            var narrative = await GateDbUtils.GetGateNarrativeAsync(db, gateNumber, ct);
            return Results.Ok(new { Narrative = await ReadNarrativeFromFileAsync(narrative) });
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

        app.MapPost("/api/chapters/{gateId:int}/{chapterNumber:int}/narrative", async (ApplicationDbContext db, CancellationToken ct, [FromBody] GateRequest request) =>
        {
            var narrative = await ChapterDbUtils.GetChapterNarrativeAsync(db, request.Gate, request.Chapter, ct);
            string? narrativeText = await ReadNarrativeFromFileAsync(narrative);

            if (!String.IsNullOrEmpty(narrativeText))
            {
                GateResponse response = new GateResponse(key: null, chapter: request.Chapter, success: true, message: narrativeText);
                return Results.Ok(response);
            }
            else 
            {
                GateResponse response = new GateResponse(key: null, chapter: request.Chapter, success: true, message: "Request successful, but failed to retrieve narrative text from file.");
                return Results.Problem($"Failed to retrieve narrative text for Gate {request.Gate} Chapter {request.Chapter}");
            }
        });

        app.MapPut("/api/chapters/save", async (Chapter chapter, ApplicationDbContext db, CancellationToken ct) =>
        {            
            return Results.Ok(await ChapterDbUtils.SaveChapterAsync(db, chapter.ToDto(), ct));
        }).RequireAuthorization("Admin");
    }

    public static void AddKeyAPIs(WebApplication app)
    {
        app.MapGet("/api/keys/all", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await KeyDbUtils.GetAllKeysAsync(db, ct));
        }).RequireAuthorization("Admin");

        app.MapGet("/api/keys/gate/{gateId:int}", async (int gateId, ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await KeyDbUtils.GetKeysByGateIdAsync(db, gateId, ct));
        }).RequireAuthorization("Admin");

        app.MapGet("/api/keys/gate/{gateId:int}/chapter/{chapterId:int}", async (int gateId, int chapterId, ApplicationDbContext db, CancellationToken ct) =>
        {
            var key = await KeyDbUtils.GetKeyByGateChapterAsync(db, gateId, chapterId, ct);
            return key is null ? Results.NotFound() : Results.Ok(key);
        }).RequireAuthorization("Admin");

        app.MapGet("/api/keys/unlocked", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var keys = await KeyDbUtils.GetUnlockedKeys(db, ct);
            return keys is null ? Results.NotFound() : Results.Ok(keys);
        });

        app.MapPut("/api/keys/save", async (KeyDto keyDto, ApplicationDbContext db, CancellationToken ct) =>
        {
            return Results.Ok(await KeyDbUtils.SaveKeyAsync(db, keyDto, ct));
        }).RequireAuthorization("Admin");
    }

    public static void AddAttemptAPIs(WebApplication app)
    {
        app.MapGet("/api/attempts/{chapterId:int}/{userId:int}/total", async (int chapterId, int userId, ApplicationDbContext db, CancellationToken ct) =>
        {
            var attempts = await AttemptsDbUtils.GetTotalAttemptsByPlayerForChapterAsync(db, userId, chapterId, ct);
            return Results.Ok(new { TotalAttempts = attempts });
        });

        app.MapGet("/api/attempts/{chapterId:int}/{userId:int}", async (int chapterId, int userId, ApplicationDbContext db, CancellationToken ct) =>
        {
            var attempts = await AttemptsDbUtils.GetAttemptsByPlayerForChapterAsync(db, userId, chapterId, ct);
            return Results.Ok(attempts);
        });

        app.MapGet("/api/attempts/{chapterId:int}", async (int chapterId, ApplicationDbContext db, CancellationToken ct) =>
        {
            var attempts = await AttemptsDbUtils.GetAttemptsForChapterAsync(db, chapterId, ct);
            return Results.Ok(attempts);
        }).RequireAuthorization("Admin");

    }

    private static async Task<string?> ReadNarrativeFromFileAsync(string narrative)
    {
        const string NARRATIVE_FOLDER = "Data/Misc/";
        string narrativePath = Path.Combine(NARRATIVE_FOLDER, narrative ?? string.Empty);

        if (!File.Exists(narrativePath) || String.IsNullOrEmpty(narrativePath))
            return String.Empty;

        return await File.ReadAllTextAsync(narrativePath);
    }

    public static void AddAPIs(WebApplication app)
    {
        AddAuthenticationAPIs(app);
        AddGateAPIs(app);
        AddChapterAPIs(app);
        AddKeyAPIs(app);
        AddAttemptAPIs(app);
    }
}
