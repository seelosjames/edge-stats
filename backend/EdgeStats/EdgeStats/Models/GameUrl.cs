using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
    public class GameUrl
    {
        [Key]
        public int GameUrlId { get; set; }

        [Required]
        [ForeignKey(nameof(Game))]
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Sportsbook))]
        public int SportsbookId { get; set; }
        public Sportsbook Sportsbook { get; set; } = null!;

        [Required]
        [MaxLength(2048)]
        public string GameUrlValue { get; set; } = string.Empty; // renamed to avoid name clash
    }
}
