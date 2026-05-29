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

            // ── Grant Admin to specific accounts ──────────────────────────────
            var adminEmails = new[]
            {
                "admin@tsuki.local"
            };

            foreach (var email in adminEmails)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
