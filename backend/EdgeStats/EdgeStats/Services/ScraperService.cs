using EdgeStats.Scrapers;

namespace EdgeStats.Services
{
	public class ScraperService
	{
		private readonly EdgeStatsDbContext _db;

		public ScraperService(EdgeStatsDbContext db)
		{
			_db = db;
		}

		public async Task ScrapeAsync(List<string> leagues, List<string> sportsbooks)
		{
			foreach (var sportsbook in sportsbooks)
			{
				ISportsbookScraper? scraper = sportsbook.ToLower() switch
				{
					"pinnacle" => new PinnacleScraper(_db),
					// "fliff" => new FliffScraper(_db),
					_ => null
				};

				if (scraper == null)
				{
					Console.WriteLine($"No scraper found for: {sportsbook}");
					continue;
				}

				await scraper.ScrapeAsync(leagues);
			}
		}
	}
}
