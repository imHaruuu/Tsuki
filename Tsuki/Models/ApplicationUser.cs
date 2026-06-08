using Microsoft.AspNetCore.Identity;

namespace Tsuki.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        // Navigation
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
