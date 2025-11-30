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

        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

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

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("uploadedBy")]
        public string UploadedBy { get; set; } = string.Empty;

        // Computed property for display - tries multiple fields
        public string SubmittedBy => !string.IsNullOrEmpty(UploadedBy) ? UploadedBy : 
                                      !string.IsNullOrEmpty(Username) ? Username : 
                                      !string.IsNullOrEmpty(UserId) ? UserId : "Unknown";

        public string MaterialType => Category;
        public DateTime SubmissionDate => UploadedAt;
    }
}
