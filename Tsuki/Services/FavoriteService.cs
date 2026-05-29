using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;

namespace Tsuki.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _db;

        public FavoriteService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsFavoritedAsync(string userId, int novelId)
        {
            return await _db.Favorites.AnyAsync(f => f.UserId == userId && f.NovelId == novelId);
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int novelId)
        {
            var existing = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.NovelId == novelId);

            if (existing != null)
            {
                _db.Favorites.Remove(existing);
                await _db.SaveChangesAsync();
                return false; // now not favorited
            }
            else
            {
                _db.Favorites.Add(new Favorite { UserId = userId, NovelId = novelId });
                await _db.SaveChangesAsync();
                return true; // now favorited
            }
        }
    }
}
