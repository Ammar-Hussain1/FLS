namespace FLS.Models
{
    public class CourseMaterial
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty; // e.g "Fall 2025" "Spring 2025"
        public string DownloadLink { get; set; } = string.Empty;
        public string PreviewLink { get; set; } = string.Empty;
        public MaterialType Type { get; set; }
    }

    public enum MaterialType
    {
        Quiz,
        Assignment,
        Mid1,
        Mid2,
        Final
    }
}

