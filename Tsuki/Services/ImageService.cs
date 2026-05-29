using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tsuki.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageService> _logger;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileBytes = 5 * 1024 * 1024; // 5MB

        public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxFileBytes)
                throw new ArgumentException("File size exceeds the 5MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException("Invalid file format. Allowed formats: JPG, JPEG, PNG, WEBP.");

            // Get wwwroot folder path
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            
            var targetFolder = Path.Combine(webRoot, "images", subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(targetFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subFolder}/{uniqueName}";
        }

        public void DeleteImage(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            if (!relativeUrl.StartsWith("/images/")) return; // Only delete local files to protect other data

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // Normalize path by stripping leading slash
            var normalizedPath = relativeUrl.TrimStart('/');
            var fullPath = Path.Combine(webRoot, normalizedPath);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image at path: {Path}", fullPath);
            }
        }
    }
}
