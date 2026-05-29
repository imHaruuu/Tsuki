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
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IImageService _imageService;

        public AuthorController(ApplicationDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
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

            string? avatarUrl = vm.AvatarUrl;
            if (vm.AvatarImage != null)
            {
                try
                {
                    avatarUrl = await _imageService.SaveImageAsync(vm.AvatarImage, "avatars");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("AvatarImage", ex.Message);
                    return View(vm);
                }
            }

            _db.Authors.Add(new Author { Name = vm.Name, Bio = vm.Bio, AvatarUrl = avatarUrl });
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

            string? avatarUrl = vm.AvatarUrl;
            if (vm.AvatarImage != null)
            {
                try
                {
                    var newAvatarUrl = await _imageService.SaveImageAsync(vm.AvatarImage, "avatars");
                    if (!string.IsNullOrEmpty(newAvatarUrl))
                    {
                        if (!string.IsNullOrEmpty(author.AvatarUrl))
                        {
                            _imageService.DeleteImage(author.AvatarUrl);
                        }
                        avatarUrl = newAvatarUrl;
                    }
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("AvatarImage", ex.Message);
                    ViewBag.AuthorId = id;
                    return View(vm);
                }
            }

            author.Name = vm.Name;
            author.Bio = vm.Bio;
            author.AvatarUrl = avatarUrl;
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

            if (!string.IsNullOrEmpty(author.AvatarUrl))
            {
                _imageService.DeleteImage(author.AvatarUrl);
            }

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Author deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
