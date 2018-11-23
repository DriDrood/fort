using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fort.Database
{
    public class FortDbContext : DbContext
    {
        // Add DbSets here
        public FortDbContext(DbContextOptions<FortDbContext> options) : base(options) { }
        public FortDbContext() : this(new DbContextOptions<FortDbContext>()) { }

        public virtual DbSet<Fortress> Fortresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(Startup.ConnectionString);
        }
    }
}