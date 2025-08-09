using EdgeStats.Scrapers;
using Microsoft.EntityFrameworkCore;

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

			await RemoveOldGamesAsync();

            foreach (var sportsbook in sportsbooks)
            {
                ISportsbookScraper? scraper = sportsbook.ToLower() switch
                {
                    "pinnacle" => new PinnacleScraper(_dbContext, "Pinnacle"),
                    // "fliff" => new FliffScraper(_db),
                    _ => null
                };

                if (scraper == null)
                {
                    Console.WriteLine($"No scraper found for: {sportsbook}");
                    continue;
                }
                
				await scraper.Scrape(leagues);
            }
		}

		private async Task RemoveOldGamesAsync()
		{
			var cutoff = DateTime.UtcNow;
			var oldGames = await _dbContext.Games
				.Where(g => g.GameDateTime < cutoff)
				.ToListAsync();

			if (oldGames.Count > 0)
			{
				_dbContext.Games.RemoveRange(oldGames);
				await _dbContext.SaveChangesAsync();
				Console.WriteLine($"Deleted {oldGames.Count} old games.");
			}
		}
	}
}
