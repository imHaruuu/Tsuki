using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Services;
using Tsuki.ViewModels;

namespace Tsuki.Controllers
{
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IReadingHistoryService _historyService;

        public ChapterController(ApplicationDbContext db, IReadingHistoryService historyService)
        {
            _db = db;
            _historyService = historyService;
        }

        // GET: /Chapter/Read/5
        // Supports both full page load and HTMX partial swap
        public async Task<IActionResult> Read(int id)
        {
            var chapter = await _db.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null) return NotFound();

            // Get prev and next chapters for navigation
            var siblings = await _db.Chapters
                .Where(c => c.NovelId == chapter.NovelId)
                .OrderBy(c => c.ChapterNumber)
                .Select(c => new { c.Id, c.ChapterNumber })
                .ToListAsync();

            var currentIndex = siblings.FindIndex(c => c.Id == id);
            var prev = currentIndex > 0 ? siblings[currentIndex - 1] : null;
            var next = currentIndex < siblings.Count - 1 ? siblings[currentIndex + 1] : null;

            var vm = new ChapterReadViewModel
            {
                Id = chapter.Id,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                Content = chapter.Content,
                WordCount = chapter.WordCount,
                NovelId = chapter.NovelId,
                NovelTitle = chapter.Novel.Title,
                NovelCoverUrl = chapter.Novel.CoverUrl,
                PrevChapterId = prev?.Id,
                NextChapterId = next?.Id,
                PrevChapterNumber = prev?.ChapterNumber,
                NextChapterNumber = next?.ChapterNumber
            };

            // Record reading history if user is logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                    await _historyService.RecordHistoryAsync(userId, chapter.NovelId, chapter.Id);
            }

            // If HTMX request, return partial view (just reader-wrap div)
            bool isHtmx = Request.Headers.ContainsKey("HX-Request");
            if (isHtmx)
            {
                // Update browser title via HX-Trigger response header
                Response.Headers.Append("HX-Push-Url", Url.Action("Read", "Chapter", new { id }) ?? $"/Chapter/Read/{id}");
                return PartialView("_ReadPartial", vm);
            }

            return View(vm);
        }
    }
}
