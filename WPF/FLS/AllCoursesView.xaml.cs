using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FLS.Models;

namespace FLS
{
    public partial class AllCoursesView : UserControl
    {
        private ObservableCollection<Course> _courses;
        private ObservableCollection<Course> _savedCourses;
        private CollectionViewSource _coursesViewSource;
        private int _nextCourseId = 1;

        public event Action<ObservableCollection<Course>> SavedCoursesChanged;
        public event Action<Models.Course> CourseSelected;

        public AllCoursesView()
        {
            InitializeComponent();
            
            _courses = new ObservableCollection<Course>();
            _savedCourses = new ObservableCollection<Course>();
            
            // Set up CollectionViewSource for filtering
            _coursesViewSource = new CollectionViewSource();
            _coursesViewSource.Source = _courses;
            _coursesViewSource.Filter += CoursesViewSource_Filter;
            
            CoursesItemsControl.ItemsSource = _coursesViewSource.View;
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
                        Id = _nextCourseId++,
                        Name = "Introduction to Programming",
                        Code = "CS101",
                        Description = "Fundamentals of programming and problem-solving",
                        Credits = 3,
                        Instructor = "Dr. John Smith",
                        CreatedDate = DateTime.Now.AddDays(-30)
                    },
                    new Course
                    {
                        Id = _nextCourseId++,
                        Name = "Data Structures and Algorithms",
                        Code = "CS201",
                        Description = "Advanced data structures and algorithm design",
                        Credits = 4,
                        Instructor = "Dr. Jane Doe",
                        CreatedDate = DateTime.Now.AddDays(-20)
                    },
                    new Course
                    {
                        Id = _nextCourseId++,
                        Name = "Database Systems",
                        Code = "CS301",
                        Description = "Design and implementation of database systems",
                        Credits = 3,
                        Instructor = "Dr. Robert Johnson",
                        CreatedDate = DateTime.Now.AddDays(-10)
                    },
                    new Course
                    {
                        Id = _nextCourseId++,
                        Name = "Web Development",
                        Code = "CS401",
                        Description = "Modern web development with HTML, CSS, and JavaScript",
                        Credits = 3,
                        Instructor = "Dr. Sarah Williams",
                        CreatedDate = DateTime.Now.AddDays(-5)
                    },
                    new Course
                    {
                        Id = _nextCourseId++,
                        Name = "Machine Learning",
                        Code = "CS501",
                        Description = "Introduction to machine learning algorithms and applications",
                        Credits = 4,
                        Instructor = "Dr. Michael Brown",
                        CreatedDate = DateTime.Now.AddDays(-2)
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

        private void SaveCourseFormButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(CourseCodeTextBox.Text))
            {
                MessageBox.Show("Please fill in at least Course Name and Course Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var course = new Course
            {
                Id = _nextCourseId++,
                Name = CourseNameTextBox.Text.Trim(),
                Code = CourseCodeTextBox.Text.Trim(),
                Description = CourseDescriptionTextBox.Text.Trim(),
                Credits = int.TryParse(CourseCreditsTextBox.Text, out int credits) ? credits : 0,
                Instructor = CourseInstructorTextBox.Text.Trim(),
                CreatedDate = DateTime.Now
            };

            _courses.Add(course);
            MessageBox.Show("Course added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            AddCourseForm.Visibility = Visibility.Collapsed;
            ClearForm();
            _coursesViewSource.View.Refresh();
            UpdateEmptyState();
        }

        private void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var course = _courses.FirstOrDefault(c => c.Id == courseId);
                if (course != null && !_savedCourses.Any(c => c.Id == courseId))
                {
                    _savedCourses.Add(course);
                    SavedCoursesChanged?.Invoke(_savedCourses);
                    MessageBox.Show($"Course '{course.Name}' saved to your saved courses!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (_savedCourses.Any(c => c.Id == courseId))
                {
                    MessageBox.Show("This course is already saved!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CourseCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Course course)
            {
                if (e.OriginalSource is Button)
                {
                    return;
                }
                CourseSelected?.Invoke(course);
            }
        }

        private void ClearForm()
        {
            CourseNameTextBox.Clear();
            CourseCodeTextBox.Clear();
            CourseDescriptionTextBox.Clear();
            CourseCreditsTextBox.Clear();
            CourseInstructorTextBox.Clear();
        }

        private void UpdateEmptyState()
        {
            int filteredCount = 0;
            if (_coursesViewSource?.View != null)
            {
                foreach (var item in _coursesViewSource.View)
                {
                    filteredCount++;
                }
            }
            
            EmptyStateText.Visibility = filteredCount == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrWhiteSpace(SearchTextBox?.Text) && filteredCount == 0 && _courses.Count > 0)
            {
                EmptyStateText.Text = "No courses found matching your search.";
            }
            else if (_courses.Count == 0)
            {
                EmptyStateText.Text = "No courses available. Click 'Add Course' to get started!";
            }
            else
            {
                EmptyStateText.Text = "No courses available. Click 'Add Course' to get started!";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_coursesViewSource != null)
            {
                _coursesViewSource.View.Refresh();
                UpdateEmptyState();
            }
            
            ClearSearchButton.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            ClearSearchButton.Visibility = Visibility.Collapsed;
        }

        private void CoursesViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Course course)
            {
                string searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = course.Name.ToLower().Contains(searchText) ||
                                 course.Code.ToLower().Contains(searchText) ||
                                 course.Description.ToLower().Contains(searchText) ||
                                 course.Instructor.ToLower().Contains(searchText);
                }
            }
            else
            {
                e.Accepted = false;
            }
        }

        public ObservableCollection<Course> GetSavedCourses()
        {
            return _savedCourses;
        }

        public void RemoveFromSaved(int courseId)
        {
            var course = _savedCourses.FirstOrDefault(c => c.Id == courseId);
            if (course != null)
            {
                _savedCourses.Remove(course);
                SavedCoursesChanged?.Invoke(_savedCourses);
            }
        }
    }
}

