using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
    public class Prop
    {
        [Key]
        public int PropId { get; set; }

        [Required]
        public Guid PropUuid { get; set; }

        [Required]
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public Game? Game { get; set; }

        [Required]
        [MaxLength(255)]
        public string? PropName { get; set; }

        [MaxLength(100)]
        public string? PropType { get; set; }

		public ICollection<Line> Lines { get; set; } = new List<Line>();
	}
}
