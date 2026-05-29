namespace Tsuki.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int NovelId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Novel Novel { get; set; } = null!;
    }
}
