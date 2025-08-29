using ChatApp.Models;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public interface IAuthService
    {
        Task<(User user, string error)> RegisterAsync(string email, string password, string firstName, string lastName);
        Task<(string token, string error)> LoginAsync(string email, string password);
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
    }
}