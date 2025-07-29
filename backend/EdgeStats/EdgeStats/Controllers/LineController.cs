using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EdgeStats.Dtos;

namespace EdgeStats.Controllers
{
    [ApiController]
    [Route("lines")]
    public class LineController : Controller
    {
        private readonly EdgeStatsDbContext _dbContext;

        public LineController(EdgeStatsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            var items = await _dbContext.Lines
                .Include(l => l.Prop)
                    .ThenInclude(p => p.Game)
                        .ThenInclude(g => g.Team1)
                .Include(l => l.Prop)
                    .ThenInclude(p => p.Game)
                        .ThenInclude(g => g.Team2)
                .Include(l => l.Sportsbook)
                .Select(l => new LineDto
                {
                    LineId = l.LineId,
                    Description = l.Description,
                    Odd = l.Odd,

                    PropName = l.Prop.PropName,
                    PropType = l.Prop.PropType,

                    SportsbookName = l.Sportsbook.SportsbookName,

                    Team1 = l.Prop.Game.Team1.TeamName,
                    Team2 = l.Prop.Game.Team2.TeamName,

                    GameDateTime = l.Prop.Game.GameDateTime
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
