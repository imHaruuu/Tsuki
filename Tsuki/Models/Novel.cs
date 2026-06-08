namespace Tsuki.Models
{
    public enum NovelStatus
    {
        Ongoing,
        Completed,
        Hiatus
    }

    public class Novel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public NovelStatus Status { get; set; } = NovelStatus.Ongoing;
        public int ViewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int AuthorId { get; set; }

        // Navigation
        public Author Author { get; set; } = null!;
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<NovelCategory> NovelCategories { get; set; } = new List<NovelCategory>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
