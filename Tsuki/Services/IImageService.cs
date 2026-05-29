using Microsoft.AspNetCore.Http;

namespace Tsuki.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Saves an uploaded image file to a specified folder under wwwroot/images.
        /// </summary>
        /// <param name="file">The uploaded image file.</param>
        /// <param name="subFolder">Subfolder like "covers" or "avatars".</param>
        /// <returns>The relative URL path to the saved image (e.g. "/images/covers/abc.jpg"), or null if no file uploaded/invalid.</returns>
        Task<string?> SaveImageAsync(IFormFile? file, string subFolder);

        /// <summary>
        /// Deletes an existing local image file.
        /// </summary>
        /// <param name="relativeUrl">The relative URL path stored in the database.</param>
        void DeleteImage(string? relativeUrl);
    }
}
