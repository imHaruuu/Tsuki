using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;
using Tsuki.ViewModels.Admin;

namespace Tsuki.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ChapterController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Chapter?novelId=5
        public async Task<IActionResult> Index(int? novelId)
        {
            var query = _db.Chapters.Include(c => c.Novel).AsQueryable();
            if (novelId.HasValue)
                query = query.Where(c => c.NovelId == novelId.Value);

            var chapters = await query.OrderBy(c => c.NovelId).ThenBy(c => c.ChapterNumber).ToListAsync();
            ViewBag.Novels = await _db.Novels.OrderBy(n => n.Title).ToListAsync();
            ViewBag.SelectedNovelId = novelId;
            return View(chapters);
        }

        // GET: /Admin/Chapter/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateChapterViewModel
            {
                AvailableNovels = await _db.Novels.OrderBy(n => n.Title).ToListAsync()
            };
            return View(vm);
        }

        // POST: /Admin/Chapter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateChapterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableNovels = await _db.Novels.OrderBy(n => n.Title).ToListAsync();
                return View(vm);
            }

            var chapter = new Chapter
            {
                NovelId = vm.NovelId,
                ChapterNumber = vm.ChapterNumber,
                Title = vm.Title,
                Content = vm.Content,
                WordCount = vm.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                CreatedAt = DateTime.UtcNow
            };
            _db.Chapters.Add(chapter);

            // Update novel's UpdatedAt
            var novel = await _db.Novels.FindAsync(vm.NovelId);
            if (novel != null) novel.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Chapter {chapter.ChapterNumber} created successfully.";
            return RedirectToAction(nameof(Index), new { novelId = vm.NovelId });
        }

        // GET: /Admin/Chapter/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var chapter = await _db.Chapters.FindAsync(id);
            if (chapter == null) return NotFound();

            var vm = new CreateChapterViewModel
            {
                NovelId = chapter.NovelId,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                Content = chapter.Content,
                AvailableNovels = await _db.Novels.OrderBy(n => n.Title).ToListAsync()
            };
            ViewBag.ChapterId = id;
            return View(vm);
        }

        // POST: /Admin/Chapter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateChapterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableNovels = await _db.Novels.OrderBy(n => n.Title).ToListAsync();
                ViewBag.ChapterId = id;
                return View(vm);
            }

            var chapter = await _db.Chapters.FindAsync(id);
            if (chapter == null) return NotFound();

            chapter.ChapterNumber = vm.ChapterNumber;
            chapter.Title = vm.Title;
            chapter.Content = vm.Content;
            chapter.WordCount = vm.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Chapter updated successfully.";
            return RedirectToAction(nameof(Index), new { novelId = chapter.NovelId });
        }

        // POST: /Admin/Chapter/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var chapter = await _db.Chapters.FindAsync(id);
            if (chapter == null) return NotFound();
            var novelId = chapter.NovelId;
            _db.Chapters.Remove(chapter);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Chapter deleted.";
            return RedirectToAction(nameof(Index), new { novelId });
        }
    }
}
