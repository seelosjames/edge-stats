using EdgeStats.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdgeStats.Dtos
{
    public class ScrapedGameUrlDto
    {
        public int SportsbookId { get; set; }
        public string GameUrl { get; set; }
    }
}
