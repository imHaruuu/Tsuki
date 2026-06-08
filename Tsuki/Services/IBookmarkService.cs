using System.Collections.Generic;
using System.Threading.Tasks;
using Tsuki.Models;

namespace Tsuki.Services
{
    public interface IBookmarkService
    {
        Task<bool> IsBookmarkedAsync(string userId, int chapterId);
        Task<bool> ToggleBookmarkAsync(string userId, int novelId, int chapterId);
        Task<List<Bookmark>> GetUserBookmarksAsync(string userId);
    }
}
