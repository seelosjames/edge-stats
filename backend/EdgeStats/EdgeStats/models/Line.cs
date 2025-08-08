using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Models
{
	public class Line
	{
		[Key]
		public int LineId { get; set; }

		[Required]
		public Guid LineUuid { get; set; }

		[Required]
		[ForeignKey("Prop")]
		public int PropId { get; set; }
		public Prop? Prop { get; set; }

		[Required]
		[ForeignKey("Sportsbook")]
		public int SportsbookId { get; set; }
		public Sportsbook? Sportsbook { get; set; }

		[Required]
		[MaxLength(255)]
		public string? Description { get; set; }

		[Required]
		[Column("odd", TypeName = "decimal(8,4)")]
		public decimal Odd { get; set; }
	}
}
