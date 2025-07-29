using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class WatchlistItems
    {
        [Key]
        public int WatchListItemsId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("Line")]
        [Required]
        public int LineId { get; set; }
        public Line Line { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


