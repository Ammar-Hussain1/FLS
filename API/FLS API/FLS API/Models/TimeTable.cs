using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("timetables")]
    public class TimeTable : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("section_id")]
        public string SectionId { get; set; } = string.Empty;

        [Column("day")]
        public string Day { get; set; } = string.Empty;

        [Column("time")]
        public string Time { get; set; } = string.Empty;

        [Column("subject")]
        public string Subject { get; set; } = string.Empty;

        [Column("room")]
        public string Room { get; set; } = string.Empty;
    }
}

