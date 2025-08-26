using EdgeStats.Interfaces;
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

        public async Task ScrapeAsync(List<string> leagues, List<string> sportsbookNames)
        {
            await _scraperRepository.RemoveOldGamesAsync();

            var sportsbooks = await _dbContext.Sportsbooks
                .Where(s => sportsbookNames.Contains(s.SportsbookName))
                .ToListAsync();

            foreach (var sportsbook in sportsbooks)
            {
                ISportsbookScraper? scraper = sportsbook.SportsbookName.ToLower() switch
                {
                    "pinnacle" => new PinnacleScraper(sportsbook, _scraperRepository),
                    "fliff" => new FliffScraper(sportsbook, _scraperRepository),
                    _ => null
                };

                if (scraper == null)
                {
                    Console.WriteLine($"No scraper found for: {sportsbook.SportsbookName}");
                    continue;
                }

                await scraper.Scrape(leagues);
            }
        }
    }
}