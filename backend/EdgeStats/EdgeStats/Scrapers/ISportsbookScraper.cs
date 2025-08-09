using EdgeStats.Models;

namespace EdgeStats.Scrapers
{
	public interface ISportsbookScraper
	{
		Task Scrape(List<string> leagues);
    }
}
