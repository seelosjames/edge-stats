using EdgeStats.Scrapers;

namespace EdgeStats.Services
{
	public class ScraperService
	{
		private readonly EdgeStatsDbContext _dbContext;

		public ScraperService(EdgeStatsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task ScrapeAsync(List<string> leagues, List<string> sportsbooks)
		{
			if (sportsbooks.Count == 0)
			{
				ISportsbookScraper? scraper = new TestScraper(_dbContext);
                await scraper.ScrapeAsync(leagues);
			}
            else
            {
                foreach (var sportsbook in sportsbooks)
                {
                    ISportsbookScraper? scraper = sportsbook.ToLower() switch
                    {
                        "pinnacle" => new PinnacleScraper(_dbContext),
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
}
