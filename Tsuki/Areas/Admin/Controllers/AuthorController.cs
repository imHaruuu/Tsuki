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
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthorController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _db.Authors.Include(a => a.Novels).OrderBy(a => a.Name).ToListAsync();
            return View(authors);
        }

        public IActionResult Create() => View(new CreateAuthorViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuthorViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _db.Authors.Add(new Author { Name = vm.Name, Bio = vm.Bio, AvatarUrl = vm.AvatarUrl });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Author \"{vm.Name}\" created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null) return NotFound();
            var vm = new CreateAuthorViewModel { Name = author.Name, Bio = author.Bio, AvatarUrl = author.AvatarUrl };
            ViewBag.AuthorId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateAuthorViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.AuthorId = id; return View(vm); }
            var author = await _db.Authors.FindAsync(id);
            if (author == null) return NotFound();
            author.Name = vm.Name;
            author.Bio = vm.Bio;
            author.AvatarUrl = vm.AvatarUrl;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Author updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null) return NotFound();
            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Author deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
