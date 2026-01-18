using Msyu9Gates.Data;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.Validation;

namespace Msyu9Gates.Data.Utils;

public static class DbUtils
{
    /// <summary>
    /// Applies pending migrations. If migration fails and auto-recreate is permitted by configuration,
    /// backs up (optional) and deletes the SQLite files, then retries a clean migration with optional seeding.
    /// </summary>
    public static async Task ApplyMigrationsAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        var logger = app.Logger;
        var config = app.Configuration;

        bool allowAutoRecreate = config.GetValue("Database:AllowAutoRecreate", false);
        bool allowAutoRecreateProd = config.GetValue("Database:AllowAutoRecreateInProduction", false);
        bool backupBeforeRecreate = config.GetValue("Database:BackupBeforeRecreate", true);
        bool seedOnCreate = config.GetValue("Database:SeedOnCreate", true);

        var env = app.Environment;

        var dbPath = Path.Combine(env.ContentRootPath, "msyu9gates.db");
        var walPath = dbPath + "-wal";
        var shmPath = dbPath + "-shm";

        // 1. Try normal migration.
        try
        {
            await MigrateAsync(app, logger, cancellationToken);
            // Attempt Seed, will check if data exists and skip safely if so.
            await SeedAsync(app, logger, cancellationToken);
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Initial database migration failed.");
        }

        // 2. Evaluate policy for destructive recovery.
        if (!allowAutoRecreate || env.IsProduction() && !allowAutoRecreateProd)
        {
            logger.LogCritical($"Auto recreation is disabled (AllowAutoRecreate={allowAutoRecreate}, Prod={env.IsProduction()}, AllowProd={allowAutoRecreateProd}). Aborting startup.");
            throw new InvalidOperationException("Database migration failed and auto-recreate is disabled.");
        }

        logger.LogWarning("Attempting destructive database recreation due to migration failure.");

        // 3. Backup existing DB if requested and it exists.
        if (backupBeforeRecreate && File.Exists(dbPath))
        {
            try
            {
                var backupDir = Path.Combine(env.ContentRootPath, "db_backups");
                Directory.CreateDirectory(backupDir);
                var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFile = Path.Combine(backupDir, $"msyu9gates_{stamp}.db");
                File.Copy(dbPath, backupFile, overwrite: false);
                logger.LogWarning("Database backed up to: {BackupFile}", backupFile);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to backup database before recreation. Aborting to avoid data loss. Exception: {ex}");
                throw;
            }
        }

        // 4. Delete files (if not locked).
        foreach (var path in new[] { dbPath, walPath, shmPath })
        {
            if (File.Exists(path))
            {
                if (IsFileLocked(path))
                {
                    logger.LogCritical("Cannot delete locked file: {File}. Aborting recreation.", path);
                    throw new IOException($"Locked file prevents recreation: {path}");
                }
                try
                {
                    File.Delete(path);
                    logger.LogWarning("Deleted database file: {File}", path);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete database file: {File}. Aborting recreation.", path);
                    throw;
                }
            }
        }

        // 5. Retry migration on a clean slate.
        try
        {
            await MigrateAsync(app, logger, cancellationToken);

            if (seedOnCreate)
            {
                logger.LogInformation("Seeding database after recreation as configured.");
                await SeedAsync(app, logger, cancellationToken);
            }

            logger.LogInformation("Database successfully recreated and migrated.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database recreation attempt failed. Application cannot start.");
            throw;
        }
    }

    private static async Task MigrateAsync(WebApplication app, ILogger logger, CancellationToken ct)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pending = await db.Database.GetPendingMigrationsAsync(ct);
        if (!pending.Any())
        {
            logger.LogInformation("No pending migrations. (Model hash may still differ if schema drifted manually.)");
        }
        else
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count(), string.Join(", ", pending));
        }

        await db.Database.MigrateAsync(ct);
        logger.LogInformation("Database migration completed (Environment: {Env}).", app.Environment.EnvironmentName);
    }

    /// <summary>
    /// Hook for initial seed data after destructive recreate or first run.
    /// Keep idempotent: re-running should not duplicate data.
    /// </summary>
    private static async Task SeedAsync(WebApplication app, ILogger logger, CancellationToken ct)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        try
        {
            await JSONDataSeeder.SeedFromJsonAsync(db, env, logger, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding.");
        }

        logger.LogInformation("Seed step complete.");
    }

    private static bool IsFileLocked(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch
        {
            return true;
        }
    }

    private static T GetValue<T>(this IConfiguration config, string key, T defaultValue)
        where T : struct
    {
        var v = config[key];
        return string.IsNullOrWhiteSpace(v) ? defaultValue :
            (T)Convert.ChangeType(v, typeof(T));
    }
}
