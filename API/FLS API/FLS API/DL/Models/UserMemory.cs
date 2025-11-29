using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FLS_API.DL.Models
{
    [Index(nameof(UserId), nameof(Importance))]
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class UserMemory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        /// <summary>
        /// Free-form memory content (e.g., "User loves playing guitar", "User struggles with calculus")
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// AI-assigned importance score (1-10)
        /// Higher scores = more important to remember
        /// </summary>
        public int Importance { get; set; } = 5;
        
        /// <summary>
        /// Memory category for organization
        /// Examples: "personal", "academic", "preferences", "goals"
        /// </summary>
        public string Category { get; set; } = "general";
        
        /// <summary>
        /// Whether this is a summary of multiple older memories
        /// </summary>
        public bool IsSummary { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastAccessedAt { get; set; }
    }
}
