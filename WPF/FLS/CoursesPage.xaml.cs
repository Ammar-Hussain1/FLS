using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class CoursesPage : Window
    {
        private ObservableCollection<Course> _courses;
        private int _nextCourseId = 1;

        public CoursesPage()
        {
            InitializeComponent();
            
            _courses = new ObservableCollection<Course>();
            CoursesItemsControl.ItemsSource = _courses;
            LoadCourses();
        }

        private void LoadCourses()
        {
            if (_courses.Count == 0)
            {
                var dummyCourses = new List<Course>
                {
                    new Course
                    {
                        Id = _nextCourseId++.ToString(),
                        Name = "Introduction to Programming",
                        Code = "CS101",
                        Description = "Fundamentals of programming and problem-solving",
                        Credits = 3,
                        CreatedDate = DateTime.Now.AddDays(-30)
                    },
                    new Course
                    {
                        Id = _nextCourseId++.ToString(),
                        Name = "Data Structures and Algorithms",
                        Code = "CS201",
                        Description = "Advanced data structures and algorithm design",
                        Credits = 4,
                        CreatedDate = DateTime.Now.AddDays(-20)
                    },
                    new Course
                    {
                        Id = _nextCourseId++.ToString(),
                        Name = "Database Systems",
                        Code = "CS301",
                        Description = "Design and implementation of database systems",
                        Credits = 3,
                        CreatedDate = DateTime.Now.AddDays(-10)
                    }
                };

                foreach (var course in dummyCourses)
                {
                    _courses.Add(course);
                }
            }

            UpdateEmptyState();
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            AddCourseForm.Visibility = AddCourseForm.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
            
            if (AddCourseForm.Visibility == Visibility.Visible)
            {
                ClearForm();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddCourseForm.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(CourseCodeTextBox.Text))
            {
                MessageBox.Show("Please fill in at least Course Name and Course Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var course = new Course
            {
                Id = _nextCourseId++.ToString(),
                Name = CourseNameTextBox.Text.Trim(),
                Code = CourseCodeTextBox.Text.Trim(),
                Description = CourseDescriptionTextBox.Text.Trim(),
                Credits = int.TryParse(CourseCreditsTextBox.Text, out int credits) ? credits : 0,
                CreatedDate = DateTime.Now
            };

            _courses.Add(course);
            MessageBox.Show("Course added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            AddCourseForm.Visibility = Visibility.Collapsed;
            ClearForm();
            UpdateEmptyState();
        }

        private void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string courseId)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this course?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var courseToDelete = _courses.FirstOrDefault(c => c.Id == courseId);
                    if (courseToDelete != null)
                    {
                        _courses.Remove(courseToDelete);
                        MessageBox.Show("Course deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateEmptyState();
                    }
                }
            }
        }

        private void ClearForm()
        {
            CourseNameTextBox.Clear();
            CourseCodeTextBox.Clear();
            CourseDescriptionTextBox.Clear();
            CourseCreditsTextBox.Clear();
        }

        private void UpdateEmptyState()
        {
            EmptyStateText.Visibility = _courses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

