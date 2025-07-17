using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(255)]
        public string TeamName { get; set; }

        [Required]
        public int LeagueId { get; set; }

        [ForeignKey("LeagueId")]
        public League League { get; set; }

        [InverseProperty("Team1")]
        public ICollection<Game> GamesAsTeam1 { get; set; }

        [InverseProperty("Team2")]
        public ICollection<Game> GamesAsTeam2 { get; set; }
    }
}
