namespace FLS_API.DL.DTOs
{
    public class SubmitPlaylistRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class ApprovePlaylistDTO
    {
        public Guid AdminId { get; set; }
    }

    public class RejectPlaylistDTO
    {
        public Guid AdminId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CommunityPlaylistDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Likes { get; set; }
    }

    public class PlaylistRequestDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? RejectionReason { get; set; }
    }
}
