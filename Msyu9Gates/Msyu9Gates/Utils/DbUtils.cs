using Msyu9Gates.Data;
using Microsoft.EntityFrameworkCore;

namespace Msyu9Gates.Utils;

public static class DbUtils
{
    public static void DatabaseMigrations(WebApplication app, WebApplicationBuilder builder, string[] args, ILogger logger)
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate(); // Applies existing migrations from the Migrations folder. Be sure to build and commit migrations before deploying.
                                       // DO NOT COMMIT DATABASE FILES.  .db, .db-wal, and .db-shm (added to gitignore)
                app.Logger.LogInformation($"Database Migration Completed in: Running in {app.Environment}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration or data rebuild. Deleting Database file.");

            string db_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db");
            string db_wal_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db-wal");
            string db_shm_path = Path.Combine(app.Environment.ContentRootPath, "msyu9gates.db-shm");

            if (File.Exists(db_path))
            {
                File.Delete(db_path);
                logger.LogWarning("Database file found: msyu9gates.db -- Deleting.");
            }
            if (File.Exists(db_wal_path))
            {
                File.Delete(db_wal_path);
                logger.LogWarning("Database WAL file found: msyu9gates.db-wal -- Deleting.");
            }
            if (File.Exists(db_shm_path))
            {
                File.Delete(db_shm_path);
                logger.LogWarning("Database SHM file found: msyu9gates.db-shm -- Deleting.");
            }

            if (args.Length == 0)
            {
                logger.LogError("Retrying migration after deleting database files.");
                Program.Main(new string[1] { "-retry" }); // Restart the application to attempt migration again.
            }
            else if (args.Length > 0 && args[0] == "-retry")
            {
                logger.LogError("Retry failed. Exiting.");
                return; // Exit the application if migration fails.
            }
        }
    }
}
