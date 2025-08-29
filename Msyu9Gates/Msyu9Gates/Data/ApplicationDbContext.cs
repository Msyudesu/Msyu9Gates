using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Data;

public class ApplicationDbContext : DbContext
{
    IConfiguration _config;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration config)
        : base(options)
    {
        _config = config;
    }

    public DbSet<KeyModel> KeysDb => Set<KeyModel>();
    public DbSet<GateModel> GatesDb => Set<GateModel>();
    public DbSet<ChapterModel> ChaptersDb => Set<ChapterModel>();
    public DbSet<UserModel> UsersDb => Set<UserModel>();
    public DbSet<AttemptModel> AttemptsDb => Set<AttemptModel>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_config.GetConnectionString("SQLite"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GateModel>()
            .HasIndex(g => g.GateNumber)
            .IsUnique();

        modelBuilder.Entity<ChapterModel>()
            .HasIndex(c => new { c.GateId, c.ChapterNumber })
            .IsUnique();

        modelBuilder.Entity<KeyModel>()
            .HasIndex(k => new { k.KeyNumber, k.KeyValue})
            .IsUnique();

        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.DiscordId)
            .IsUnique();

        modelBuilder.Entity<AttemptModel>()
            .HasIndex(a => new { a.UserId, a.GateId, a.ChapterId });

        base.OnModelCreating(modelBuilder);
    }
}
