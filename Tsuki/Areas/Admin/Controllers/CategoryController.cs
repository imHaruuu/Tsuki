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
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .Include(c => c.NovelCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult Create() => View(new CreateCategoryViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _db.Categories.Add(new Category { Name = vm.Name, Slug = vm.Slug });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Category \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            var vm = new CreateCategoryViewModel { Name = cat.Name, Slug = cat.Slug };
            ViewBag.CategoryId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCategoryViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.CategoryId = id; return View(vm); }
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Name = vm.Name;
            cat.Slug = vm.Slug;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
