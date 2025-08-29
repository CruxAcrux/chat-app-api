using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public List<Friendship>? Friends { get; set; }
        public List<Message>? SentMessages { get; set; }
        public List<Message>? ReceivedMessages { get; set; }
    }
}