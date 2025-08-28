using EdgeStats.Dtos;
using EdgeStats.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		[HttpGet("all")]
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

		[HttpGet("value")]
		public async Task<IActionResult> GetValueLines()
		{
			// 1) Get all lines grouped by Prop (and its Game context)
			var props = await _dbContext.Props
				.Include(p => p.Game)
					.ThenInclude(g => g.Team1)
				.Include(p => p.Game)
					.ThenInclude(g => g.Team2)
				.Include(p => p.Lines)
					.ThenInclude(l => l.Sportsbook)
				.ToListAsync();

			var results = new List<ValueLineDto>();

			foreach (var prop in props)
			{
				var game = prop.Game;

				// Group lines by sportsbook -> must have pairs for de-vig
				var byBook = prop.Lines
					.GroupBy(l => l.SportsbookId)
					.ToList();

				// Build a list of fair probs per sportsbook for consensus
				var fairProbs = new List<(decimal prob, decimal weight, Line line)>();

				foreach (var bookGroup in byBook)
				{
					if (bookGroup.Count() < 2) continue; // need at least 2 sides to de-vig properly

					var linesArr = bookGroup.ToArray();
					var qValues = linesArr.Select(l => 1m / l.Odd).ToArray();
					var sum = qValues.Sum();

					// de-vig
					for (int i = 0; i < linesArr.Length; i++)
					{
						var fair = qValues[i] / sum;
						var line = linesArr[i];
						var weight = line.Sportsbook.Weight; // assume Weight property exists, else use 1m

						fairProbs.Add((fair, weight, line));
					}
				}

				if (fairProbs.Count == 0) continue;

				// Consensus = weighted median (robust)
				var consensus = WeightedMedian(fairProbs.Select(fp => (fp.prob, fp.weight)));

				// Build results with Value%
				foreach (var fp in fairProbs)
				{
					var edge = (consensus - fp.prob) / fp.prob * 100m;

					results.Add(new ValueLineDto
					{
						LineId = fp.line.LineId,
						Description = fp.line.Description,
						Odd = fp.line.Odd,
						Value = edge,

						PropName = prop.PropName,
						PropType = prop.PropType,

						SportsbookName = fp.line.Sportsbook.SportsbookName,

						Team1 = game.Team1.TeamName,
						Team2 = game.Team2.TeamName,
						GameDateTime = game.GameDateTime
					});
				}
			}

			// Sort descending by Value
			var sorted = results.OrderByDescending(r => r.Value).ToList();

			return Ok(sorted);
		}

		// === helpers ===
		private static decimal WeightedMedian(IEnumerable<(decimal value, decimal weight)> items)
		{
			var list = items
				.Where(t => t.weight > 0)
				.OrderBy(t => t.value)
				.ToList();

			var totalW = list.Sum(t => t.weight);
			if (totalW == 0) return list[list.Count / 2].value;

			decimal acc = 0;
			foreach (var (v, w) in list)
			{
				acc += w;
				if (acc >= totalW / 2m) return v;
			}
			return list.Last().value;
		}

		public class ValueLineDto : LineDto
		{
			public decimal Value { get; set; }
		}
	}
}
