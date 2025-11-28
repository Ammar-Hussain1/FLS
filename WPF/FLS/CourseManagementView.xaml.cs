using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class CourseManagementView : UserControl
    {
        private ObservableCollection<CourseItem> _courses;

        public CourseManagementView()
        {
            InitializeComponent();
            _courses = new ObservableCollection<CourseItem>();
            CoursesListView.ItemsSource = _courses;
            LoadDummyCourses();
        }

        private void LoadDummyCourses()
        {
            _courses.Add(new CourseItem(1, "CS101", "Programming Fundamentals", 3, "Dr. Ahmed Khan"));
            _courses.Add(new CourseItem(2, "CS102", "Data Structures", 3, "Dr. Sara Ali"));
            _courses.Add(new CourseItem(3, "MATH201", "Calculus II", 4, "Dr. Hassan"));
            _courses.Add(new CourseItem(4, "PHY101", "Physics I", 3, "Dr. Fatima"));
            _courses.Add(new CourseItem(5, "ENG102", "English Composition", 2, "Ms. Ayesha"));
            _courses.Add(new CourseItem(6, "CS201", "Object Oriented Programming", 3, "Dr. Usman"));
            _courses.Add(new CourseItem(7, "CS301", "Database Systems", 3, "Dr. Zainab"));
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddCourseDialog();
            if (dialog.ShowDialog() == true)
            {
                int newId = _courses.Count > 0 ? _courses.Max(c => c.CourseId) + 1 : 1;
                _courses.Add(new CourseItem(
                    newId,
                    dialog.CourseCode,
                    dialog.CourseName,
                    dialog.CreditHours,
                    dialog.Instructor
                ));

                MessageBox.Show("Course added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int courseId)
            {
                var course = _courses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove {course.CourseCode} - {course.CourseName}?",
                        "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        _courses.Remove(course);
                        MessageBox.Show("Course removed successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }

    public class CourseItem
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int CreditHours { get; set; }
        public string Instructor { get; set; }

        public CourseItem(int id, string code, string name, int credits, string instructor)
        {
            CourseId = id;
            CourseCode = code;
            CourseName = name;
            CreditHours = credits;
            Instructor = instructor;
        }
    }
}
