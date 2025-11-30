using System;
using System.Text.Json.Serialization;

namespace FLS.Models
{
    public class MaterialRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("uploadedAt")]
        public DateTime UploadedAt { get; set; }

        [JsonPropertyName("uploadedBy")]
        public string? UploadedBy { get; set; }

        [JsonPropertyName("uploadedByName")]
        public string? UploadedByName { get; set; }

        // For now, the API does not return a separate course code for
        // material requests, so we expose CourseName via CourseCode to
        // keep existing XAML bindings working.
        public string CourseCode => CourseName;
        public string MaterialType => Category;
        public DateTime SubmissionDate => UploadedAt;
        public string SubmittedBy => UploadedByName ?? UploadedBy ?? "Unknown";
    }
}
