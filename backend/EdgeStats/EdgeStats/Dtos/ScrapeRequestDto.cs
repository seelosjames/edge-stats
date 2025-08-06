namespace EdgeStats.Dtos
{
	public class ScrapeRequestDto
	{
		public List<string> Leagues { get; set; } = new();
		public List<string> Sportsbooks { get; set; } = new();
	}
}
