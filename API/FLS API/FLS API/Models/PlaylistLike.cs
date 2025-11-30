using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("playlist_likes")]
    public class PlaylistLike : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("playlist_id")]
        public string PlaylistId { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("liked_at")]
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}

