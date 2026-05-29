using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;
using Tsuki.ViewModels;

namespace Tsuki.Services
{
    public class NovelService : INovelService
    {
        private readonly ApplicationDbContext _db;

        public NovelService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<NovelListViewModel> GetNovelsAsync(string? searchQuery, int? categoryId, int page, int pageSize)
        {
            var query = _db.Novels
                .Include(n => n.Author)
                .Include(n => n.NovelCategories)
                    .ThenInclude(nc => nc.Category)
                .Include(n => n.Chapters)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(n => n.Title.ToLower().Contains(searchQuery.ToLower()));

            if (categoryId.HasValue)
                query = query.Where(n => n.NovelCategories.Any(nc => nc.CategoryId == categoryId.Value));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var novels = await query
                .OrderByDescending(n => n.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

            return new NovelListViewModel
            {
                Novels = novels.Select(MapToCard).ToList(),
                SearchQuery = searchQuery,
                ActiveCategoryId = categoryId,
                Categories = categories,
                CurrentPage = page,
                TotalPages = totalPages == 0 ? 1 : totalPages,
                PageSize = pageSize
            };
        }

        public async Task<NovelDetailViewModel?> GetNovelDetailAsync(int novelId)
        {
            var novel = await _db.Novels
                .Include(n => n.Author)
                .Include(n => n.NovelCategories)
                    .ThenInclude(nc => nc.Category)
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null) return null;

            return new NovelDetailViewModel
            {
                Id = novel.Id,
                Title = novel.Title,
                Description = novel.Description,
                CoverUrl = novel.CoverUrl,
                Status = novel.Status,
                AuthorName = novel.Author.Name,
                AuthorId = novel.AuthorId,
                ViewCount = novel.ViewCount,
                UpdatedAt = novel.UpdatedAt,
                CategoryNames = novel.NovelCategories.Select(nc => nc.Category.Name).ToList(),
                Chapters = novel.Chapters
                    .OrderBy(c => c.ChapterNumber)
                    .Select(c => new ChapterListItemViewModel
                    {
                        Id = c.Id,
                        ChapterNumber = c.ChapterNumber,
                        Title = c.Title,
                        WordCount = c.WordCount,
                        CreatedAt = c.CreatedAt
                    }).ToList()
            };
        }

        public async Task<List<NovelCardViewModel>> GetFeaturedNovelsAsync(int count = 6)
        {
            var novels = await _db.Novels
                .Include(n => n.Author)
                .Include(n => n.NovelCategories).ThenInclude(nc => nc.Category)
                .Include(n => n.Chapters)
                .OrderByDescending(n => n.ViewCount)
                .Take(count)
                .ToListAsync();

            return novels.Select(MapToCard).ToList();
        }

        public async Task<List<NovelCardViewModel>> GetRecentlyUpdatedNovelsAsync(int count = 8)
        {
            var novels = await _db.Novels
                .Include(n => n.Author)
                .Include(n => n.NovelCategories).ThenInclude(nc => nc.Category)
                .Include(n => n.Chapters)
                .OrderByDescending(n => n.UpdatedAt)
                .Take(count)
                .ToListAsync();

            return novels.Select(MapToCard).ToList();
        }

        private static NovelCardViewModel MapToCard(Novel novel) => new()
        {
            Id = novel.Id,
            Title = novel.Title,
            CoverUrl = novel.CoverUrl,
            AuthorName = novel.Author.Name,
            Status = novel.Status,
            ChapterCount = novel.Chapters.Count,
            ViewCount = novel.ViewCount,
            CategoryNames = novel.NovelCategories.Select(nc => nc.Category.Name).ToList(),
            UpdatedAt = novel.UpdatedAt
        };
    }
}
