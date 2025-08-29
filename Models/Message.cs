using System;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = null!;
        public User? Sender { get; set; }
        public string ReceiverId { get; set; } = null!;
        public User? Receiver { get; set; }
        public string? Content { get; set; }
        public string? ImagePath { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}