using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    [Table("prop")]
    public class Prop
    {
        [Key]
        [Column("prop_id")]
        public int PropId { get; set; }

        [Required]
        [Column("prop_uuid")]
        public Guid PropUuid { get; set; }

        [Required]
        [Column("game_id")]
        public int GameId { get; set; }
        public Game Game { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("prop_name")]
        public string PropName { get; set; }

        [MaxLength(100)]
        [Column("prop_type")]
        public string PropType { get; set; }
    }
}
