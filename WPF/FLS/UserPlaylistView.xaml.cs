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
using FLS.Models;
using FLS.Services;

namespace FLS
{
    public partial class UserPlaylistView : UserControl
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5000/api";

        private ObservableCollection<Course> _allCourses;
        private ObservableCollection<Course> _displayedCourses;
        private List<CommunityPlaylist> _allCommunityPlaylists;
        private ObservableCollection<PlaylistRequest> _playlistRequests;

        private int _currentPage = 1;
        private int _pageSize = 5;
        private int _totalPages = 1;
        private string? _selectedCourseId = null;

        public UserPlaylistView()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                await LoadUserCoursesAsync();
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
            // For now, using dummy courses. In production, fetch user's enrolled courses from API
            _allCourses = new ObservableCollection<Course>
            {
                new Course { Id = "1", Name = "Introduction to Programming", Code = "CS101", Credits = 3 },
                new Course { Id = "2", Name = "Data Structures", Code = "CS201", Credits = 4 },
                new Course { Id = "3", Name = "Database Systems", Code = "CS301", Credits = 3 },
                new Course { Id = "4", Name = "Web Development", Code = "CS202", Credits = 3 },
                new Course { Id = "5", Name = "Operating Systems", Code = "CS302", Credits = 4 },
                new Course { Id = "6", Name = "Computer Networks", Code = "CS303", Credits = 3 },
                new Course { Id = "7", Name = "Software Engineering", Code = "CS401", Credits = 4 },
                new Course { Id = "8", Name = "Artificial Intelligence", Code = "CS402", Credits = 3 },
                new Course { Id = "9", Name = "Machine Learning", Code = "CS403", Credits = 4 },
                new Course { Id = "10", Name = "Mobile App Development", Code = "CS304", Credits = 3 },
                new Course { Id = "11", Name = "Cloud Computing", Code = "CS404", Credits = 3 },
                new Course { Id = "12", Name = "Cybersecurity", Code = "CS405", Credits = 4 }
            };

            CourseComboBox.ItemsSource = _allCourses;
        }

        private async Task LoadCommunityPlaylistsAsync()
        {
            try
            {
                var userId = SessionManager.Instance.GetCurrentUserId();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Playlist/community?userId={userId}");

                if (response.IsSuccessStatusCode)
                {
                    _allCommunityPlaylists = await response.Content.ReadFromJsonAsync<List<CommunityPlaylist>>()
                        ?? new List<CommunityPlaylist>();
                }
                else
                {
                    _allCommunityPlaylists = new List<CommunityPlaylist>();
                }
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

                    // Filter to show only current user's requests
                    var userId = SessionManager.Instance.GetCurrentUserId();
                    _playlistRequests = new ObservableCollection<PlaylistRequest>(
                        allRequests.Where(r => r.UserId == userId));
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
            _totalPages = (int)Math.Ceiling((double)_allCourses.Count / _pageSize);

            var skip = (_currentPage - 1) * _pageSize;
            _displayedCourses = new ObservableCollection<Course>(_allCourses.Skip(skip).Take(_pageSize));

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

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CommunityPlaylist playlist)
            {
                playlist.Likes++;
                
                if (_selectedCourseId != null)
                {
                    var coursePlaylists = _allCommunityPlaylists
                        .Where(p => p.CourseId == _selectedCourseId)
                        .ToList();

                    if (response.IsSuccessStatusCode)
                    {
                        playlist.Likes++;

                        // Refresh the display
                        if (_selectedCourseId.HasValue)
                        {
                            var coursePlaylists = _allCommunityPlaylists
                                .Where(p => p.CourseId == _selectedCourseId.Value)
                                .ToList();

                            PlaylistsControl.ItemsSource = null;
                            PlaylistsControl.ItemsSource = coursePlaylists;
                        }

                        MessageBox.Show($"You liked \"{playlist.Name}\"!\nTotal likes: {playlist.Likes}",
                            "Liked", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
                    var requestDto = new
                    {
                        Name = PlaylistNameInput.Text.Trim(),
                        PlaylistName = PlaylistNameInput.Text.Trim(),
                        Url = PlaylistUrlInput.Text.Trim(),
                        CourseId = selectedCourse.Id,
                        UserId = SessionManager.Instance.GetCurrentUserId()
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

                        // Reload requests
                        await LoadPlaylistRequestsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to submit playlist request.", "Error",
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
    }
}
