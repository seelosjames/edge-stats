using EdgeStats.models;
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
            var items = await _dbContext.WatchlistItems
                .Include(l => l.Line)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddWatchlistItem(int lineId)
        {
            var line = await _dbContext.Lines.FindAsync(lineId);
            if (line == null) { return NotFound("Line not found"); }

            var newWatchlistItem = new WatchlistItem
            {
                LineId = lineId,
                UserId = "demo-user"
            };

            _dbContext.WatchlistItems.Add(newWatchlistItem);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWatchlist), new { id = newWatchlistItem.LineId }, newWatchlistItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWatchlistItem(int watchListItemsId)
        {
            var watchlistItem = await _dbContext.WatchlistItems.FindAsync(watchListItemsId);
            if (watchlistItem == null) { return NotFound(); }

            _dbContext.WatchlistItems.Remove(watchlistItem);
            await _dbContext.SaveChangesAsync();

            return NoContent();

        }

    }
}
