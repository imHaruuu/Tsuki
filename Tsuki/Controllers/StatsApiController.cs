using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;

namespace Tsuki.Controllers
{
    [ApiController]
    [Route("api/stats")]
    public class StatsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates API key using ADMIN_API_KEY from .env file
        /// </summary>
        private bool IsAuthorized()
        {
            var expectedApiKey = Environment.GetEnvironmentVariable("ADMIN_API_KEY");
            if (string.IsNullOrEmpty(expectedApiKey))
            {
                // If the key is not configured in .env, we default to allowing access 
                // but we can log a warning or enforce safety. For student defense ease, 
                // we allow it if unset, otherwise we validate.
                return true;
            }

            // Check custom X-API-KEY header
            if (Request.Headers.TryGetValue("X-API-KEY", out var headerValue) && headerValue.ToString() == expectedApiKey)
            {
                return true;
            }

            // Check query parameter (convenient for quick browser tests)
            if (Request.Query.TryGetValue("apiKey", out var queryValue) && queryValue.ToString() == expectedApiKey)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// GET: /api/stats/overview
        /// Returns high-level database stats.
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            if (!IsAuthorized())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Unauthorized: Invalid or missing API Key" });
            }

            var totalNovels = await _context.Novels.CountAsync();
            var totalChapters = await _context.Chapters.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalViews = await _context.Novels.SumAsync(n => n.ViewCount);
            var totalFavorites = await _context.Favorites.CountAsync();
            var totalBookmarks = await _context.Bookmarks.CountAsync();

            var statusDistribution = await _context.Novels
                .GroupBy(n => n.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                overview = new
                {
                    totalNovels,
                    totalChapters,
                    totalUsers,
                    totalViews,
                    totalFavorites,
                    totalBookmarks
                },
                statusDistribution
            });
        }

        /// <summary>
        /// GET: /api/stats/novels
        /// Returns top novels ordered by view counts.
        /// </summary>
        [HttpGet("novels")]
        public async Task<IActionResult> GetNovelsStats([FromQuery] int limit = 10)
        {
            if (!IsAuthorized())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Unauthorized: Invalid or missing API Key" });
            }

            if (limit <= 0 || limit > 100) limit = 10;

            var popularNovels = await _context.Novels
                .Include(n => n.Author)
                .OrderByDescending(n => n.ViewCount)
                .Take(limit)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    AuthorName = n.Author.Name,
                    Status = n.Status.ToString(),
                    n.ViewCount,
                    ChapterCount = n.Chapters.Count,
                    FavoriteCount = n.Favorites.Count,
                    n.CreatedAt,
                    n.UpdatedAt
                })
                .ToListAsync();

            return Ok(popularNovels);
        }

        /// <summary>
        /// GET: /api/stats/users
        /// Returns a basic list of registered accounts.
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsersStats([FromQuery] int limit = 10)
        {
            if (!IsAuthorized())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Unauthorized: Invalid or missing API Key" });
            }

            if (limit <= 0 || limit > 100) limit = 10;

            // In EF Core + Identity, the Users DbSet might not have custom registration date,
            // but we can retrieve basic info. Since ID is string-based UUID for Identity,
            // we order by UserName or Email or just take a set of them.
            var users = await _context.Users
                .OrderBy(u => u.UserName)
                .Take(limit)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.DisplayName,
                    u.EmailConfirmed
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
