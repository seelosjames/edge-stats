namespace EdgeStats.Dtos
{
	public class ScrapedLineDto
	{
		public string LineUuid { get; set; }
		public string PropId { get; set; }
		public string PropUuid { get; set; }
		public string Description { get; set; }
		public Double Odd { get; set; }
		public string Sportsbook { set; get; }
	}
}
