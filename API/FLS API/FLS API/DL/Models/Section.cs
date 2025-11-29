using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("sections")]
    public class Section : BaseModel
    {
        [PrimaryKey("id", true)]
        public Guid Id { get; set; }

        [Column("course_id")]
        public Guid CourseId { get; set; }

        [Column("section")]
        public string Name { get; set; }

        [Column("instructor_name")]
        public string? InstructorName { get; set; }
    }
}
