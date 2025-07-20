using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Msyu9Gates.Data.Models;

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

        public DbSet<KeyModel> KeysDb { get; set; }
        public DbSet<GateModel> GatesDb { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_config.GetConnectionString("SQLite"));
        }
    }
}
