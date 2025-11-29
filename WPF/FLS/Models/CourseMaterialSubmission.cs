using System;

namespace FLS.Models
{
    public class CourseMaterialSubmission
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public MaterialType MaterialType { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
        public string SubmittedBy { get; set; } = string.Empty;
    }
}

