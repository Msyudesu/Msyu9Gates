using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Msyu9Gates.Data;
using Msyu9Gates.Lib.Models;

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

        public DbSet<IKeyModel> KeysDb { get; set; }
        public DbSet<IGateModel> GatesDb { get; set; }
        public DbSet<IChapterModel> ChaptersDb { get; set; }
        public DbSet<IUserModel> UsersDb { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_config.GetConnectionString("SQLite"));
        }
    }
}
