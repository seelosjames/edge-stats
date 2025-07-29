using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EdgeStats.Controllers
{
    [ApiController]
    [Route("watchlist")]
    public class WatchlistItemController : Controller
    {
        private readonly EdgeStatsDbContext _dbContext;

        public WatchlistItemController(EdgeStatsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetWatchlist()
        {
            var items = await _dbContext.WatchlistItems.ToListAsync();

                return Ok(items);
        }

    }
}
