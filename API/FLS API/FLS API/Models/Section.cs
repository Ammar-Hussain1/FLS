using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.Models
{
    [Table("sections")]
    public class Section : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("section")]
        public string Name { get; set; } = string.Empty;

        [Column("section_short")]
        public string? SectionShort { get; set; }

        [Column("instructor_name")]
        public string? InstructorName { get; set; }
    }
}
