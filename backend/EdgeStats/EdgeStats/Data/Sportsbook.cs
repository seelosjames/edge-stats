using System.ComponentMode.DataAnnotations;

namespace EdgeStats.Data
{
    public class Sportsbook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SportsbookName { get; set; }
    }
}
