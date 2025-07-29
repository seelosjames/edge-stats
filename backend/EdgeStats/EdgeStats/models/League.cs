using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EdgeStats.models
{
    public class League
    {
        [Key]
        public int LeagueId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? LeagueName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? SportType { get; set; }

        [JsonIgnore]
        public ICollection<Team>? Teams { get; set; }

        [JsonIgnore]
        public ICollection<Game>? Games { get; set; }
    }
}
