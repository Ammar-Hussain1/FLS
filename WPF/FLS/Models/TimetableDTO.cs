using System;

namespace FLS.Models
{
    /// <summary>
    /// Timetable DTO used by the WPF client. We intentionally keep
    /// property names identical to the API's DTO (Id, CourseId, etc.)
    /// so that System.Text.Json can bind them without needing any
    /// JsonPropertyName attributes.
    /// </summary>
    public class TimetableDTO
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? InstructorName { get; set; }
        public string? SectionName { get; set; }
    }
}