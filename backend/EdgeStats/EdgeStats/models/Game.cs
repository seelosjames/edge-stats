using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
	public class Game
	{
		[Key]
		public int GameId { get; set; }

		[Required]
		public Guid GameUuid { get; set; }

		[Required]
		[ForeignKey("League")]
		public int LeagueId { get; set; }
		public League? League { get; set; }

		[Required]
		[ForeignKey(nameof(Team1))]
		public int Team1Id { get; set; }
		[InverseProperty("GamesAsTeam1")]
		public Team? Team1 { get; set; }

		[Required]
		[ForeignKey(nameof(Team2))]
		public int Team2Id { get; set; }
		[InverseProperty("GamesAsTeam2")]
		public Team? Team2 { get; set; }

		[Required]
		public DateTime GameDateTime { get; set; }

		public ICollection<Prop> Props { get; set; } = new List<Prop>();
        public ICollection<GameUrl> GameUrls { get; set; } = new List<GameUrl>();

    }
}
