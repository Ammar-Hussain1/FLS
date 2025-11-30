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
        private Guid? _selectedCourseId = null;

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
                new Course { Id = Guid.NewGuid(), Name = "Introduction to Programming", Code = "CS101", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Data Structures", Code = "CS201", Credits = 4 },
                new Course { Id = Guid.NewGuid(), Name = "Database Systems", Code = "CS301", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Web Development", Code = "CS202", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Operating Systems", Code = "CS302", Credits = 4 },
                new Course { Id = Guid.NewGuid(), Name = "Computer Networks", Code = "CS303", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Software Engineering", Code = "CS401", Credits = 4 },
                new Course { Id = Guid.NewGuid(), Name = "Artificial Intelligence", Code = "CS402", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Machine Learning", Code = "CS403", Credits = 4 },
                new Course { Id = Guid.NewGuid(), Name = "Mobile App Development", Code = "CS304", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Cloud Computing", Code = "CS404", Credits = 3 },
                new Course { Id = Guid.NewGuid(), Name = "Cybersecurity", Code = "CS405", Credits = 4 }
            };

            _allCommunityPlaylists = new List<CommunityPlaylist>
            {
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Python Programming for Beginners", Url = "https://www.youtube.com/playlist?list=PL1", Likes = 245, CourseId = _allCourses.First(c => c.Name == "Introduction to Programming").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "C++ Fundamentals", Url = "https://www.youtube.com/playlist?list=PL2", Likes = 189, CourseId = _allCourses.First(c => c.Name == "Introduction to Programming").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Java Complete Course", Url = "https://www.youtube.com/playlist?list=PL3", Likes = 312, CourseId = _allCourses.First(c => c.Name == "Introduction to Programming").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Data Structures Masterclass", Url = "https://www.youtube.com/playlist?list=PL4", Likes = 428, CourseId = _allCourses.First(c => c.Name == "Data Structures").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Algorithms Explained", Url = "https://www.youtube.com/playlist?list=PL5", Likes = 356, CourseId = _allCourses.First(c => c.Name == "Data Structures").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Trees and Graphs", Url = "https://www.youtube.com/playlist?list=PL6", Likes = 267, CourseId = _allCourses.First(c => c.Name == "Data Structures").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "SQL Tutorial", Url = "https://www.youtube.com/playlist?list=PL7", Likes = 523, CourseId = _allCourses.First(c => c.Name == "Database Systems").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Database Design Principles", Url = "https://www.youtube.com/playlist?list=PL8", Likes = 198, CourseId = _allCourses.First(c => c.Name == "Database Systems").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "NoSQL Databases", Url = "https://www.youtube.com/playlist?list=PL9", Likes = 145, CourseId = _allCourses.First(c => c.Name == "Database Systems").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "HTML & CSS Complete Guide", Url = "https://www.youtube.com/playlist?list=PL10", Likes = 678, CourseId = _allCourses.First(c => c.Name == "Web Development").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "JavaScript Essentials", Url = "https://www.youtube.com/playlist?list=PL11", Likes = 589, CourseId = _allCourses.First(c => c.Name == "Web Development").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "React.js Tutorial", Url = "https://www.youtube.com/playlist?list=PL12", Likes = 734, CourseId = _allCourses.First(c => c.Name == "Web Development").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Operating Systems Concepts", Url = "https://www.youtube.com/playlist?list=PL13", Likes = 412, CourseId = _allCourses.First(c => c.Name == "Operating Systems").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Linux Administration", Url = "https://www.youtube.com/playlist?list=PL14", Likes = 298, CourseId = _allCourses.First(c => c.Name == "Operating Systems").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Computer Networks Fundamentals", Url = "https://www.youtube.com/playlist?list=PL15", Likes = 367, CourseId = _allCourses.First(c => c.Name == "Computer Networks").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "TCP/IP Protocol Suite", Url = "https://www.youtube.com/playlist?list=PL16", Likes = 234, CourseId = _allCourses.First(c => c.Name == "Computer Networks").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Software Engineering Best Practices", Url = "https://www.youtube.com/playlist?list=PL17", Likes = 456, CourseId = _allCourses.First(c => c.Name == "Software Engineering").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Agile and Scrum", Url = "https://www.youtube.com/playlist?list=PL18", Likes = 389, CourseId = _allCourses.First(c => c.Name == "Software Engineering").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "AI Fundamentals", Url = "https://www.youtube.com/playlist?list=PL19", Likes = 812, CourseId = _allCourses.First(c => c.Name == "Artificial Intelligence").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Neural Networks Explained", Url = "https://www.youtube.com/playlist?list=PL20", Likes = 645, CourseId = _allCourses.First(c => c.Name == "Artificial Intelligence").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Machine Learning A-Z", Url = "https://www.youtube.com/playlist?list=PL21", Likes = 923, CourseId = _allCourses.First(c => c.Name == "Machine Learning").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Deep Learning Specialization", Url = "https://www.youtube.com/playlist?list=PL22", Likes = 867, CourseId = _allCourses.First(c => c.Name == "Machine Learning").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Android Development", Url = "https://www.youtube.com/playlist?list=PL23", Likes = 534, CourseId = _allCourses.First(c => c.Name == "Mobile App Development").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "iOS Development with Swift", Url = "https://www.youtube.com/playlist?list=PL24", Likes = 478, CourseId = _allCourses.First(c => c.Name == "Mobile App Development").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "AWS Cloud Practitioner", Url = "https://www.youtube.com/playlist?list=PL25", Likes = 612, CourseId = _allCourses.First(c => c.Name == "Cloud Computing").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Azure Fundamentals", Url = "https://www.youtube.com/playlist?list=PL26", Likes = 445, CourseId = _allCourses.First(c => c.Name == "Cloud Computing").Id },
                
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Ethical Hacking", Url = "https://www.youtube.com/playlist?list=PL27", Likes = 789, CourseId = _allCourses.First(c => c.Name == "Cybersecurity").Id },
                new CommunityPlaylist { Id = Guid.NewGuid(), Name = "Network Security", Url = "https://www.youtube.com/playlist?list=PL28", Likes = 567, CourseId = _allCourses.First(c => c.Name == "Cybersecurity").Id }
            };

            _playlistRequests = new ObservableCollection<PlaylistRequest>
            {
                new PlaylistRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Advanced Python Techniques", 
                    Url = "https://www.youtube.com/playlist?list=PLR1", 
                    CourseId = _allCourses.First(c => c.Name == "Introduction to Programming").Id, 
                    CourseName = "Introduction to Programming",
                    Status = "Approved",
                    SubmittedDate = DateTime.Now.AddDays(-15)
                },
                new PlaylistRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Graph Algorithms Visualization", 
                    Url = "https://www.youtube.com/playlist?list=PLR2", 
                    CourseId = _allCourses.First(c => c.Name == "Data Structures").Id, 
                    CourseName = "Data Structures",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-3)
                },
                new PlaylistRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "MongoDB Tutorial", 
                    Url = "https://www.youtube.com/playlist?list=PLR3", 
                    CourseId = _allCourses.First(c => c.Name == "Database Systems").Id, 
                    CourseName = "Database Systems",
                    Status = "Rejected",
                    SubmittedDate = DateTime.Now.AddDays(-8)
                },
                new PlaylistRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Vue.js Complete Course", 
                    Url = "https://www.youtube.com/playlist?list=PLR4", 
                    CourseId = _allCourses.First(c => c.Name == "Web Development").Id, 
                    CourseName = "Web Development",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-1)
                },
                new PlaylistRequest 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Docker and Kubernetes", 
                    Url = "https://www.youtube.com/playlist?list=PLR5", 
                    CourseId = _allCourses.First(c => c.Name == "Cloud Computing").Id, 
                    CourseName = "Cloud Computing",
                    Status = "Approved",
                    SubmittedDate = DateTime.Now.AddDays(-20)
                }
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
            if (sender is Button button && button.Tag is Guid courseId)
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
