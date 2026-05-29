namespace Tsuki.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }

        // Navigation
        public ICollection<Novel> Novels { get; set; } = new List<Novel>();
    }
}
