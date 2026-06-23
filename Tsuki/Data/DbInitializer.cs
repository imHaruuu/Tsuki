using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tsuki.Models;

namespace Tsuki.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var db          = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply any pending migrations automatically
            await db.Database.MigrateAsync();

            // ── Roles ─────────────────────────────────────────────────────────
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // ── Admin User Seeding ────────────────────────────────────────────
            var adminEmail = "admin@tsuki.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ── Authors Seeding ───────────────────────────────────────────────
            if (!await db.Authors.AnyAsync())
            {
                db.Authors.AddRange(
                    new Author { Name = "Lighter", Bio = "A famous light novel author." },
                    new Author { Name = "Tsuki Writer", Bio = "Co-author of Tsuki novels." }
                );
                await db.SaveChangesAsync();
            }

            // ── Categories Seeding ────────────────────────────────────────────
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { Name = "Fantasy", Slug = "fantasy" },
                    new Category { Name = "Romance", Slug = "romance" },
                    new Category { Name = "Sci-Fi", Slug = "sci-fi" },
                    new Category { Name = "Action", Slug = "action" },
                    new Category { Name = "Slice of Life", Slug = "slice-of-life" }
                );
                await db.SaveChangesAsync();
            }

            // ── Novels & Chapters Seeding ────────────────────────────────────
            if (!await db.Novels.AnyAsync())
            {
                var author1 = await db.Authors.FirstOrDefaultAsync(a => a.Name == "Suzu Charo");
                var author2 = await db.Authors.FirstOrDefaultAsync(a => a.Name == "Tsuki Writer");
                
                var catFantasy = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "fantasy");
                var catRomance = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "romance");
                var catSciFi = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "sci-fi");
                var catAction = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "action");
                var catSlice = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "slice-of-life");

                if (author1 != null && author2 != null && catFantasy != null && catRomance != null && catSciFi != null && catAction != null && catSlice != null)
                {
                    var novel1 = new Novel
                    {
                        Title = "Crimson Petals",
                        Description = "A tale of a young swordsman's journey through a world of magic and war.",
                        Status = NovelStatus.Ongoing,
                        AuthorId = author1.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var novel2 = new Novel
                    {
                        Title = "Starlight Academy",
                        Description = "Students gifted with mysterious powers navigate school life and ancient prophecies.",
                        Status = NovelStatus.Ongoing,
                        AuthorId = author2.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var novel3 = new Novel
                    {
                        Title = "The Last Signal",
                        Description = "In a post-apocalyptic world, one engineer fights to restore humanity's communication network.",
                        Status = NovelStatus.Completed,
                        AuthorId = author1.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    db.Novels.AddRange(novel1, novel2, novel3);
                    await db.SaveChangesAsync();

                    // Seed NovelCategories
                    db.NovelCategories.AddRange(
                        new NovelCategory { NovelId = novel1.Id, CategoryId = catFantasy.Id },
                        new NovelCategory { NovelId = novel1.Id, CategoryId = catAction.Id },
                        new NovelCategory { NovelId = novel2.Id, CategoryId = catFantasy.Id },
                        new NovelCategory { NovelId = novel2.Id, CategoryId = catRomance.Id },
                        new NovelCategory { NovelId = novel3.Id, CategoryId = catSciFi.Id }
                    );

                    // Seed Chapters
                    var ch1_1_content = "The morning sky was painted in hues of crimson and gold as Kenji drew his blade. For years, the training grounds of the northern valley had been his sanctuary. Today, however, marked the end of his peace. A dark force was approaching from the eastern borders, and the elders had chosen him to bear the legacy of the Crimson Petals.";
                    var ch1_2_content = "Three days had passed since Kenji left his homeland. The path through the Whispering Woods was treacherous, filled with illusions and beasts. As night fell, he noticed a faint blue glow in the distance. He gripped the hilt of his sword, sensing that this was no ordinary fire, but the start of a confrontation that would test his resolve.";
                    var ch2_1_content = "She stepped off the steam train, her eyes wide with wonder at the towering crystal towers of Starlight Academy. Ailia had spent her entire childhood in the quiet country village of Oakhaven, hiding the spark of light that danced at her fingertips. Now, surrounded by peers who could control elements, gravity, and even time itself, she felt both exhilarated and terrified.";
                    var ch3_1_content = "The last broadcast came at exactly midnight, a crackling signal rising through the static of the dying world. 'Is anyone out there?' a voice whispered before being cut short. From his makeshift radio tower in the ruins of Sector 7, Marcus listened to the silence that followed. He knew the risk, but the message was clear: humanity's frequency was still beating, somewhere.";
                    var ch3_2_content = "Silence had become a familiar companion to Marcus. For two weeks, he had worked tirelessly to repair the main transmission relay on the peak of Mount Ridge. The cold wind bit through his worn leather jacket, but his fingers did not falter as he soldered the final connection. He flipped the switch, and the relay hummed to life, pulsing a beacon into the dark sky.";

                    db.Chapters.AddRange(
                        new Chapter 
                        { 
                            NovelId = novel1.Id, 
                            ChapterNumber = 1, 
                            Title = "The Red Dawn", 
                            Content = ch1_1_content, 
                            WordCount = ch1_1_content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            CreatedAt = DateTime.UtcNow 
                        },
                        new Chapter 
                        { 
                            NovelId = novel1.Id, 
                            ChapterNumber = 2, 
                            Title = "Blade and Blood", 
                            Content = ch1_2_content, 
                            WordCount = ch1_2_content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            CreatedAt = DateTime.UtcNow 
                        },
                        new Chapter 
                        { 
                            NovelId = novel2.Id, 
                            ChapterNumber = 1, 
                            Title = "First Day", 
                            Content = ch2_1_content, 
                            WordCount = ch2_1_content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            CreatedAt = DateTime.UtcNow 
                        },
                        new Chapter 
                        { 
                            NovelId = novel3.Id, 
                            ChapterNumber = 1, 
                            Title = "Static", 
                            Content = ch3_1_content, 
                            WordCount = ch3_1_content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            CreatedAt = DateTime.UtcNow 
                        },
                        new Chapter 
                        { 
                            NovelId = novel3.Id, 
                            ChapterNumber = 2, 
                            Title = "Frequency Lost", 
                            Content = ch3_2_content, 
                            WordCount = ch3_2_content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            CreatedAt = DateTime.UtcNow 
                        }
                    );

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
