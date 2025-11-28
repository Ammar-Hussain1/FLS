using System;

namespace FLS.Models
{
    public class MaterialRequest
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string MaterialType { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public string FilePath { get; set; }

        public MaterialRequest(int id, string courseCode, string materialType, string submittedBy, DateTime submissionDate, string status, string filePath)
        {
            Id = id;
            CourseCode = courseCode;
            MaterialType = materialType;
            SubmittedBy = submittedBy;
            SubmissionDate = submissionDate;
            Status = status;
            FilePath = filePath;
        }
    }
}
