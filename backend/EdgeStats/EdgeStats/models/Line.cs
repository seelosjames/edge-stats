using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class Line
    {
        [Key]
        public int LineId { get; set; }

        [Required]
        public Guid LineUuid { get; set; }

        [Required]
        public int PropId { get; set; }

        [ForeignKey("PropId")]
        public Prop Prop { get; set; }

        [Required]
        public int SportsbookId { get; set; }

        [ForeignKey("SportsbookId")]
        public Sportsbook Sportsbook { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,4)")]
        public decimal Odd { get; set; }
    }
}
