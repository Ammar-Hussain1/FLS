using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FLS.Models;
using FLS.BL;
using FLS.DL;
using FLS.Services;

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
        
        private readonly CourseService _courseService;
        private readonly UserCourseService _userCourseService;
        private readonly TimetableService _timetableService;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _searchCancellationTokenSource;

        public event Action<ObservableCollection<UserCourse>> SavedCoursesChanged;
        public event Action<Models.Course> CourseSelected;

        public AllCoursesView()
        {
            InitializeComponent();
            
            _courses = new ObservableCollection<Course>();
            _savedCourses = new ObservableCollection<UserCourse>();
            
            _httpClient = new HttpClient();
            var courseApiClient = new CourseApiClient(_httpClient);
            var userCourseApiClient = new UserCourseApiClient(_httpClient);
            var timetableApiClient = new TimetableApiClient(_httpClient);
            
            _courseService = new CourseService(courseApiClient);
            _userCourseService = new UserCourseService(userCourseApiClient);
            _timetableService = new TimetableService(timetableApiClient);
            
            Loaded += AllCoursesView_Loaded;
        }

        private async void AllCoursesView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AllCoursesView_Loaded;
            await LoadCoursesAsync();
            await LoadSavedCoursesFromApiAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var response = await _courseService.GetCoursesAsync(_currentPage, _pageSize, _currentSearch);
                
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

        private async Task LoadSavedCoursesFromApiAsync()
        {
            _savedCourses.Clear();

            string userId;
            try
            {
                userId = FLS.Services.SessionManager.Instance.GetCurrentUserId();
            }
            catch (InvalidOperationException)
            {
                // Not logged in; nothing to load
                return;
            }

            try
            {
                var response = await _userCourseService.GetMyCoursesAsync(userId);
                if (!response.Success || response.Data == null)
                {
                    return;
                }

                foreach (var dto in response.Data)
                {
                    // Try to find the course in the already loaded list
                    var course = _courses.FirstOrDefault(c => c.Id == dto.CourseId);
                    if (course == null)
                    {
                        // Fallback: create a lightweight course entry
                        course = new Course
                        {
                            Id = dto.CourseId,
                            Code = dto.CourseCode,
                            Name = dto.CourseName,
                            Description = dto.Description ?? string.Empty,
                            Credits = 0,
                            CreatedDate = DateTime.Now
                        };
                        _courses.Add(course);
                    }

                    var userCourse = new UserCourse
                    {
                        Id = _nextUserCourseId++,
                        Course = course,
                        Section = dto.SectionName ?? string.Empty,
                        EnrolledDate = DateTime.Now
                    };

                    _savedCourses.Add(userCourse);
                }

                if (_savedCourses.Any())
                {
                    SavedCoursesChanged?.Invoke(_savedCourses);
                }
            }
            catch
            {
                // If loading saved courses fails, we simply start with an empty list for this session.
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

        private async void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Course course)
            {
                if (course == null) return;
        
                if (_savedCourses.Any(uc => uc.Course.Id == course.Id))
                {
                    MessageBox.Show("This course is already saved!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                List<string> sectionsForCourse = new List<string>();
                try
                {
                    var timetableData = await _timetableService.GetTimetableAsync();
                    if (timetableData != null && timetableData.Any())
                    {
                        // First, try strict match on CourseId
                        var matchingEntries = timetableData
                            .Where(t => t.CourseId == course.Id)
                            .ToList();

                        if (!matchingEntries.Any())
                        {
                            var code = course.Code?.Trim();
                            var name = course.Name?.Trim();

                            matchingEntries = timetableData
                                .Where(t =>
                                    (!string.IsNullOrWhiteSpace(code) &&
                                     !string.IsNullOrWhiteSpace(t.Subject) &&
                                     t.Subject.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrWhiteSpace(name) &&
                                     !string.IsNullOrWhiteSpace(t.Subject) &&
                                     t.Subject.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0))
                                .ToList();
                        }

                        sectionsForCourse = matchingEntries
                            .Select(t => t.SectionName ?? string.Empty)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(s => s)
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Unable to load sections from timetable for this course. You can still type the section manually.\n\nDetails: {ex.Message}",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                var sectionDialog = new SectionInputDialog(course.Name, sectionsForCourse)
                {
                    Owner = Window.GetWindow(this)
                };

                if (sectionDialog.ShowDialog() == true && sectionDialog.IsSaved)
                {
                    // Ensure we have a logged-in user before saving
                    string userId;
                    try
                    {
                        userId = SessionManager.Instance.GetCurrentUserId();
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show(
                            "You must be logged in to save courses.",
                            "Not Logged In",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var apiResult = await _userCourseService.AddUserCourseAsync(userId, course.Id, sectionDialog.Section);
                    if (!apiResult.Success)
                    {
                        MessageBox.Show(
                            apiResult.Message ?? "Failed to save course to your account.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var userCourse = new UserCourse
                    {
                        Id = _nextUserCourseId++,
                        Course = course,
                        Section = sectionDialog.Section,
                        EnrolledDate = DateTime.Now
                    };

                    _savedCourses.Add(userCourse);
                    SavedCoursesChanged?.Invoke(_savedCourses);
                    
                    await LoadSavedCoursesFromApiAsync();
                    
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


