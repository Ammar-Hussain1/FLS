using System;

namespace FLS.Models
{
    public class CommunityPlaylist
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int CourseId { get; set; }
    }
}
