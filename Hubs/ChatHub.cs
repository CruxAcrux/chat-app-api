using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Services;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(string receiverId, string content, string? imagePath)
        {
            var senderId = Context.User?.Identity?.Name;
            if (senderId == null)
            {
                throw new HubException("User not authenticated.");
            }

            await _chatService.SendMessageAsync(senderId, receiverId, content, imagePath);
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content, imagePath);
            await Clients.Caller.SendAsync("MessageSent", receiverId, content, imagePath);
        }

        public async Task MarkMessageRead(int messageId)
        {
            await _chatService.MarkMessageAsReadAsync(messageId);
            await Clients.All.SendAsync("MessageRead", messageId);
        }
    }
}