namespace Tsuki.Models
{
    // Many-to-many join table between Novel and Category
    public class NovelCategory
    {
        public int NovelId { get; set; }
        public Novel Novel { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
