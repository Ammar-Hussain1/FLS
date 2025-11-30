using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.Models
{
    [Table("coursematerials")]
    public class CourseMaterial : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("file_path")]
        public string? FilePath { get; set; }

        [Column("year")]
        public int? Year { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("uploaded_by")]
        public string? UploadedBy { get; set; }
    }
}

