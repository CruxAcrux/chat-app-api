using ChatApp.Models;

namespace ChatApp.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(string userId1, string userId2);
        Task AddMessageAsync(Message message);
        Task MarkAsReadAsync(int messageId);
    }
}