
namespace EdgeStats.Scrapers
{
    public class TestScraper : ISportsbookScraper
    {
        private readonly EdgeStatsDbContext _db;

        public TestScraper(EdgeStatsDbContext dbContext)
        {
            _db = dbContext;
        }
        public async Task ScrapeAsync(List<string> leagues)
        {
            await _db.SaveChangesAsync();
        }
    }
}
