using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tsuki.Data;
using Tsuki.Models;
using Tsuki.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// ─── Load .env file ──────────────────────────────────────────────────────────
// Load từ thư mục project (nơi chứa .env)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ─── Build connection string từ biến môi trường ──────────────────────────────
var dbHost     = Environment.GetEnvironmentVariable("DB_HOST")     ?? "localhost";
var dbPort     = Environment.GetEnvironmentVariable("DB_PORT")     ?? "5432";
var dbName     = Environment.GetEnvironmentVariable("DB_NAME")     ?? "TsukiDb";
var dbUser     = Environment.GetEnvironmentVariable("DB_USER")     ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─── Identity ────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Strict Password Complexity Policy
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // Requires at least one special character

    // Lockout Settings (Brute Force Protection)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Lock for 15 minutes
    options.Lockout.MaxFailedAccessAttempts = 5; // Lock after 5 failed attempts
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ─── Argon2id Password Hasher (overrides default PBKDF2) ─────────────────────
// Must be registered AFTER AddIdentity so it replaces the built-in hasher.
// Existing PBKDF2 hashes are still verified and silently re-hashed on next login.
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2PasswordHasher>();

// Configure cookie paths for Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ─── Rate Limiting (DoS and Bot protection) ──────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-limiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5, // Limit to 5 login/registration requests per minute per IP
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// ─── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<INovelService, NovelService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReadingHistoryService, ReadingHistoryService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IImageService, ImageService>();

// ─── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ─── Seed database ───────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

// ─── Middleware pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Web Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
