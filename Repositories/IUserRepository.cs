using ChatApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> SearchUsersAsync(string query);
        Task<User> GetByIdAsync(string id);
        Task<bool> AreFriendsAsync(string userId, string friendId);
        Task AddFriendshipAsync(string userId, string friendId);
        Task<IEnumerable<User>> GetFriendsAsync(string userId);
    }
}