namespace EdgeStats.Scrapers
{
	public interface ISportsbookScraper
	{
		Task ScrapeAsync(List<string> leagues);
	}
}
