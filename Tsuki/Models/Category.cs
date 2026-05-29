namespace Tsuki.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Navigation
        public ICollection<NovelCategory> NovelCategories { get; set; } = new List<NovelCategory>();
    }
}
