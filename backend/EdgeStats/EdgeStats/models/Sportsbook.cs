using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    public class Sportsbook
    {
        [Key]
        public int SportsbookId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? SportsbookName { get; set; }
    }
}
