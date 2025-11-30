using System;
using System.Text.Json.Serialization;

namespace FLS.Models
{
    public class TimetableDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("sectionId")]
        public string SectionId { get; set; } = string.Empty;

        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("room")]
        public string Room { get; set; } = string.Empty;

        [JsonPropertyName("instructorName")]
        public string? InstructorName { get; set; }

        [JsonPropertyName("sectionName")]
        public string? SectionName { get; set; }
    }
}