using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Models;
using ChatApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, AppDbContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
        }

        public async Task<(User user, string? error)> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return (null!, "All fields are required.");
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (null!, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return (user, null);
        }

        public async Task<(string? token, string? error)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return (null, "Email and password are required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return (null, "Invalid login attempt.");
            }

            var token = GenerateJwtToken(user);
            return (token, null);
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("RequestPasswordResetAsync: Email is empty.");
                return;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine($"RequestPasswordResetAsync: No user found for email {email}");
                return;
            }

            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(1)
            };

            await _context.PasswordResetTokens.AddAsync(resetToken);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Password reset token created: UserId={user.Id}, Token={token}, Expiry={resetToken.ExpiryDate}");

            var resetLink = $"http://localhost:3000/reset-password?email={email}&token={token}";
            try
            {
                await _emailService.SendEmailAsync(email, "Password Reset Request", $"Click here to reset your password: {resetLink}");
                Console.WriteLine($"Password reset email sent to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RequestPasswordResetAsync: Failed to send email to {email}: {ex.Message}");
                throw;
            }
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("ResetPasswordAsync: Missing required fields.");
                throw new Exception("All fields are required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine($"ResetPasswordAsync: No user found for email {email}");
                throw new Exception("Invalid email.");
            }

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == token && t.ExpiryDate > DateTime.UtcNow);
            if (resetToken == null)
            {
                Console.WriteLine($"ResetPasswordAsync: Invalid or expired token for UserId={user.Id}, Token={token}");
                throw new Exception("Invalid or expired token.");
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
            {
                Console.WriteLine($"ResetPasswordAsync: Failed to remove password for UserId={user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new Exception("Failed to reset password.");
            }

            result = await _userManager.AddPasswordAsync(user, newPassword);
            if (!result.Succeeded)
            {
                Console.WriteLine($"ResetPasswordAsync: Failed to set new password for UserId={user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Password reset successful for UserId={user.Id}");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? throw new ArgumentNullException(nameof(user.Id))),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? throw new ArgumentNullException(nameof(user.Email))),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your_default_secret_key_32_chars_long!!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ChatApp",
                audience: _configuration["Jwt:Audience"] ?? "ChatAppUsers",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}