using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FLS_API.DTOs
{
    public class MaterialRequestDTO
    {
        [Required]
        public string FileLink { get; set; } = string.Empty;

        [Required]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; 
        public int? Year { get; set; }
    }

    public class MaterialResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public int? Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class CourseWithMaterialsDTO
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public Dictionary<string, List<MaterialResponseDTO>> MaterialsByCategory { get; set; } = new();
    }

    public class UserCourseDTO
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class FileUploadDTO
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty;

        public int? Year { get; set; }
    }
}

