using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EdgeStats.models
{
    [Table("team")]
    public class Team
    {
        [Key]
        [Column("team_id")]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("team_name")]
        public string TeamName { get; set; }

        [Required]
        [Column("league_id")]
        public int LeagueId { get; set; }
        public League League { get; set; }

        [JsonIgnore]
        [InverseProperty("Team1")]
        public ICollection<Game> GamesAsTeam1 { get; set; } = new List<Game>();

        [JsonIgnore]
        [InverseProperty("Team2")]
        public ICollection<Game> GamesAsTeam2 { get; set; } = new List<Game>();

    }
}
