using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{

    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; }
        
        [Column("full_name")]
        public string FullName { get; set; }
        
        [Column("email")]
        public string Email { get; set; }
        
        [Column("role")]
        public string Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
