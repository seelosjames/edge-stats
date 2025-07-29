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

        public enum GameStatus { Scheduled, InProgress, Completed, Cancelled }
        [Required]
        public GameStatus Status { get; set; } = GameStatus.Scheduled;
    }
}
