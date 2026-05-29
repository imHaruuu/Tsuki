using Tsuki.Models;

namespace Tsuki.ViewModels
{
    public class NovelCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public NovelStatus Status { get; set; }
        public int ChapterCount { get; set; }
        public int ViewCount { get; set; }
        public List<string> CategoryNames { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class NovelListViewModel
    {
        public List<NovelCardViewModel> Novels { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? ActiveCategoryId { get; set; }
        public List<Category> Categories { get; set; } = new();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPrevPage => CurrentPage > 1;
    }
}
