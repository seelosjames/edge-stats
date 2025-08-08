using EdgeStats.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EdgeStats.Dtos;

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
            Console.WriteLine("WE ARE HERE");

            var items = await _dbContext.WatchlistItems
                .Include(w => w.Line)
                .Select(w => new WatchlistItemDto
                {
                    LineId = w.LineId,
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddWatchlistItem([FromBody] WatchlistItemDto watchlistItemDto)
        {
            int lineId = watchlistItemDto.LineId;

            var line = await _dbContext.Lines.FindAsync(lineId);
            if (line == null) { return NotFound("Line not found"); }

            var newWatchlistItem = new WatchlistItem
            {
                LineId = lineId,
                UserId = "demo-user"
            };

            _dbContext.WatchlistItems.Add(newWatchlistItem);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetWatchlist),
                new { id = newWatchlistItem.LineId },
                new WatchlistItemDto { LineId = newWatchlistItem.LineId }
            );
        }

        [HttpDelete("{lineId}")]
        public async Task<IActionResult> DeleteWatchlistItem(int lineId)
        {
            var watchlistItem = await _dbContext.WatchlistItems.FirstOrDefaultAsync(w => w.LineId == lineId); ;
            if (watchlistItem == null) { return NotFound(); }

            _dbContext.WatchlistItems.Remove(watchlistItem);
            await _dbContext.SaveChangesAsync();

            return NoContent();

        }

    }
}
