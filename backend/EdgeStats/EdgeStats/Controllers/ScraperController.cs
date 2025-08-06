using EdgeStats.Dtos;
using EdgeStats.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdgeStats.Controllers
{
	[ApiController]
	[Route("controller")]
	public class ScraperController : Controller
	{
		private readonly ScraperService _scraperService;

		public ScraperController(ScraperService scraper, EdgeStatsDbContext dbContext)
		{
			_scraperService = scraper;
		}

		[HttpPost("scrape")]
		public async Task<IActionResult> Scrape([FromBody] ScrapeRequestDto request)
		{
			if (request.Leagues.Count == 0 || request.Sportsbooks.Count == 0)
				return BadRequest("League and sportsbook must be provided.");

			await _scraperService.ScrapeAsync(request.Leagues, request.Sportsbooks);

			return Ok(new { message = "Scrape complete" });
		}
	}
}
