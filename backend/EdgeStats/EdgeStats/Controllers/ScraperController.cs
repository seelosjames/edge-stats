using EdgeStats.Dtos;
using EdgeStats.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdgeStats.Controllers
{
	[ApiController]
	[Route("scraper")]
	public class ScraperController : Controller
	{
		private readonly ScraperService _scraperService;

		public ScraperController(ScraperService scraper)
		{
			_scraperService = scraper;
		}

		[HttpPost]
		public async Task<IActionResult> Scrape([FromBody] ScrapeRequestDto request)
		{
			if (request.Leagues.Count == 0 || request.Sportsbooks.Count == 0)
				return BadRequest("League and sportsbook must be provided.");

			await _scraperService.ScrapeAsync(request.Leagues, request.Sportsbooks, _scraperService);

			return Ok(new { message = "Scrape complete" });
		}
	}
}
