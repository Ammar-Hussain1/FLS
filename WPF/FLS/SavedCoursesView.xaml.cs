using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class SavedCoursesView : UserControl
    {
        private ObservableCollection<Course> _savedCourses;
        private Dashboard _parentDashboard;

        public event Action<Models.Course> CourseSelected;

        public SavedCoursesView()
        {
            InitializeComponent();
            _savedCourses = new ObservableCollection<Course>();
            SavedCoursesItemsControl.ItemsSource = _savedCourses;
            LoadSavedCourses();
        }

        private void CourseCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Course course)
            {
                // Don't trigger if clicking on the remove button
                if (e.OriginalSource is Button)
                {
                    return;
                }
                CourseSelected?.Invoke(course);
            }
        }

        public void SetParentDashboard(Dashboard dashboard)
        {
            _parentDashboard = dashboard;
        }

        private void LoadSavedCourses()
        {
            UpdateEmptyState();
        }

        public void SetSavedCourses(ObservableCollection<Course> savedCourses)
        {
            _savedCourses.Clear();
            foreach (var course in savedCourses)
            {
                _savedCourses.Add(course);
            }
            UpdateEmptyState();
        }

        private void RemoveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int courseId)
            {
                var result = MessageBox.Show(
                    "Remove this course from your saved courses?",
                    "Remove Course",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var courseToRemove = _savedCourses.FirstOrDefault(c => c.Id == courseId);
                    if (courseToRemove != null)
                    {
                        _savedCourses.Remove(courseToRemove);
                        
                        // Notify AllCoursesView to update
                        if (_parentDashboard != null)
                        {
                            _parentDashboard.RemoveFromSavedCourses(courseId);
                        }
                        
                        MessageBox.Show("Course removed from saved courses!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateEmptyState();
                    }
                }
            }
        }

        private void UpdateEmptyState()
        {
            EmptyStateText.Visibility = _savedCourses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

