using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EdgeStats.Factories
{
	public class EdgeStatsDbContextFactory : IDesignTimeDbContextFactory<EdgeStatsDbContext>
	{
		public EdgeStatsDbContext CreateDbContext(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory()) // make sure this is correct path for your project root
				.AddJsonFile("appsettings.json")
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<EdgeStatsDbContext>();
			var connStr = config.GetConnectionString("DefaultConnection");

			optionsBuilder.UseNpgsql(connStr);

			return new EdgeStatsDbContext(optionsBuilder.Options);
		}
	}
}
