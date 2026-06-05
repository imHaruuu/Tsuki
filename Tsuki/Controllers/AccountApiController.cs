using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tsuki.Models;
using System.ComponentModel.DataAnnotations;

namespace Tsuki.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AccountApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountApiController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Registers a new admin account via Postman.
        /// POST: /api/admin/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Invalid data", errors });
            }

            // Verify API Key from .env (if set) to secure the admin creation endpoint
            var expectedApiKey = Environment.GetEnvironmentVariable("ADMIN_API_KEY");
            if (!string.IsNullOrEmpty(expectedApiKey))
            {
                if (request.SecretKey != expectedApiKey)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Unauthorized: Invalid SecretKey" });
                }
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already registered" });
            }

            // Ensure the Admin role exists in the database
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // Assign Admin role
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok(new { message = "Admin account created successfully", email = user.Email });
            }

            var creationErrors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Failed to create admin account", errors = creationErrors });
        }
    }

    public class AdminRegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        public string? SecretKey { get; set; }
    }
}
