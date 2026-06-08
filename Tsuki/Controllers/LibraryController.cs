using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Tsuki.Data;
using Tsuki.ViewModels;

namespace Tsuki.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LibraryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Library
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var favorites = await _db.Favorites
                .Include(f => f.Novel)
                    .ThenInclude(n => n.Author)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var bookmarks = await _db.Bookmarks
                .Include(b => b.Novel)
                .Include(b => b.Chapter)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var history = await _db.ReadingHistories
                .Include(r => r.Novel)
                    .ThenInclude(n => n.Author)
                .Include(r => r.Chapter)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.LastReadAt)
                .ToListAsync();

            var vm = new LibraryViewModel
            {
                FavoriteNovels = favorites,
                BookmarkedChapters = bookmarks,
                ReadingHistories = history
            };

            return View(vm);
        }
    }
}
