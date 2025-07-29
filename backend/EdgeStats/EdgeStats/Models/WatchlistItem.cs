using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class WatchlistItem
    {
        [Key]
        public int WatchListItemsId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        [ForeignKey("Line")]
        public int LineId { get; set; }
        public Line? Line { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


