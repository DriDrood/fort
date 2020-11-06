using Fort.Database.Entities;
using Fort.Managers;
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
            // city occupation
            builder.Entity<CityOccupation>()
                .HasKey(e => new { e.CityId, e.TurnId });
            builder.Entity<CityOccupation>()
                .HasOne(e => e.City)
                .WithMany(e => e.CityOccupations)
                .HasForeignKey(e => e.CityId);
            builder.Entity<CityOccupation>()
                .HasOne(e => e.Owner)
                .WithMany(e => e.CityOccupations)
                .HasForeignKey(e => e.OwnerId);
            builder.Entity<CityOccupation>()
                .HasOne(e => e.Turn)
                .WithMany(e => e.CityOccupations)
                .HasForeignKey(e => e.TurnId);

            // order
            builder.Entity<Order>()
                .HasKey(e => new { e.StCityId, e.NdCityId, e.TurnId, e.StIsSource });
            builder.Entity<Order>()
                .HasOne(e => e.StCity)
                .WithMany(e => e.StForOrders)
                .HasForeignKey(e => e.StCityId);
            builder.Entity<Order>()
                .HasOne(e => e.NdCity)
                .WithMany(e => e.NdForOrders)
                .HasForeignKey(e => e.NdCityId);
            builder.Entity<Order>()
                .HasOne(e => e.StCityOccupation)
                .WithMany(e => e.StForOrders)
                .HasForeignKey(e => new { e.StCityId, e.TurnId });
            builder.Entity<Order>()
                .HasOne(e => e.NdCityOccupation)
                .WithMany(e => e.NdForOrders)
                .HasForeignKey(e => new { e.NdCityId, e.TurnId });
            builder.Entity<Order>()
                .HasOne(e => e.Road)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => new { e.StCityId, e.NdCityId });
            builder.Entity<Order>()
                .HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId);
            builder.Entity<Order>()
                .HasOne(e => e.Turn)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.TurnId);

            // road
            builder.Entity<Road>()
                .HasKey(e => new { e.StCityId, e.NdCityId });
            builder.Entity<Road>()
                .HasOne(e => e.StCity)
                .WithMany(e => e.StForRoads)
                .HasForeignKey(e => e.StCityId);
            builder.Entity<Road>()
                .HasOne(e => e.NdCity)
                .WithMany(e => e.NdForRoads)
                .HasForeignKey(e => e.NdCityId);

            // starting position
            builder.Entity<StartingPosition>()
                .HasKey(e => e.CityId);
            builder.Entity<StartingPosition>()
                .HasOne(e => e.City)
                .WithMany(e => e.StartingPositionFor)
                .HasForeignKey(e => e.CityId);
            builder.Entity<StartingPosition>()
                .HasOne(e => e.User)
                .WithMany(e => e.StartingPositions)
                .HasForeignKey(e => e.UserId);

            // turn
            builder.Entity<Turn>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            // user
            builder.Entity<User>()
                .HasOne(e => e.Team)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.TeamId);
            builder.Entity<User>()
                .HasIndex(e => e.UserName)
                .IsUnique();
            builder.Entity<User>()
                .HasIndex(e => e.Email)
                .IsUnique();

            DbSeed.Cities(builder);
            DbSeed.Roads(builder);
            DbSeed.Teams(builder);
            DbSeed.Users(builder);
            DbSeed.StartingPositions(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(ConfigManager.ConnectionString);
        }
    }
}