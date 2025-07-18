using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.models
{
    [Table("sportsbook")]
    public class Sportsbook
    {
        [Key]
        [Column("sportsbook_id")]
        public int SportsbookId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("sportsbook_name")]
        public string SportsbookName { get; set; }
    }
}
