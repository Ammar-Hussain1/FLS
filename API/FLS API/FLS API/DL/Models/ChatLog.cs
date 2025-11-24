using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FLS_API.DL.Models
{
    [Index(nameof(UserId))] // Index for fast retrieval
    public class ChatLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public string Role { get; set; } = string.Empty; // "user" or "bot"
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
