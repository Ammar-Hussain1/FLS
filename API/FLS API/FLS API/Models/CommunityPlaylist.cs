using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("community_playlists")]
    public class CommunityPlaylist : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Column("url")]
        public string Url { get; set; } = string.Empty;
        
        [Column("course_id")]
        public int CourseId { get; set; }
        
        [Column("likes")]
        public int Likes { get; set; } = 0;
        
        [Column("submitted_by_user_id")]
        public Guid? SubmittedByUserId { get; set; }
        
        [Column("approved_by_admin_id")]
        public Guid? ApprovedByAdminId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }
    }
}
