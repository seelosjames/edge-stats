using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EdgeStats.models
{
    [Table("league")]
    public class League
    {
        [Key]
        [Column("league_id")]
        public int LeagueId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("league_name")]
        public string LeagueName { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sport_type")]
        public string SportType { get; set; }

        [JsonIgnore]
        public ICollection<Team> Teams { get; set; }

        [JsonIgnore]
        public ICollection<Game> Games { get; set; }
    }
}
