using ChatApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public interface IChatService
    {
        Task<IEnumerable<User>> SearchUsersAsync(string query, string currentUserId);
        Task<(bool success, string? error)> AddFriendAsync(string userId, string friendId);
        Task<IEnumerable<User>> GetFriendsAsync(string userId);
        Task SendMessageAsync(string senderId, string receiverId, string content, string? imagePath = null);
        Task<IEnumerable<Message>> GetMessagesAsync(string userId1, string userId2);
        Task MarkMessageAsReadAsync(int messageId);
    }
}