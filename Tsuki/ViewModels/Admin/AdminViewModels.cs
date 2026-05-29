using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Tsuki.Models;

namespace Tsuki.ViewModels.Admin
{
    public class CreateNovelViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Cover Image URL")]
        public string? CoverUrl { get; set; }

        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImage { get; set; }

        [Required]
        [Display(Name = "Status")]
        public NovelStatus Status { get; set; } = NovelStatus.Ongoing;

        [Required]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }

        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        // Chapters list for editing context
        public List<Chapter> Chapters { get; set; } = new();

        // For dropdowns in the form
        public List<Author> AvailableAuthors { get; set; } = new();
        public List<Category> AvailableCategories { get; set; } = new();
    }

    public class CreateChapterViewModel
    {
        [Required]
        [Display(Name = "Novel")]
        public int NovelId { get; set; }

        [Required]
        [Display(Name = "Chapter Number")]
        [Range(1, 9999)]
        public int ChapterNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Chapter Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        // For dropdown in form
        public List<Novel> AvailableNovels { get; set; } = new();
    }

    public class CreateAuthorViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Author Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Biography")]
        public string? Bio { get; set; }

        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Upload Avatar Image")]
        public IFormFile? AvatarImage { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Slug (URL-friendly)")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must be lowercase letters, numbers, and hyphens only.")]
        public string Slug { get; set; } = string.Empty;
    }
}
