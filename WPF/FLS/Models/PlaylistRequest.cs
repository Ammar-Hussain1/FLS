using System;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("playlistName")]
        public string PlaylistName { get; set; } = string.Empty;
        
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;
        
        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;
        
        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";
        
        [JsonPropertyName("submittedDate")]
        public DateTime SubmittedDate { get; set; }
        
        [JsonPropertyName("submissionDate")]
        public DateTime SubmissionDate { get; set; }
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string SubmittedBy => UserId;
    }
}
