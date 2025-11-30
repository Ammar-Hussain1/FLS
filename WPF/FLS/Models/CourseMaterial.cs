using System.Text.Json.Serialization;

namespace FLS.Models
{
    public class MaterialResponseDTO
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
    }

    public class UserCourseDTO
    {
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class CourseWithMaterialsDTO
    {
        [JsonPropertyName("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [JsonPropertyName("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [JsonPropertyName("materialsByCategory")]
        public Dictionary<string, List<MaterialResponseDTO>> MaterialsByCategory { get; set; } = new();
    }

    public class CourseMaterial
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public string PreviewLink { get; set; } = string.Empty;
        public MaterialType Type { get; set; }

        public static CourseMaterial FromDTO(MaterialResponseDTO dto)
        {
            return new CourseMaterial
            {
                Id = int.TryParse(dto.Id, out var id) ? id : 0,
                Title = dto.Title,
                Semester = dto.Year?.ToString() ?? "N/A",
                DownloadLink = dto.FilePath ?? string.Empty,
                PreviewLink = dto.FilePath ?? string.Empty,
                Type = MapCategoryToType(dto.Category)
            };
        }

        private static MaterialType MapCategoryToType(string category)
        {
            return category.ToLowerInvariant() switch
            {
                "quizzes" => MaterialType.Quiz,
                "assignments" => MaterialType.Assignment,
                "midterm 1" => MaterialType.Mid1,
                "midterm 2" => MaterialType.Mid2,
                "final" => MaterialType.Final,
                _ => MaterialType.Assignment
            };
        }
    }

    public enum MaterialType
    {
        Quiz,
        Assignment,
        Mid1,
        Mid2,
        Final
    }
}

