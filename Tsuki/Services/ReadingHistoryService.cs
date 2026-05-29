using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;

namespace Tsuki.Services
{
    public class ReadingHistoryService : IReadingHistoryService
    {
        private readonly ApplicationDbContext _db;

        public ReadingHistoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RecordHistoryAsync(string userId, int novelId, int chapterId)
        {
            // Upsert: update existing entry or create new one
            var existing = await _db.ReadingHistories
                .FirstOrDefaultAsync(r => r.UserId == userId && r.NovelId == novelId);

            if (existing != null)
            {
                existing.ChapterId = chapterId;
                existing.LastReadAt = DateTime.UtcNow;
            }
            else
            {
                _db.ReadingHistories.Add(new ReadingHistory
                {
                    UserId = userId,
                    NovelId = novelId,
                    ChapterId = chapterId,
                    LastReadAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
