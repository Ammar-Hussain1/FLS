using System;

namespace FLS.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class PlaylistRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public Guid UserId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedDate { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
    }
}
