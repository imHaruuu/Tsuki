using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Tsuki.Services;

namespace Tsuki.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarkController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // POST: /Bookmark/Toggle (HTMX endpoint - returns _BookmarkButton partial)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int novelId, int chapterId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var isBookmarked = await _bookmarkService.ToggleBookmarkAsync(userId, novelId, chapterId);

            ViewBag.NovelId = novelId;
            ViewBag.ChapterId = chapterId;
            ViewBag.IsBookmarked = isBookmarked;

            return PartialView("~/Views/Shared/_BookmarkButton.cshtml");
        }

        // POST: /Bookmark/Remove (HTMX endpoint - removes bookmark from library page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int novelId, int chapterId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // Toggle will remove it since it already exists
            await _bookmarkService.ToggleBookmarkAsync(userId, novelId, chapterId);

            // Return empty content so HTMX removes/clears the target element
            return Content("");
        }
    }
}
