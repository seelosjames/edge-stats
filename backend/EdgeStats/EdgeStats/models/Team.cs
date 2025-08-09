using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EdgeStats.Models
{
	public class Team
	{
		[Key]
		public int TeamId { get; set; }

		[Required]
		[MaxLength(255)]
		public string TeamName { get; set; }

		[Required]
		[ForeignKey("League")]
		public int LeagueId { get; set; }
		public League? League { get; set; }

		[JsonIgnore]
		[InverseProperty("Team1")]
		public ICollection<Game> GamesAsTeam1 { get; set; } = new List<Game>();

		[JsonIgnore]
		[InverseProperty("Team2")]
		public ICollection<Game> GamesAsTeam2 { get; set; } = new List<Game>();

	}
}
