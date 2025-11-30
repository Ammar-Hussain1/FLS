using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FLS_API.DL.Models
{
    [Table("user_courses")]
    public class UserCourse : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        
        [Column("user_id")]
        public Guid UserId { get; set; }
        
        [Column("course_id")]
        public int CourseId { get; set; }
        
        [Column("section")]
        public string? Section { get; set; }
        
        [Column("enrolled_date")]
        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
    }
}
