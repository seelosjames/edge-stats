using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        public Guid GameUuid { get; set; }

        [Required]
        public int LeagueId { get; set; }

        [ForeignKey("LeagueId")]
        public League League { get; set; }

        [Required]
        public int Team1Id { get; set; }

        [ForeignKey("Team1Id")]
        [InverseProperty("GamesAsTeam1")]
        public Team Team1 { get; set; }

        [Required]
        public int Team2Id { get; set; }

        [ForeignKey("Team2Id")]
        [InverseProperty("GamesAsTeam2")]
        public Team Team2 { get; set; }

        [Required]
        public DateTime GameDateTime { get; set; }
    }
}
