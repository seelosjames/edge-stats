using System.ComponentModel.DataAnnotations;

namespace EdgeStats.models
{
    public class League
    {
        [Key]
        public int LeagueId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LeagueName { get; set; }

        [Required]
        [MaxLength(100)]
        public string SportType { get; set; }

        public ICollection<Team> Teams { get; set; }
        public ICollection<Game> Games { get; set; }
    }
}
