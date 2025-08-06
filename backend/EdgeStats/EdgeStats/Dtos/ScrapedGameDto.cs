namespace EdgeStats.Dtos
{
	public class ScrapedGameDto
	{
		public string League { get; set; }
		public string Sportsbook { get; set; }
		public string Team1 { get; set; }
		public string Team2 { get; set; }
		public DateTime GameTime { get; set; }
		public string GameUrl { get; set; }
		public string GameUuid { get; set; }
	}
}
