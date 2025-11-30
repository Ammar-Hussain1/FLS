using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("playlist_requests")]
    public class PlaylistRequest : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Column("playlist_name")]
        public string PlaylistName { get; set; } = string.Empty;
        
        [Required]
        [Column("url")]
        public string Url { get; set; } = string.Empty;
        
        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;
        
        [Column("user_id")]
        public Guid UserId { get; set; }
        
        [Column("status")]
        public string Status { get; set; } = "Pending";
        
        [Column("submitted_date")]
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        
        [Column("reviewed_date")]
        public DateTime? ReviewedDate { get; set; }
        
        [Column("reviewed_by_admin_id")]
        public Guid? ReviewedByAdminId { get; set; }
        
        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }
    }
}
