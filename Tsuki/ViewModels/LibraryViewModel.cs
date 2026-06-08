using System.Collections.Generic;
using Tsuki.Models;

namespace Tsuki.ViewModels
{
    public class LibraryViewModel
    {
        public List<Favorite> FavoriteNovels { get; set; } = new List<Favorite>();
        public List<Bookmark> BookmarkedChapters { get; set; } = new List<Bookmark>();
        public List<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
    }
}
