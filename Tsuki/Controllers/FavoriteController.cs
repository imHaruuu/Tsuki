using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tsuki.Services;

namespace Tsuki.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        // POST: /Favorite/Toggle  (HTMX endpoint — returns partial view)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int novelId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var isFavorited = await _favoriteService.ToggleFavoriteAsync(userId, novelId);

            // Return the updated favorite button partial
            ViewBag.NovelId = novelId;
            ViewBag.IsFavorited = isFavorited;
            return PartialView("_FavoriteButton");
        }
    }
}
