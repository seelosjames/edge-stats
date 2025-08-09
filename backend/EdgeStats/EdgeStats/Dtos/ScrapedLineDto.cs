namespace EdgeStats.Dtos
{
	public class ScrapedLineDto
	{
		public Guid LineUuid { get; set; }
		public string PropId { get; set; }
		public Guid PropUuid { get; set; }
		public string Description { get; set; }
		public Double Odd { get; set; }
		public string Sportsbook { set; get; }
	}
}
