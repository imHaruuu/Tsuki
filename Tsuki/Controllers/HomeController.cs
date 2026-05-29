using Microsoft.AspNetCore.Mvc;
using Tsuki.Models;
using Tsuki.Services;

namespace Tsuki.Controllers
{
    public class HomeController : Controller
    {
        private readonly INovelService _novelService;

        public HomeController(INovelService novelService)
        {
            _novelService = novelService;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _novelService.GetFeaturedNovelsAsync(6);
            var recent = await _novelService.GetRecentlyUpdatedNovelsAsync(8);
            ViewBag.FeaturedNovels = featured;
            ViewBag.RecentNovels = recent;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }
}
