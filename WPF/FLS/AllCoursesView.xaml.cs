using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FLS.Models;
using FLS.DL;

namespace FLS
{
    public partial class AllCoursesView : UserControl
    {
        private ObservableCollection<Course> _courses;
        private ObservableCollection<UserCourse> _savedCourses;
        private int _nextUserCourseId = 1;
        
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private int _totalCount = 0;
        private string _currentSearch = string.Empty;
        
        private readonly ApiClient _apiClient;
        private CancellationTokenSource? _searchCancellationTokenSource;

        public event Action<ObservableCollection<UserCourse>> SavedCoursesChanged;
        public event Action<Models.Course> CourseSelected;

        public AllCoursesView()
        {
            InitializeComponent();
            
            _courses = new ObservableCollection<Course>();
            _savedCourses = new ObservableCollection<UserCourse>();
            _apiClient = new ApiClient();
            
            LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var response = await _apiClient.GetCoursesAsync(_currentPage, _pageSize, _currentSearch);
                
                if (response.Success && response.Data != null)
                {
                    _courses.Clear();
                    
                    foreach (var courseDto in response.Data.Data)
                    {
                        _courses.Add(new Course
                        {
                            Id = courseDto.Id,
                            Name = courseDto.Name,
                            Code = courseDto.Code,
                            Description = courseDto.Description ?? string.Empty,
                            Credits = 0, 
                            CreatedDate = DateTime.Now 
                        });
                    }
                    
                    if (response.Data.Pagination != null)
                    {
                        _totalPages = response.Data.Pagination.TotalPages;
                        _totalCount = response.Data.Pagination.TotalCount;
                        _currentPage = response.Data.Pagination.Page;
                    }
                    
                    UpdatePaginationUI();
                }
                else
                {
                    MessageBox.Show(
                        response.Message ?? "Failed to load courses",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading courses: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddCourseForm.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private async void SaveCourseFormButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(CourseCodeTextBox.Text))
            {
                MessageBox.Show("Please fill in at least Course Name and Course Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var courseDto = new CourseDTO
                {
                    Name = CourseNameTextBox.Text.Trim(),
                    Code = CourseCodeTextBox.Text.Trim(),
                    Description = CourseDescriptionTextBox.Text.Trim()
                };

                MessageBox.Show(
                    "Course creation via API is not yet implemented. Please use the API directly or add the CreateCourse endpoint.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                
                AddCourseForm.Visibility = Visibility.Collapsed;
                ClearForm();
                _currentPage = 1;
                await LoadCoursesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating course: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Course course)
            {
                if (course == null) return;
        
                if (_savedCourses.Any(uc => uc.Course.Id == course.Id))
                {
                    MessageBox.Show("This course is already saved!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var sectionDialog = new SectionInputDialog(course.Name)
                {
                    Owner = Window.GetWindow(this)
                };

                if (sectionDialog.ShowDialog() == true && sectionDialog.IsSaved)
                {
                    var userCourse = new UserCourse
                    {
                        Id = _nextUserCourseId++,
                        Course = course,
                        Section = sectionDialog.Section,
                        EnrolledDate = DateTime.Now
                    };

                    _savedCourses.Add(userCourse);
                    SavedCoursesChanged?.Invoke(_savedCourses);
                    
                    MessageBox.Show(
                        $"Course '{course.Name}' (Section {userCourse.Section}) has been saved!\n\n" +
                        "This information will be used to generate your timetable later.",
                        "Course Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
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
        }

        private void UpdatePaginationUI()
        {
            CoursesItemsControl.ItemsSource = _courses;
            PageInfoText.Text = $"Page {_currentPage} of {_totalPages} (Total: {_totalCount})";
            PrevPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
            
            EmptyStateText.Visibility = _courses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            if (!string.IsNullOrWhiteSpace(_currentSearch) && _courses.Count == 0 && _totalCount > 0)
            {
                EmptyStateText.Text = "No courses found matching your search.";
            }
            else if (_courses.Count == 0)
            {
                EmptyStateText.Text = "No courses available.";
            }
        }
        
        private async void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCoursesAsync();
            }
        }
        
        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadCoursesAsync();
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            var searchText = SearchTextBox?.Text?.Trim() ?? string.Empty;
            _currentSearch = searchText;
            
            ClearSearchButton.Visibility = string.IsNullOrWhiteSpace(searchText) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
            
            _currentPage = 1;
            
            try
            {
                await Task.Delay(500, _searchCancellationTokenSource.Token);
                await LoadCoursesAsync();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            ClearSearchButton.Visibility = Visibility.Collapsed;
            _currentSearch = string.Empty;
            _currentPage = 1;
            await LoadCoursesAsync();
        }

        public ObservableCollection<UserCourse> GetSavedCourses()
        {
            return _savedCourses;
        }

        public ObservableCollection<Course> GetCourses()
        {
            return _courses;
        }

        public void RemoveFromSaved(string courseId)
        {
            var userCourse = _savedCourses.FirstOrDefault(uc => uc.Course.Id == courseId);
            if (userCourse != null)
            {
                _savedCourses.Remove(userCourse);
                SavedCoursesChanged?.Invoke(_savedCourses);
            }
        }
    }
}


