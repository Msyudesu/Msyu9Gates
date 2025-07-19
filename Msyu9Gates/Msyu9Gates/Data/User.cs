using Microsoft.EntityFrameworkCore;

namespace Msyu9Gates.Data
{
    public class User
    {
        public int Id { get; set; }
        public string DiscordId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }

    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.DiscordId).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Username).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Avatar).IsRequired();
        }
    }
}
