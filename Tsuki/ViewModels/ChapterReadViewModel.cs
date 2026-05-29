namespace Tsuki.ViewModels
{
    public class ChapterReadViewModel
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int WordCount { get; set; }

        // Parent novel info
        public int NovelId { get; set; }
        public string NovelTitle { get; set; } = string.Empty;
        public string? NovelCoverUrl { get; set; }

        // Navigation
        public int? PrevChapterId { get; set; }
        public int? NextChapterId { get; set; }
        public int? PrevChapterNumber { get; set; }
        public int? NextChapterNumber { get; set; }
    }
}
