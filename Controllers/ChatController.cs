using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatApp.Models;
using System.IO;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(new { Message = "Query cannot be empty." });
            }
            var userId = User.Identity?.Name;
            Console.WriteLine($"Search users: Query={query}, UserId={userId}");
            if (userId == null)
            {
                Console.WriteLine("Search users failed: User not authenticated");
                return Unauthorized(new { Message = "User not authenticated." });
            }
            var users = await _chatService.SearchUsersAsync(query, userId);
            Console.WriteLine($"Search result: {users.Count()} users found");
            return Ok(users);
        }

        [HttpPost("add-friend")]
        public async Task<IActionResult> AddFriend([FromBody] AddFriendDto dto)
        {
            if (dto.FriendId == null)
            {
                return BadRequest(new { Message = "FriendId cannot be null." });
            }
            var userId = User.Identity?.Name;
            Console.WriteLine($"Add friend: UserId={userId}, FriendId={dto.FriendId}");
            if (userId == null)
            {
                Console.WriteLine("Add friend failed: User not authenticated");
                return Unauthorized(new { Message = "User not authenticated." });
            }
            var (success, error) = await _chatService.AddFriendAsync(userId, dto.FriendId);
            if (!success)
            {
                Console.WriteLine($"Add friend error: {error}");
                return BadRequest(new { Message = error });
            }
            Console.WriteLine("Friend added successfully");
            return Ok(new { Message = "Friend added successfully." });
        }

        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.Identity?.Name;
            Console.WriteLine($"Get friends: UserId={userId}");
            if (userId == null)
            {
                Console.WriteLine("Get friends failed: User not authenticated");
                return Unauthorized(new { Message = "User not authenticated." });
            }
            var friends = await _chatService.GetFriendsAsync(userId);
            Console.WriteLine($"Get friends result: {friends.Count()} friends found");
            return Ok(friends);
        }

        [HttpGet("messages/{friendId}")]
        public async Task<IActionResult> GetMessages(string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                return BadRequest(new { Message = "FriendId cannot be empty." });
            }
            var userId = User.Identity?.Name;
            Console.WriteLine($"Get messages: UserId={userId}, FriendId={friendId}");
            if (userId == null)
            {
                Console.WriteLine("Get messages failed: User not authenticated");
                return Unauthorized(new { Message = "User not authenticated." });
            }
            var messages = await _chatService.GetMessagesAsync(userId, friendId);
            return Ok(messages);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("UploadImage: No file uploaded or file is empty.");
                return BadRequest(new { Message = "No file uploaded." });
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Console.WriteLine($"Creating uploads folder: {uploadsFolder}");
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/Uploads/{fileName}";
                Console.WriteLine($"Image uploaded successfully: {relativePath}, File exists: {System.IO.File.Exists(filePath)}");
                return Ok(new { Message = "Image uploaded successfully.", ImagePath = relativePath });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadImage error: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to upload image.", Error = ex.Message });
            }
        }
    }

    public class AddFriendDto
    {
        public string? FriendId { get; set; }
    }
}