using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Data.Utils;

public static class JSONDataSeeder
{
    private const string SeedFileRelativePath = "Data/Misc/SeedData.json";

    public static async Task SeedFromJsonAsync(
        ApplicationDbContext db,
        IWebHostEnvironment env,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // 1. Read JSON file from content root
        var seedFilePath = Path.Combine(env.ContentRootPath, SeedFileRelativePath);
        if (!File.Exists(seedFilePath))
        {
            logger.LogWarning("Seed data file not found at path: {Path}. Skipping JSON seeding.", seedFilePath);
            return;
        }

        var existingGates = await GateDbUtils.GetAllGatesAsync(db, cancellationToken);
        if (existingGates.Count > 0)
        {
            logger.LogWarning("Database already contains Gate data. Aborting seed.");
            return;
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(seedFilePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read seed data file: {Path}", seedFilePath);
            return;
        }

        SeedRoot? root;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            root = JsonSerializer.Deserialize<SeedRoot>(json, options);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize seed data JSON.");
            return;
        }

        if (root?.Gates is null || root.Gates.Count == 0)
        {
            logger.LogWarning("Seed data JSON did not contain any gates or is malformed. Skipping.");
            return;
        }

        // 2. Flatten gates/chapters/keys from the nested dictionary structure
        var gatesToInsert = new List<Gate>();
        var chaptersToInsert = new List<Chapter>();
        var keysToInsert = new List<GateKey>();

        foreach ((string gateKey, GateSeed gateSeed) in root.Gates)
        {
            // Create GateModel
            var gateModel = new Gate
            {
                GateNumber = gateSeed.GateNumber,
                GateOverallDifficultyLevel = gateSeed.GateOverallDifficultyLevel,
                IsCompleted = gateSeed.IsCompleted,
                IsLocked = gateSeed.IsLocked,
                DateUnlocked = gateSeed.DateUnlocked,
                DateCompleted = gateSeed.DateCompleted,
                Narrative = gateSeed.Narrative,
                Conclusion = gateSeed.Conclusion
            };
            gatesToInsert.Add(gateModel);

            if (gateSeed.Chapters is null)
            {
                continue;
            }

            foreach ((string chapterKey, ChapterSeed chapterSeed) in gateSeed.Chapters)
            {
                var chapterModel = new Chapter
                {
                    GateId = chapterSeed.GateId,
                    ChapterNumber = chapterSeed.ChapterNumber,
                    DifficultyLevel = chapterSeed.DifficultyLevel,
                    IsCompleted = chapterSeed.IsCompleted,
                    IsLocked = chapterSeed.IsLocked,
                    DateUnlockedUtc = chapterSeed.DateUnlocked,
                    DateCompletedUtc = chapterSeed.DateCompleted,
                    Narrative = chapterSeed.Narrative,
                    RouteGuid = ParseGuidOrNull(chapterSeed.RouteGuid)
                };
                chaptersToInsert.Add(chapterModel);

                if (chapterSeed.Keys is null)
                {
                    continue;
                }

                foreach ((string keyId, KeySeed keySeed) in chapterSeed.Keys)
                {
                    var keyModel = new GateKey
                    {
                        GateId = keySeed.GateId,
                        ChapterId = keySeed.ChapterId,
                        KeyNumber = keySeed.KeyNumber,
                        KeyValue = keySeed.KeyValue,
                        Discovered = keySeed.Discovered,
                        DateDiscoveredUtc = keySeed.DateDiscovered
                    };
                    keysToInsert.Add(keyModel);
                }
            }
        }

        // 3. Idempotent insert:
        //    - Avoid inserting duplicates that violate unique indexes.
        //    - Assume a freshly recreated DB is empty, but still guard in case of partial data.

        // Gates: unique by GateNumber
        var existingGateNumbers = await db.GatesDb
            .Select(g => g.GateNumber)
            .ToListAsync(cancellationToken);

        var newGates = gatesToInsert
            .Where(g => !existingGateNumbers.Contains(g.GateNumber))
            .ToList();

        if (newGates.Count > 0)
        {
            await db.GatesDb.AddRangeAsync(newGates, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} gates from JSON.", newGates.Count);
        }
        else
        {
            logger.LogInformation("No new gates to seed from JSON (all gate numbers already present).");
        }

        // At this point, GateModel.Id values are generated. We keep using GateId from seed,
        // which your model also uses as a separate FK, so we do not need to remap IDs.

        // Chapters: unique by (GateId, ChapterNumber)
        var existingChapterKeys = await db.ChaptersDb
            .Select(c => new { c.GateId, c.ChapterNumber })
            .ToListAsync(cancellationToken);

        var newChapters = chaptersToInsert
            .Where(c => !existingChapterKeys.Any(ec => ec.GateId == c.GateId && ec.ChapterNumber == c.ChapterNumber))
            .ToList();

        if (newChapters.Count > 0)
        {
            await db.ChaptersDb.AddRangeAsync(newChapters, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} chapters from JSON.", newChapters.Count);
        }
        else
        {
            logger.LogInformation("No new chapters to seed from JSON (all (GateId,ChapterNumber) combos already present).");
        }

        // Keys: unique by (KeyNumber, KeyValue)
        var existingKeyPairs = await db.KeysDb
            .Select(k => new { k.KeyNumber, k.KeyValue })
            .ToListAsync(cancellationToken);

        var newKeys = keysToInsert
            .Where(k => !existingKeyPairs.Any(ek => ek.KeyNumber == k.KeyNumber && ek.KeyValue == k.KeyValue))
            .ToList();

        if (newKeys.Count > 0)
        {
            await db.KeysDb.AddRangeAsync(newKeys, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} keys from JSON.", newKeys.Count);
        }
        else
        {
            logger.LogInformation("No new keys to seed from JSON (all (KeyNumber,KeyValue) combos already present).");
        }
    }

    private static Guid? ParseGuidOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    // ---------------------------------------------------------------------
    // Internal DTOs mirroring the JSON shape (with dictionary nesting)
    // ---------------------------------------------------------------------

    private sealed class SeedRoot
    {
        public Dictionary<string, GateSeed>? Gates { get; set; }
    }

    private sealed class GateSeed
    {
        public int GateNumber { get; set; }
        public int GateOverallDifficultyLevel { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? DateUnlocked { get; set; }
        public DateTimeOffset? DateCompleted { get; set; }
        public string? Narrative { get; set; }
        public string? Conclusion { get; set; }

        public Dictionary<string, ChapterSeed>? Chapters { get; set; }
    }

    private sealed class ChapterSeed
    {
        public int GateId { get; set; }
        public int ChapterNumber { get; set; }
        public int DifficultyLevel { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? DateUnlocked { get; set; }
        public DateTimeOffset? DateCompleted { get; set; }
        public string? Narrative { get; set; }
        public string? RouteGuid { get; set; }

        public Dictionary<string, KeySeed>? Keys { get; set; }
    }

    private sealed class KeySeed
    {
        public int GateId { get; set; }
        public int ChapterId { get; set; }
        public int KeyNumber { get; set; }
        public string KeyValue { get; set; } = string.Empty;
        public bool Discovered { get; set; }
        public DateTimeOffset? DateDiscovered { get; set; }
    }
}
