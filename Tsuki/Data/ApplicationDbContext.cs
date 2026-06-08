using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tsuki.Models;

namespace Tsuki.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Novel> Novels { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NovelCategory> NovelCategories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ReadingHistory> ReadingHistories { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite primary key for the join table
            builder.Entity<NovelCategory>()
                .HasKey(nc => new { nc.NovelId, nc.CategoryId });

            builder.Entity<NovelCategory>()
                .HasOne(nc => nc.Novel)
                .WithMany(n => n.NovelCategories)
                .HasForeignKey(nc => nc.NovelId);

            builder.Entity<NovelCategory>()
                .HasOne(nc => nc.Category)
                .WithMany(c => c.NovelCategories)
                .HasForeignKey(nc => nc.CategoryId);

            // Configure NovelStatus enum as string in DB for readability
            builder.Entity<Novel>()
                .Property(n => n.Status)
                .HasConversion<string>();

            // Ensure a user can only favorite a novel once
            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.NovelId })
                .IsUnique();

            // Ensure only one reading history entry per user per novel
            builder.Entity<ReadingHistory>()
                .HasIndex(r => new { r.UserId, r.NovelId })
                .IsUnique();

            // Ensure only one bookmark per user per novel
            builder.Entity<Bookmark>()
                .HasIndex(b => new { b.UserId, b.NovelId })
                .IsUnique();
        }
    }
}
