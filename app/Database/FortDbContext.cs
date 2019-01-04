using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fort.Database
{
    public class FortDbContext : DbContext
    {
        // Add DbSets here
        public FortDbContext(DbContextOptions<FortDbContext> options) : base(options) { }
        public FortDbContext() : this(new DbContextOptions<FortDbContext>()) { }

        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Path> Paths { get; set; }
        public virtual DbSet<Round> Rounds { get; set; }
        public virtual DbSet<StartingPosition> StartingPositions { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<Turn> Turns { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<City>()
                .HasMany(c => c.SourceToPaths)
                .WithOne(p => p.Source);
            builder.Entity<City>()
                .HasMany(c => c.TargetToPaths)
                .WithOne(p => p.Target);
                
            builder.Entity<City>()
                .HasMany(c => c.SourceToTurns)
                .WithOne(p => p.SourceCity);
            builder.Entity<City>()
                .HasMany(c => c.TargetToTurns)
                .WithOne(p => p.TargetCity);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(Startup.ConnectionString);
        }
    }
}