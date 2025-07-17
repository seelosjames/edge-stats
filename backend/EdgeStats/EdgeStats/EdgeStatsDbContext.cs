using EdgeStats.models;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats
{
    public class SportsDbContext : DbContext
    {
        public SportsDbContext(DbContextOptions<SportsDbContext> options) : base(options) { }

        public DbSet<Line> Lines { get; set; }
        public DbSet<Prop> Props { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Sportsbook> Sportsbooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Add constraints/configurations here if needed
            base.OnModelCreating(modelBuilder);
        }
    }
}
