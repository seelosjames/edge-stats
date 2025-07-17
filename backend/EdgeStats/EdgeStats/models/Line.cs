using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    [Table("line")]
    public class Line
    {
        [Key]
        [Column("line_id")]
        public int LineId { get; set; }

        [Required]
        [Column("line_uuid")]
        public Guid LineUuid { get; set; }

        [Required]
        [Column("prop_id")]
        public int PropId { get; set; }

        [ForeignKey("PropId")]
        public Prop Prop { get; set; }

        [Required]
        [Column("sportsbook_id")]
        public int SportsbookId { get; set; }

        [ForeignKey("SportsbookId")]
        public Sportsbook Sportsbook { get; set; }

        [MaxLength(255)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("odd", TypeName = "decimal(8,4)")]
        public decimal Odd { get; set; }
    }
}
