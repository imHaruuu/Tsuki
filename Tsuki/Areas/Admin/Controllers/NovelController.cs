using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;
using Tsuki.Services;
using Tsuki.ViewModels.Admin;

namespace Tsuki.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NovelController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public NovelController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        // GET: /Admin/Novel
        public async Task<IActionResult> Index()
        {
            var novels = await _db.Novels
                .Include(n => n.Author)
                .Include(n => n.Chapters)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
            return View(novels);
        }

        // GET: /Admin/Novel/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateNovelViewModel
            {
                AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync(),
                AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
            };
            return View(vm);
        }

        // POST: /Admin/Novel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNovelViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync();
                vm.AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            string? coverUrl = vm.CoverUrl;
            if (vm.CoverImage != null)
            {
                try
                {
                    coverUrl = await _imageService.SaveImageAsync(vm.CoverImage, "covers");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("CoverImage", ex.Message);
                    vm.AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync();
                    vm.AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                    return View(vm);
                }
            }

            var novel = new Novel
            {
                Title = vm.Title,
                Description = vm.Description,
                CoverUrl = coverUrl,
                Status = vm.Status,
                AuthorId = vm.AuthorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Novels.Add(novel);
            await _db.SaveChangesAsync();

            // Add categories
            foreach (var catId in vm.SelectedCategoryIds)
                _db.NovelCategories.Add(new NovelCategory { NovelId = novel.Id, CategoryId = catId });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Novel \"{novel.Title}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Novel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var novel = await _db.Novels
                .Include(n => n.NovelCategories)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (novel == null) return NotFound();

            var vm = new CreateNovelViewModel
            {
                Title = novel.Title,
                Description = novel.Description,
                CoverUrl = novel.CoverUrl,
                Status = novel.Status,
                AuthorId = novel.AuthorId,
                SelectedCategoryIds = novel.NovelCategories.Select(nc => nc.CategoryId).ToList(),
                AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync(),
                AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
            };
            ViewBag.NovelId = id;
            return View(vm);
        }

        // POST: /Admin/Novel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateNovelViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync();
                vm.AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                ViewBag.NovelId = id;
                return View(vm);
            }

            var novel = await _db.Novels
                .Include(n => n.NovelCategories)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (novel == null) return NotFound();

            string? coverUrl = vm.CoverUrl;
            if (vm.CoverImage != null)
            {
                try
                {
                    var newCoverUrl = await _imageService.SaveImageAsync(vm.CoverImage, "covers");
                    if (!string.IsNullOrEmpty(newCoverUrl))
                    {
                        // Clean up old local image
                        if (!string.IsNullOrEmpty(novel.CoverUrl))
                        {
                            _imageService.DeleteImage(novel.CoverUrl);
                        }
                        coverUrl = newCoverUrl;
                    }
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("CoverImage", ex.Message);
                    vm.AvailableAuthors = await _db.Authors.OrderBy(a => a.Name).ToListAsync();
                    vm.AvailableCategories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                    ViewBag.NovelId = id;
                    return View(vm);
                }
            }

            novel.Title = vm.Title;
            novel.Description = vm.Description;
            novel.CoverUrl = coverUrl;
            novel.Status = vm.Status;
            novel.AuthorId = vm.AuthorId;
            novel.UpdatedAt = DateTime.UtcNow;

            // Update categories
            _db.NovelCategories.RemoveRange(novel.NovelCategories);
            foreach (var catId in vm.SelectedCategoryIds)
                _db.NovelCategories.Add(new NovelCategory { NovelId = id, CategoryId = catId });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Novel \"{novel.Title}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Novel/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var novel = await _db.Novels.FindAsync(id);
            if (novel == null) return NotFound();

            if (!string.IsNullOrEmpty(novel.CoverUrl))
            {
                _imageService.DeleteImage(novel.CoverUrl);
            }

            _db.Novels.Remove(novel);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Novel deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
