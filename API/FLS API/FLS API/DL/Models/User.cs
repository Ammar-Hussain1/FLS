using System.ComponentModel.DataAnnotations;

namespace FLS_API.DL.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" or "User"
        public string PasswordHash { get; set; } = string.Empty;
        
        public List<UserMemory> Memories { get; set; } = new();
        public List<ChatLog> ChatLogs { get; set; } = new();
    }
}
