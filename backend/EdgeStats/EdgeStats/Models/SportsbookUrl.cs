using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
    public class SportsbookUrl
    {
        [Key]
        public int UrlId { get; set; }

        [Required]
        [ForeignKey(nameof(Sportsbook))]
        public int SportsbookId { get; set; }
        public Sportsbook Sportsbook { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(League))]
        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        [Required]
        [MaxLength(2048)]
        public string Url { get; set; } = string.Empty;
    }
}
