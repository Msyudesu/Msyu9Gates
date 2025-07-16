using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Msyu9Gates.Data
{
    public class ApplicationDbContext : DbContext
    {
        IConfiguration _config;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        public DbSet<Gate> Gates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_config.GetConnectionString("SQLite"));
        }
    }
}
