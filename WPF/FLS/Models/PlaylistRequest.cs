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
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}
