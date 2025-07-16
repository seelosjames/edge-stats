using System.ComponentModel.DataAnnotations;

namespace EdgeStats.Data
{
    public class Line
    {
        [Key]
        public int LineId {  get; set; }

        [Required]
        public int PropId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Odd {  get; set; }

        [Required]
        public int LineUuid { get; set; }

        [Required]
        public int SportsbookId { get; set; }
    }
}
