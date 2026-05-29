using Tsuki.Models;

namespace Tsuki.ViewModels
{
    public class ChapterListItemViewModel
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NovelDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public NovelStatus Status { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public int ViewCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> CategoryNames { get; set; } = new();
        public List<ChapterListItemViewModel> Chapters { get; set; } = new();
        public bool IsFavorited { get; set; }
    }
}
