using System;

namespace FLS.Models
{
    public class TimetableSlot
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Section { get; set; }
        public string Room { get; set; }
        public string Instructor { get; set; }

        public TimetableSlot(string day, string time, string courseCode, string courseName, string section, string room, string instructor)
        {
            Day = day;
            Time = time;
            CourseCode = courseCode;
            CourseName = courseName;
            Section = section;
            Room = room;
            Instructor = instructor;
        }
    }
}
