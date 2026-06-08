using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;

namespace Tsuki.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _db;

        public BookmarkService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsBookmarkedAsync(string userId, int chapterId)
        {
            return await _db.Bookmarks.AnyAsync(b => b.UserId == userId && b.ChapterId == chapterId);
        }

        public async Task<bool> ToggleBookmarkAsync(string userId, int novelId, int chapterId)
        {
            var existingOnChapter = await _db.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChapterId == chapterId);

            if (existingOnChapter != null)
            {
                _db.Bookmarks.Remove(existingOnChapter);
                await _db.SaveChangesAsync();
                return false; // removed
            }

            // Remove any other bookmark the user has for this novel
            var existingOnNovel = await _db.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.NovelId == novelId);

            if (existingOnNovel != null)
            {
                _db.Bookmarks.Remove(existingOnNovel);
            }

            _db.Bookmarks.Add(new Bookmark
            {
                UserId = userId,
                NovelId = novelId,
                ChapterId = chapterId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true; // added
        }

        public async Task<List<Bookmark>> GetUserBookmarksAsync(string userId)
        {
            return await _db.Bookmarks
                .Include(b => b.Novel)
                .Include(b => b.Chapter)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}
