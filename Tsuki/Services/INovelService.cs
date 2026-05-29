using Tsuki.ViewModels;

namespace Tsuki.Services
{
    public interface INovelService
    {
        Task<NovelListViewModel> GetNovelsAsync(string? searchQuery, int? categoryId, int page, int pageSize);
        Task<NovelDetailViewModel?> GetNovelDetailAsync(int novelId);
        Task<List<NovelCardViewModel>> GetFeaturedNovelsAsync(int count = 6);
        Task<List<NovelCardViewModel>> GetRecentlyUpdatedNovelsAsync(int count = 8);
    }
}
