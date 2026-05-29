namespace Tsuki.Services
{
    public interface IReadingHistoryService
    {
        Task RecordHistoryAsync(string userId, int novelId, int chapterId);
    }
}
