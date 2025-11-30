using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.Models
{
    [Table("usercourses")]
    public class UserCourseModel : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("section_id")]
        public string SectionId { get; set; } = string.Empty;

        [Column("enrolled_at")]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}

