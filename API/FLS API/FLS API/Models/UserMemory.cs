using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("user_memory")]
    public class UserMemory : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;
        
        [Column("importance")]
        public int Importance { get; set; } = 5;
        
        [Column("category")]
        public string Category { get; set; } = "general";
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
