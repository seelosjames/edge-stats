using Microsoft.AspNetCore.Mvc;

namespace EdgeStats.Controllers
{
    [ApiController]
    [Route("watchlist")]
    public class WatchlistItemController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
