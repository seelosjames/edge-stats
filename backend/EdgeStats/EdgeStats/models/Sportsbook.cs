using System.ComponentModel.DataAnnotations;

namespace EdgeStats.models
{
    public class Sportsbook
    {
        [Key]
        public int SportsbookId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SportsbookName { get; set; }
    }
}
