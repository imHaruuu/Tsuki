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
        private readonly IBookmarkService _bookmarkService;

        public ChapterController(ApplicationDbContext db, IReadingHistoryService historyService, IBookmarkService bookmarkService)
        {
            _db = db;
            _historyService = historyService;
            _bookmarkService = bookmarkService;
        }

        // GET: /Chapter/Read/5
        // Supports both full page load and HTMX partial swap
        public async Task<IActionResult> Read(int id)
        {
            var chapter = await _db.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null) return NotFound();

            // Increment parent novel view count when a chapter is read
            chapter.Novel.ViewCount++;
            await _db.SaveChangesAsync();

            // Get prev and next chapters for navigation
            var siblings = await _db.Chapters
                .Where(c => c.NovelId == chapter.NovelId)
                .OrderBy(c => c.ChapterNumber)
                .Select(c => new { c.Id, c.ChapterNumber })
                .ToListAsync();

            var currentIndex = siblings.FindIndex(c => c.Id == id);
            var prev = currentIndex > 0 ? siblings[currentIndex - 1] : null;
            var next = currentIndex < siblings.Count - 1 ? siblings[currentIndex + 1] : null;

            var rawContent = chapter.Content ?? string.Empty;
            bool isHtml = IsHtmlContent(rawContent);
            string renderedContent = isHtml
                ? rawContent
                : PlainTextToHtml(rawContent);

            bool isBookmarked = false;
            // Record reading history if user is logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    await _historyService.RecordHistoryAsync(userId, chapter.NovelId, chapter.Id);
                    isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, chapter.Id);
                }
            }

            var vm = new ChapterReadViewModel
            {
                Id = chapter.Id,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                Content = renderedContent,
                IsHtmlContent = isHtml,
                WordCount = chapter.WordCount,
                NovelId = chapter.NovelId,
                NovelTitle = chapter.Novel.Title,
                NovelCoverUrl = chapter.Novel.CoverUrl,
                PrevChapterId = prev?.Id,
                NextChapterId = next?.Id,
                PrevChapterNumber = prev?.ChapterNumber,
                NextChapterNumber = next?.ChapterNumber,
                IsBookmarked = isBookmarked
            };

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

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Detects whether the content string contains HTML markup.
        /// Uses a quick heuristic: presence of common block/inline HTML tags.
        /// </summary>
        private static bool IsHtmlContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            var trimmed = content.TrimStart();
            // Detect leading block tags or any <p>, <br>, <div>, <h1-6>, <img>, <ul>, <ol>
            return System.Text.RegularExpressions.Regex.IsMatch(
                trimmed,
                @"<(p|br|div|span|h[1-6]|img|ul|ol|li|blockquote|pre|table|strong|em|a)\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Converts plain text (with newlines as paragraph separators) into
        /// well-formed HTML paragraphs. Double newlines become new paragraphs;
        /// single newlines become &lt;br&gt; within a paragraph.
        /// </summary>
        private static string PlainTextToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Normalise CRLF → LF
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // Split on blank lines (paragraph breaks)
            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new System.Text.StringBuilder();
            foreach (var para in paragraphs)
            {
                var trimmed = para.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Within each paragraph, single newlines → <br>
                var encoded = System.Net.WebUtility.HtmlEncode(trimmed)
                                    .Replace("\n", "<br />");
                sb.Append("<p>").Append(encoded).AppendLine("</p>");
            }
            return sb.ToString();
        }
    }
}
