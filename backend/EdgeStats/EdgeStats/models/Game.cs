using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    [Table("game")]
    public class Game
    {
        [Key]
        [Column("game_id")]
        public int GameId { get; set; }

        [Required]
        [Column("game_uuid")]
        public Guid GameUuid { get; set; }

        [Required]
        [Column("league_id")]
        public int LeagueId { get; set; }
        public League League { get; set; }

        [Required]
        [Column("team_1")]
        public int Team1Id { get; set; }
        [InverseProperty("GamesAsTeam1")]
        public Team Team1 { get; set; }

        [Required]
        [Column("team_2")]
        public int Team2Id { get; set; }
        [InverseProperty("GamesAsTeam2")]
        public Team Team2 { get; set; }

        [Required]
        [Column("game_datetime")]
        public DateTime GameDateTime { get; set; }
    }
}
