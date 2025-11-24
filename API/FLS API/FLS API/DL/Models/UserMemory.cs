using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FLS_API.DL.Models
{
    [Index(nameof(UserId))]
    public class UserMemory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public string MemoryContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
