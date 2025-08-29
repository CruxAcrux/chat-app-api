namespace ChatApp.Models
{
    public class Friendship
    {
        public int Id { get; set; } 
        public string UserId { get; set; } = null!; 
        public string FriendId { get; set; } = null!;
        public User? User { get; set; } 
        public User? Friend { get; set; }
    }
}