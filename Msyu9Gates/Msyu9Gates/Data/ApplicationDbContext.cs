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

    public DbSet<GateKey> KeysDb => Set<GateKey>();
    public DbSet<Gate> GatesDb => Set<Gate>();
    public DbSet<Chapter> ChaptersDb => Set<Chapter>();
    public DbSet<User> UsersDb => Set<User>();
    public DbSet<Attempt> AttemptsDb => Set<Attempt>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_config.GetConnectionString("SQLite"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gate>()
            .HasIndex(g => g.GateNumber)
            .IsUnique();

        modelBuilder.Entity<Chapter>()
            .HasIndex(c => new { c.GateId, c.ChapterNumber })
            .IsUnique();

        modelBuilder.Entity<GateKey>()
            .HasIndex(k => new { k.KeyNumber, k.KeyValue})
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.DiscordId)
            .IsUnique();

        modelBuilder.Entity<Attempt>()
            .HasIndex(a => new { a.UserId, a.GateId, a.ChapterId });

        base.OnModelCreating(modelBuilder);
    }
}
