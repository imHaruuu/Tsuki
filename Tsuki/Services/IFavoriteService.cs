namespace Tsuki.Services
{
    public interface IFavoriteService
    {
        Task<bool> IsFavoritedAsync(string userId, int novelId);
        Task<bool> ToggleFavoriteAsync(string userId, int novelId);
    }
}
