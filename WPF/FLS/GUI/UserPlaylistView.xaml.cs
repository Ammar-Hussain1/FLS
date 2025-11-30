using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FLS.BL;
using FLS.DL;
using FLS.Models;
using FLS.Services;

namespace FLS
{
    public partial class UserPlaylistView : UserControl
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5232/api";
        private readonly CourseService _courseService;
        private readonly PlaylistService _playlistService;
        private readonly UserCourseService _userCourseService;

        private ObservableCollection<Course> _allCourses;
        private ObservableCollection<Course> _displayedCourses;
        private List<CommunityPlaylist> _allCommunityPlaylists;
        private ObservableCollection<PlaylistRequest> _playlistRequests;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private int _totalPages = 1;
        private string? _selectedCourseId = null;
        private HashSet<string> _userCourseCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public UserPlaylistView()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            
            var courseApiClient = new CourseApiClient(_httpClient);
            var playlistApiClient = new PlaylistApiClient(_httpClient);
            var userCourseApiClient = new UserCourseApiClient(_httpClient);
            
            _courseService = new CourseService(courseApiClient);
            _playlistService = new PlaylistService(playlistApiClient);
            _userCourseService = new UserCourseService(userCourseApiClient);
            
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                await LoadUserCoursesAsync();
                await LoadUserSavedCourseCodesAsync();
                await LoadCommunityPlaylistsAsync();
                await LoadPlaylistRequestsAsync();
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUserCoursesAsync()
        {
            _allCourses = new ObservableCollection<Course>();

            try
            {
                // Load all courses from the API so that the user can
                // request playlists for any course in the system.
                int currentPage = 1;
                int pageSize = 100;
                bool hasMorePages = true;

                while (hasMorePages)
                {
                    var response = await _courseService.GetCoursesAsync(currentPage, pageSize);
                    if (response.Success && response.Data != null)
                    {
                        foreach (var courseDto in response.Data.Data)
                        {
                            _allCourses.Add(new Course
                            {
                                Id = courseDto.Id,
                                Code = courseDto.Code,
                                Name = courseDto.Name,
                                Description = courseDto.Description ?? string.Empty,
                                Credits = 0,
                                CreatedDate = DateTime.Now
                            });
                        }

                        hasMorePages = response.Data.Pagination?.HasNextPage ?? false;
                        currentPage++;
                    }
                    else
                    {
                        hasMorePages = false;
                    }
                }
            }
            catch
            {
                // If course loading fails, fall back to an empty list;
                // the rest of the view will still function but without
                // a course dropdown for new requests.
                _allCourses.Clear();
            }

            // Sort courses alphabetically by name for a nicer dropdown
            var sortedCourses = _allCourses.OrderBy(c => c.Name).ToList();
            _allCourses = new ObservableCollection<Course>(sortedCourses);

            CourseComboBox.ItemsSource = _allCourses;
        }

        private async Task LoadUserSavedCourseCodesAsync()
        {
            _userCourseCodes.Clear();

            try
            {
                var userId = SessionManager.Instance.GetCurrentUserId();
                var response = await _userCourseService.GetMyCoursesAsync(userId);

                if (response.Success && response.Data != null)
                {
                    foreach (var userCourse in response.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(userCourse.CourseCode))
                        {
                            _userCourseCodes.Add(userCourse.CourseCode.Trim());
                        }
                    }
                }
            }
            catch
            {
                // If this fails, we simply won't filter by saved courses,
                // but the rest of the playlist view will still function.
            }
        }

        private async Task LoadCommunityPlaylistsAsync()
        {
            try
            {
                var userId = SessionManager.Instance.GetCurrentUserId();
                _allCommunityPlaylists = await _playlistService.GetCommunityPlaylistsAsync(userId);
            }
            catch
            {
                _allCommunityPlaylists = new List<CommunityPlaylist>();
            }
        }

        private async Task LoadPlaylistRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Playlist/requests");

                if (response.IsSuccessStatusCode)
                {
                    var allRequests = await response.Content.ReadFromJsonAsync<List<PlaylistRequest>>()
                        ?? new List<PlaylistRequest>();

                    var userId = SessionManager.Instance.GetCurrentUserId();
                    _playlistRequests = new ObservableCollection<PlaylistRequest>(
                        allRequests.Where(r => r.UserId == userId || r.SubmittedBy == userId));
                }
                else
                {
                    _playlistRequests = new ObservableCollection<PlaylistRequest>();
                }
            }
            catch
            {
                _playlistRequests = new ObservableCollection<PlaylistRequest>();
            }

            RequestsDataGrid.ItemsSource = _playlistRequests;
        }

        private void UpdatePagination()
        {
            // Only show playlist courses that correspond to the user's saved/enrolled courses
            var filteredCourses = _userCourseCodes != null && _userCourseCodes.Any()
                ? _allCourses.Where(c => !string.IsNullOrWhiteSpace(c.Code) && _userCourseCodes.Contains(c.Code.Trim()))
                : Enumerable.Empty<Course>();

            var filteredList = filteredCourses.ToList();

            _totalPages = filteredList.Count > 0
                ? (int)Math.Ceiling((double)filteredList.Count / _pageSize)
                : 1;

            var skip = (_currentPage - 1) * _pageSize;
            _displayedCourses = new ObservableCollection<Course>(filteredList.Skip(skip).Take(_pageSize));

            CourseListControl.ItemsSource = _displayedCourses;
            PageInfoText.Text = $"Page {_currentPage} of {_totalPages}";

            PrevPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private void CourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string courseId)
            {
                _selectedCourseId = courseId;
                var course = _allCourses.FirstOrDefault(c => c.Id == courseId);

                if (course != null)
                {
                    SelectedCourseText.Text = $"Community Playlists for {course.Name}";

                    // Clear search when switching courses
                    SearchTextBox.Text = string.Empty;

                    var coursePlaylists = _allCommunityPlaylists
                        .Where(p => p.CourseId == courseId)
                        .ToList();

                    PlaylistsControl.ItemsSource = coursePlaylists;
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedCourseId == null)
                return;

            var searchQuery = SearchTextBox.Text.Trim().ToLower();
            
            var coursePlaylists = _allCommunityPlaylists
                .Where(p => p.CourseId == _selectedCourseId)
                .ToList();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                coursePlaylists = coursePlaylists
                    .Where(p => p.Name.ToLower().Contains(searchQuery) || 
                               p.Url.ToLower().Contains(searchQuery))
                    .ToList();
            }

            PlaylistsControl.ItemsSource = coursePlaylists;
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CommunityPlaylist playlist)
            {
                try
                {
                    // Call backend to increment like count and record user like
                    var userId = SessionManager.Instance.GetCurrentUserId();
                    var response = await _httpClient.PostAsync(
                        $"{_apiBaseUrl}/Playlist/like/{playlist.Id}?userId={Uri.EscapeDataString(userId)}", null);

                    if (!response.IsSuccessStatusCode)
                    {
                        if ((int)response.StatusCode == 409)
                        {
                            MessageBox.Show("You have already liked this playlist.",
                                "Already Liked", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to like playlist. Please try again.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }

                    // Update local model
                    playlist.Likes++;

                    // Refresh the display for the currently selected course
                    if (_selectedCourseId != null)
                    {
                        var coursePlaylists = _allCommunityPlaylists
                            .Where(p => p.CourseId == _selectedCourseId)
                            .OrderByDescending(p => p.Likes)
                            .ToList();

                        PlaylistsControl.ItemsSource = null;
                        PlaylistsControl.ItemsSource = coursePlaylists;
                    }

                    MessageBox.Show($"You liked \"{playlist.Name}\"!\nTotal likes: {playlist.Likes}",
                        "Liked", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error liking playlist: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedItem is Course selectedCourse &&
                !string.IsNullOrWhiteSpace(PlaylistNameInput.Text) &&
                !string.IsNullOrWhiteSpace(PlaylistUrlInput.Text))
            {
                try
                {
                    var userId = SessionManager.Instance.GetCurrentUserId();
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        MessageBox.Show("User ID is required.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var requestDto = new
                    {
                        Name = PlaylistNameInput.Text.Trim(),
                        PlaylistName = PlaylistNameInput.Text.Trim(),
                        Url = PlaylistUrlInput.Text.Trim(),
                        CourseId = selectedCourse.Id,
                        UserId = userId
                    };

                    var json = JsonSerializer.Serialize(requestDto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Playlist/request", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Playlist request submitted successfully for {selectedCourse.Name}!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Clear form
                        PlaylistNameInput.Text = string.Empty;
                        PlaylistUrlInput.Text = string.Empty;
                        CourseComboBox.SelectedIndex = -1;

                        // Reload requests and refresh data
                        await LoadPlaylistRequestsAsync();
                        await RefreshUserCourses();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Failed to submit playlist request: {errorContent}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error submitting request: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please fill in all fields and select a course.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async void RefreshData()
        {
            try
            {
                await LoadUserSavedCourseCodesAsync();
                await LoadCommunityPlaylistsAsync();
                await LoadPlaylistRequestsAsync();
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshUserCourses()
        {
            await LoadUserSavedCourseCodesAsync();
            UpdatePagination();
        }
    }
}
