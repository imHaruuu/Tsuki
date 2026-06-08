using System;

namespace Tsuki.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int NovelId { get; set; }
        public int ChapterId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Novel Novel { get; set; } = null!;
        public Chapter Chapter { get; set; } = null!;
    }
}
