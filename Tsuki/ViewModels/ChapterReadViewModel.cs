namespace Tsuki.ViewModels
{
    public class ChapterReadViewModel
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int WordCount { get; set; }

        /// <summary>
        /// True if Content contains HTML markup; false if it is plain text.
        /// The controller sets this automatically by detecting HTML tags.
        /// </summary>
        public bool IsHtmlContent { get; set; }

        // Parent novel info
        public int NovelId { get; set; }
        public string NovelTitle { get; set; } = string.Empty;
        public string? NovelCoverUrl { get; set; }

        // Navigation
        public int? PrevChapterId { get; set; }
        public int? NextChapterId { get; set; }
        public int? PrevChapterNumber { get; set; }
        public int? NextChapterNumber { get; set; }

        // Bookmark state
        public bool IsBookmarked { get; set; }
    }
}
