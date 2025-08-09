using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
    public class Sportsbook
    {
        [Key]
        public int SportsbookId { get; set; }

        [Required, MaxLength(255)]
        public string? SportsbookName { get; set; }

        [Required, MaxLength(255)]
        public string? SportsbookUrl { get; set; }

        public ICollection<SportsbookUrl> SportsbookUrls { get; set; } = new List<SportsbookUrl>();
        public ICollection<GameUrl> GameUrls { get; set; } = new List<GameUrl>();
    }
}
