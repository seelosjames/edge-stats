namespace EdgeStats.Dtos
{
	public class ScrapedGameDto
	{
		public int LeagueId { get; set; }
		public string Team1 { get; set; }
		public string Team2 { get; set; }
		public DateTime GameTime { get; set; }
		public ScrapedGameUrlDto GameUrl { get; set; }
		public Guid GameUuid { get; set; }
	}
}
