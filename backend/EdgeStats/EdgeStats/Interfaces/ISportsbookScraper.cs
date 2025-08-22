using EdgeStats.Models;

namespace EdgeStats.Interfaces
{
	public interface ISportsbookScraper
	{
		Task Scrape(List<string> leagues);
    }
}
