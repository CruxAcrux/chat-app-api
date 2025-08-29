using ChatApp.Models;
using ChatApp.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class ChatService : IChatService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;

        public ChatService(IUserRepository userRepository, IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query, string currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(query);
            return users.Where(u => u.Id != currentUserId);
        }

        public async Task<(bool success, string? error)> AddFriendAsync(string userId, string friendId)
        {
            if (userId == friendId)
            {
                return (false, "Cannot add yourself as a friend.");
            }
            if (await _userRepository.AreFriendsAsync(userId, friendId))
            {
                return (false, "Users are already friends.");
            }
            var friend = await _userRepository.GetByIdAsync(friendId);
            if (friend == null)
            {
                return (false, "Friend not found.");
            }
            await _userRepository.AddFriendshipAsync(userId, friendId);
            return (true, null);
        }

        public async Task<IEnumerable<User>> GetFriendsAsync(string userId)
        {
            return await _userRepository.GetFriendsAsync(userId);
        }

        public async Task SendMessageAsync(string senderId, string receiverId, string content, string? imagePath = null)
        {
            if (!await _userRepository.AreFriendsAsync(senderId, receiverId))
            {
                throw new Exception("Users are not friends.");
            }

            if (content?.Length > 500)
            {
                throw new Exception("Message exceeds 500 characters.");
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ImagePath = imagePath,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _messageRepository.AddMessageAsync(message);
        }

        public async Task<IEnumerable<Message>> GetMessagesAsync(string userId1, string userId2)
        {
            return await _messageRepository.GetMessagesBetweenUsersAsync(userId1, userId2);
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            await _messageRepository.MarkAsReadAsync(messageId);
        }
    }
}