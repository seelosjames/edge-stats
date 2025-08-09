using EdgeStats.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats
{
	public class EdgeStatsDbContext : IdentityDbContext<ApplicationUser>
	{
		public EdgeStatsDbContext(DbContextOptions<EdgeStatsDbContext> options) : base(options) { }

		public DbSet<Line> Lines { get; set; }
		public DbSet<Prop> Props { get; set; }
		public DbSet<Game> Games { get; set; }
		public DbSet<Team> Teams { get; set; }
		public DbSet<League> Leagues { get; set; }
		public DbSet<Sportsbook> Sportsbooks { get; set; }
        public DbSet<SportsbookUrl> SportsbookUrls { get; set; }
        public DbSet<GameUrl> GameUrls { get; set; }
        public DbSet<WatchlistItem> WatchlistItems { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
