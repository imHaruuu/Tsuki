using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tsuki.Models;

namespace Tsuki.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply any pending migrations automatically in development
            await db.Database.MigrateAsync();

            // ── Roles ─────────────────────────────────────────────────────────
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // ── Admin user ────────────────────────────────────────────────────
            if (await userManager.FindByEmailAsync("admin@tsuki.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@tsuki.com",
                    Email = "admin@tsuki.com",
                    DisplayName = "Administrator",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── Skip if data already seeded ───────────────────────────────────
            if (await db.Authors.AnyAsync()) return;

            // ── Authors ───────────────────────────────────────────────────────
            var authors = new List<Author>
            {
                new() { Name = "Hajime Isayama", Bio = "Japanese manga artist, known for Attack on Titan." },
                new() { Name = "Reki Kawahara", Bio = "Japanese novelist, creator of Sword Art Online." },
                new() { Name = "Nagaru Tanigawa", Bio = "Japanese novelist, author of The Melancholy of Haruhi Suzumiya." }
            };
            db.Authors.AddRange(authors);
            await db.SaveChangesAsync();

            // ── Categories ────────────────────────────────────────────────────
            var categories = new List<Category>
            {
                new() { Name = "Action", Slug = "action" },
                new() { Name = "Fantasy", Slug = "fantasy" },
                new() { Name = "Romance", Slug = "romance" },
                new() { Name = "Sci-Fi", Slug = "sci-fi" },
                new() { Name = "Mystery", Slug = "mystery" },
                new() { Name = "Slice of Life", Slug = "slice-of-life" }
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            // ── Novels ────────────────────────────────────────────────────────
            var novels = new List<Novel>
            {
                new()
                {
                    Title = "Shadows of the Fallen Kingdom",
                    Description = "In a world where ancient kingdoms have crumbled, a lone warrior embarks on a journey to restore peace and uncover the secrets of a forgotten civilization.",
                    CoverUrl = "/images/covers/cover1.jpg",
                    Status = NovelStatus.Ongoing,
                    AuthorId = authors[0].Id,
                    ViewCount = 15420,
                    CreatedAt = DateTime.UtcNow.AddDays(-90),
                    UpdatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new()
                {
                    Title = "The Digital Frontier",
                    Description = "When the line between virtual reality and the real world blurs, a group of gamers must fight for their lives — and their identities — in a game with no logout button.",
                    CoverUrl = "/images/covers/cover2.jpg",
                    Status = NovelStatus.Ongoing,
                    AuthorId = authors[1].Id,
                    ViewCount = 22100,
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Title = "Echoes of Tomorrow",
                    Description = "A time-traveling scientist discovers that every change in the past creates ripples that threaten to unravel the future. A gripping mystery spanning centuries.",
                    CoverUrl = "/images/covers/cover3.jpg",
                    Status = NovelStatus.Completed,
                    AuthorId = authors[2].Id,
                    ViewCount = 8750,
                    CreatedAt = DateTime.UtcNow.AddDays(-180),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Title = "Crimson Petals",
                    Description = "A forbidden love between a noble lady and a wandering swordsman blossoms amid political conspiracies and ancient curses in feudal Japan.",
                    CoverUrl = "/images/covers/cover4.jpg",
                    Status = NovelStatus.Hiatus,
                    AuthorId = authors[0].Id,
                    ViewCount = 5300,
                    CreatedAt = DateTime.UtcNow.AddDays(-45),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    Title = "Neon Requiem",
                    Description = "In a neon-lit cyberpunk city, a rogue AI and a street hacker form an unlikely alliance to take down a corporation that controls all information.",
                    CoverUrl = "/images/covers/cover5.jpg",
                    Status = NovelStatus.Ongoing,
                    AuthorId = authors[1].Id,
                    ViewCount = 11800,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };
            db.Novels.AddRange(novels);
            await db.SaveChangesAsync();

            // ── Novel-Category relationships ───────────────────────────────────
            var novelCategories = new List<NovelCategory>
            {
                new() { NovelId = novels[0].Id, CategoryId = categories[0].Id }, // Action
                new() { NovelId = novels[0].Id, CategoryId = categories[1].Id }, // Fantasy
                new() { NovelId = novels[1].Id, CategoryId = categories[0].Id }, // Action
                new() { NovelId = novels[1].Id, CategoryId = categories[3].Id }, // Sci-Fi
                new() { NovelId = novels[2].Id, CategoryId = categories[4].Id }, // Mystery
                new() { NovelId = novels[2].Id, CategoryId = categories[3].Id }, // Sci-Fi
                new() { NovelId = novels[3].Id, CategoryId = categories[2].Id }, // Romance
                new() { NovelId = novels[3].Id, CategoryId = categories[0].Id }, // Action
                new() { NovelId = novels[4].Id, CategoryId = categories[3].Id }, // Sci-Fi
                new() { NovelId = novels[4].Id, CategoryId = categories[4].Id }, // Mystery
            };
            db.NovelCategories.AddRange(novelCategories);
            await db.SaveChangesAsync();

            // ── Chapters ──────────────────────────────────────────────────────
            var chapters = new List<Chapter>();

            for (int i = 1; i <= 5; i++)
            {
                chapters.Add(new Chapter
                {
                    NovelId = novels[0].Id,
                    ChapterNumber = i,
                    Title = $"Chapter {i}: {GetChapterTitle(0, i)}",
                    Content = GenerateSampleContent(i),
                    WordCount = 1200 + (i * 100),
                    CreatedAt = DateTime.UtcNow.AddDays(-90 + (i * 5))
                });
            }

            for (int i = 1; i <= 4; i++)
            {
                chapters.Add(new Chapter
                {
                    NovelId = novels[1].Id,
                    ChapterNumber = i,
                    Title = $"Chapter {i}: {GetChapterTitle(1, i)}",
                    Content = GenerateSampleContent(i),
                    WordCount = 1500 + (i * 80),
                    CreatedAt = DateTime.UtcNow.AddDays(-60 + (i * 7))
                });
            }

            for (int i = 1; i <= 3; i++)
            {
                chapters.Add(new Chapter
                {
                    NovelId = novels[2].Id,
                    ChapterNumber = i,
                    Title = $"Chapter {i}: {GetChapterTitle(2, i)}",
                    Content = GenerateSampleContent(i),
                    WordCount = 2000 + (i * 50),
                    CreatedAt = DateTime.UtcNow.AddDays(-180 + (i * 10))
                });
            }

            db.Chapters.AddRange(chapters);
            await db.SaveChangesAsync();
        }

        private static string GetChapterTitle(int novelIndex, int chapterNum)
        {
            string[][] titles = [
                ["The Last Ember", "Ruins of the Old World", "A Stranger's Blade", "The Hidden Passage", "Blood and Ash"],
                ["Welcome to NeoVerse", "First Login", "The Dungeon Below", "Betrayal Protocol"],
                ["A Clock That Ticks Backwards", "The First Thread", "Convergence Point"]
            ];
            return novelIndex < titles.Length && chapterNum <= titles[novelIndex].Length
                ? titles[novelIndex][chapterNum - 1]
                : $"The Journey Continues";
        }

        private static string GenerateSampleContent(int chapterNum)
        {
            return $@"<p>The air was thick with anticipation as our protagonist stepped forward into the unknown. Chapter {chapterNum} marked a turning point — a moment where every decision would echo through the pages of history.</p>

<p>The landscape stretched endlessly before them, painted in hues of amber and violet as the sun dipped below the jagged horizon. Somewhere in the distance, a bell tolled — once, twice, three times — its sound swallowed by the wind.</p>

<p>""We have no choice,"" a voice said from behind. The words were calm, measured, carrying the weight of someone who had seen too much to be surprised by anything anymore. ""The path forward is the only path we have.""</p>

<p>They pressed on. The shadows grew longer. The silence grew heavier. And yet, with every step, something deep within stirred — an ember that refused to be extinguished, a spark that knew its time had not yet come to burn.</p>

<p>It was only when they reached the crossroads that the full weight of their journey settled upon their shoulders. Four paths stretched outward like the arms of a compass, each promising something different — safety, glory, truth, or ruin.</p>

<p>The choice, as always, would define everything that followed.</p>

<p>Hours passed. The moon climbed high and the stars wheeled overhead. By the time they made camp at the edge of the treeline, exhaustion had settled into their bones like old snow — cold, persistent, stubborn. But sleep, when it came, was mercifully dreamless.</p>

<p>Tomorrow, the real journey would begin.</p>";
        }
    }
}
