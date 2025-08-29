using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            return await _context.Users
                .Where(u => u.Email.Contains(query) || u.FirstName.Contains(query) || u.LastName.Contains(query))
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> AreFriendsAsync(string userId, string friendId)
        {
            return await _context.Friendships
                .AnyAsync(f => (f.UserId == userId && f.FriendId == friendId) || (f.UserId == friendId && f.FriendId == userId));
        }

        public async Task AddFriendshipAsync(string userId, string friendId)
        {
            var friendship = new Friendship
            {
                UserId = userId,
                FriendId = friendId
            };
            await _context.Friendships.AddAsync(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetFriendsAsync(string userId)
        {
            return await _context.Friendships
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .Join(_context.Users,
                    f => f.UserId == userId ? f.FriendId : f.UserId,
                    u => u.Id,
                    (f, u) => u)
                .ToListAsync();
        }
    }
}