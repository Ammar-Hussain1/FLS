using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("timetables")]
    public class TimeTable : BaseModel
    {
        [PrimaryKey("id", true)]
        public Guid Id { get; set; }

        [Column("course_id")]
        public Guid CourseId { get; set; }

        [Column("section_id")] //uuid just getting if needed in future
        public Guid SectionId { get; set; }

        [Column("section_name")]  // like A, B, C, etc
        public string SectionName { get; set; }

        [Column("day")]
        public string Day { get; set; }

        [Column("time")]
        public string Time { get; set; }

        [Column("subject")]
        public string? Subject { get; set; }  // Course Name actually

        [Column("room")]
        public string? Room { get; set; }

        [Column("instructor_name")]
        public string? InstructorName { get; set; }
    }
}
