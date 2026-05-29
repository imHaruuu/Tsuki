using Microsoft.AspNetCore.Mvc;
using Tsuki.Services;

namespace Tsuki.Controllers
{
    public class NovelController : Controller
    {
        private readonly INovelService _novelService;
        private readonly IFavoriteService _favoriteService;

        public NovelController(INovelService novelService, IFavoriteService favoriteService)
        {
            _novelService = novelService;
            _favoriteService = favoriteService;
        }

        // GET: /Novel
        public async Task<IActionResult> Index(string? q, int? category, int page = 1)
        {
            var vm = await _novelService.GetNovelsAsync(q, category, page, pageSize: 12);
            return View(vm);
        }

        // GET: /Novel/Search  (HTMX partial endpoint)
        public async Task<IActionResult> Search(string? q, int? category, int page = 1)
        {
            var vm = await _novelService.GetNovelsAsync(q, category, page, pageSize: 12);
            return PartialView("_NovelList", vm);
        }

        // GET: /Novel/NavbarSearch  (HTMX navbar dropdown endpoint)
        public async Task<IActionResult> NavbarSearch(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Content(""); // Hide dropdown by returning empty when query is empty
            }
            var vm = await _novelService.GetNovelsAsync(q, categoryId: null, page: 1, pageSize: 5);
            return PartialView("_NavbarSearchDropdown", vm.Novels);
        }

        // GET: /Novel/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var vm = await _novelService.GetNovelDetailAsync(id);
            if (vm == null) return NotFound();

            // Check if current user has favorited this novel
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                    vm.IsFavorited = await _favoriteService.IsFavoritedAsync(userId, id);
            }

            return View(vm);
        }
    }
}
