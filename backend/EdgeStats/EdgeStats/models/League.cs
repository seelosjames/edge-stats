using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EdgeStats.Models
{
	public class League
	{
		[Key]
		public int LeagueId { get; set; }

		[Required, MaxLength(50)]
		public string? LeagueName { get; set; }

        [Required, MaxLength(10)]
        public string LeagueCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
		public string? Sport { get; set; }

		[JsonIgnore]
		public ICollection<Team>? Teams { get; set; }

		[JsonIgnore]
		public ICollection<Game>? Games { get; set; }
	}
}
