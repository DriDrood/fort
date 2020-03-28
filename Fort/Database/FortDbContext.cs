using Fort.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fort.Database
{
    public class FortDbContext : DbContext
    {
        // Add DbSets here
        public FortDbContext(DbContextOptions<FortDbContext> options) : base(options) { }
        public FortDbContext() : this(new DbContextOptions<FortDbContext>()) { }

        public DbSet<City> Cities { get; set; }
        public DbSet<CityOccupation> CityOccupations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Road> Roads { get; set; }
        public DbSet<StartingPosition> StartingPositions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            DbSeed.Cities(builder);
            DbSeed.Roads(builder);
            DbSeed.Teams(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(Startup.Configuration.ConnectionString);
        }
    }
}