using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("timetable_entries")]
    public class TimetableEntry : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("day")]
        public string Day { get; set; }

        [Column("room")]
        public string Room { get; set; }

        [Column("time_slot")]
        public string TimeSlot { get; set; }

        [Column("course_name")]
        public string CourseName { get; set; }

        [Column("section")]
        public string Section { get; set; }

        [Column("instructor")]
        public string Instructor { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
