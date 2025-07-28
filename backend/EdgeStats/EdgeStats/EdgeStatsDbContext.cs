using EdgeStats.models;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats
{
    public class EdgeStatsDbContext : DbContext
    {
        public EdgeStatsDbContext(DbContextOptions<EdgeStatsDbContext> options) : base(options) { }

        public DbSet<Line> Lines { get; set; }
        public DbSet<Prop> Props { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Sportsbook> Sportsbooks { get; set; }
        public DbSet<WatchlistItems> WatchlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add constraints/configurations here if needed
            base.OnModelCreating(modelBuilder);
        }
    }
}
