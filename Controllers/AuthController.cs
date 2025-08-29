using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Services;
using ChatApp.Models;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authService, UserManager<User> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input." });
            }
            var (user, error) = await _authService.RegisterAsync(dto.Email!, dto.Password!, dto.FirstName!, dto.LastName!);
            if (error != null)
            {
                return BadRequest(new { Message = error });
            }
            return Ok(new { Message = "Registration successful", UserId = user.Id, Email = user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input." });
            }
            var (token, error) = await _authService.LoginAsync(dto.Email!, dto.Password!);
            if (error != null)
            {
                return Unauthorized(new { Message = error });
            }
            return Ok(new { message = "Login successful", token });
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                {
                    return BadRequest(new { Message = "Email is required." });
                }
                await _authService.RequestPasswordResetAsync(dto.Email);
                return Ok(new { Message = "Password reset link sent if email exists." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RequestPasswordReset error: {ex.Message}");
                return BadRequest(new { Message = "Failed to send password reset link.", Error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto.Email!, dto.Token!, dto.NewPassword!);
                return Ok(new { Message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetPassword error: {ex.Message}");
                return BadRequest(new { Message = "Failed to reset password.", Error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return Unauthorized(new { Message = "User not authenticated." });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            return Ok(new { Email = user.Email, FirstName = user.FirstName, LastName = user.LastName });
        }
    }

    public class RegisterDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RequestPasswordResetDto
    {
        public string? Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}