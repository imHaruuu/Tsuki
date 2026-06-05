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
                    DisplayName = "Admin User",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Password123!");
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
                    new Author { Name = "Suzu Charo", Bio = "A famous light novel author." },
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
        }
    }
}
