namespace EdgeStats.Dtos
{
    public class LineDto
    {
        public int LineId { get; set; }
        public string Description { get; set; }
        public double Odd { get; set; }

        public string? PropName { get; set; }
        public string? PropType { get; set; }

        public string? SportsbookName { get; set; }

        public string? Team1 { get; set; }
        public string? Team2 { get; set; }

        public DateTime? GameDateTime { get; set; }
        public string GameStatus { get; set; }
    }

}
