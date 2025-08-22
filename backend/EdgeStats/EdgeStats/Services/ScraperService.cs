using EdgeStats.Dtos;
using EdgeStats.Interfaces;
using EdgeStats.Models;
using EdgeStats.Scrapers;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats.Services
{
	public class ScraperService
	{
		private readonly EdgeStatsDbContext _dbContext;
        private readonly IScraperRepository _scraperRepository;

        public ScraperService(EdgeStatsDbContext dbContext, IScraperRepository scraperRepository)
		{
			_dbContext = dbContext;
            _scraperRepository = scraperRepository;
		}

		public async Task ScrapeAsync(List<string> leagues, List<string> sportsbooks)
		{
			await _scraperRepository.RemoveOldGamesAsync();

            foreach (var sportsbook in sportsbooks)
            {
                ISportsbookScraper? scraper = sportsbook.ToLower() switch
                {
                    "pinnacle" => new PinnacleScraper(_dbContext, "Pinnacle", _scraperRepository),
                     "fliff" => new FliffScraper(_dbContext, "Fliff", _scraperRepository),
                };

                if (scraper == null)
                {
                    Console.WriteLine($"No scraper found for: {sportsbook}");
                    continue;
                }
                
				await scraper.Scrape(leagues);
            }
		}
	}
}
