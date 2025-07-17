using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class Prop
    {
        [Key]
        public int PropId { get; set; }

        [Required]
        public Guid PropUuid { get; set; }

        [Required]
        public int GameId { get; set; }

        // Assuming you have a Game model:
        [ForeignKey("GameId")]
        public Game Game { get; set; }

        [Required]
        [MaxLength(255)]
        public string PropName { get; set; }

        [MaxLength(100)]
        public string PropType { get; set; }
    }
}
