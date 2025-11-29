using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FLS.Models;

namespace FLS
{
    public partial class UserPlaylistView : UserControl
    {
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
            LoadDummyData();
            UpdatePagination();
        }

        private void LoadDummyData()
        {
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

            _allCommunityPlaylists = new List<CommunityPlaylist>
            {
                new CommunityPlaylist { Id = 1, Name = "Python Programming for Beginners", Url = "https://www.youtube.com/playlist?list=PL1", Likes = 245, CourseId = "1" },
                new CommunityPlaylist { Id = 2, Name = "C++ Fundamentals", Url = "https://www.youtube.com/playlist?list=PL2", Likes = 189, CourseId = "1" },
                new CommunityPlaylist { Id = 3, Name = "Java Complete Course", Url = "https://www.youtube.com/playlist?list=PL3", Likes = 312, CourseId = "1" },
                
                new CommunityPlaylist { Id = 4, Name = "Data Structures Masterclass", Url = "https://www.youtube.com/playlist?list=PL4", Likes = 428, CourseId = "2" },
                new CommunityPlaylist { Id = 5, Name = "Algorithms Explained", Url = "https://www.youtube.com/playlist?list=PL5", Likes = 356, CourseId = "2" },
                new CommunityPlaylist { Id = 6, Name = "Trees and Graphs", Url = "https://www.youtube.com/playlist?list=PL6", Likes = 267, CourseId = "2" },
                
                new CommunityPlaylist { Id = 7, Name = "SQL Tutorial", Url = "https://www.youtube.com/playlist?list=PL7", Likes = 523, CourseId = "3" },
                new CommunityPlaylist { Id = 8, Name = "Database Design Principles", Url = "https://www.youtube.com/playlist?list=PL8", Likes = 198, CourseId = "3" },
                new CommunityPlaylist { Id = 9, Name = "NoSQL Databases", Url = "https://www.youtube.com/playlist?list=PL9", Likes = 145, CourseId = "3" },
                
                new CommunityPlaylist { Id = 10, Name = "HTML & CSS Complete Guide", Url = "https://www.youtube.com/playlist?list=PL10", Likes = 678, CourseId = "4" },
                new CommunityPlaylist { Id = 11, Name = "JavaScript Essentials", Url = "https://www.youtube.com/playlist?list=PL11", Likes = 589, CourseId = "4" },
                new CommunityPlaylist { Id = 12, Name = "React.js Tutorial", Url = "https://www.youtube.com/playlist?list=PL12", Likes = 734, CourseId = "4" },
                
                new CommunityPlaylist { Id = 13, Name = "Operating Systems Concepts", Url = "https://www.youtube.com/playlist?list=PL13", Likes = 412, CourseId = "5" },
                new CommunityPlaylist { Id = 14, Name = "Linux Administration", Url = "https://www.youtube.com/playlist?list=PL14", Likes = 298, CourseId = "5" },
                
                new CommunityPlaylist { Id = 15, Name = "Computer Networks Fundamentals", Url = "https://www.youtube.com/playlist?list=PL15", Likes = 367, CourseId = "6" },
                new CommunityPlaylist { Id = 16, Name = "TCP/IP Protocol Suite", Url = "https://www.youtube.com/playlist?list=PL16", Likes = 234, CourseId = "6" },
                
                new CommunityPlaylist { Id = 17, Name = "Software Engineering Best Practices", Url = "https://www.youtube.com/playlist?list=PL17", Likes = 456, CourseId = "7" },
                new CommunityPlaylist { Id = 18, Name = "Agile and Scrum", Url = "https://www.youtube.com/playlist?list=PL18", Likes = 389, CourseId = "7" },
                
                new CommunityPlaylist { Id = 19, Name = "AI Fundamentals", Url = "https://www.youtube.com/playlist?list=PL19", Likes = 812, CourseId = "8" },
                new CommunityPlaylist { Id = 20, Name = "Neural Networks Explained", Url = "https://www.youtube.com/playlist?list=PL20", Likes = 645, CourseId = "8" },
                
                new CommunityPlaylist { Id = 21, Name = "Machine Learning A-Z", Url = "https://www.youtube.com/playlist?list=PL21", Likes = 923, CourseId = "9" },
                new CommunityPlaylist { Id = 22, Name = "Deep Learning Specialization", Url = "https://www.youtube.com/playlist?list=PL22", Likes = 867, CourseId = "9" },
                
                new CommunityPlaylist { Id = 23, Name = "Android Development", Url = "https://www.youtube.com/playlist?list=PL23", Likes = 534, CourseId = "10" },
                new CommunityPlaylist { Id = 24, Name = "iOS Development with Swift", Url = "https://www.youtube.com/playlist?list=PL24", Likes = 478, CourseId = "10" },
                
                new CommunityPlaylist { Id = 25, Name = "AWS Cloud Practitioner", Url = "https://www.youtube.com/playlist?list=PL25", Likes = 612, CourseId = "11" },
                new CommunityPlaylist { Id = 26, Name = "Azure Fundamentals", Url = "https://www.youtube.com/playlist?list=PL26", Likes = 445, CourseId = "11" },
                
                new CommunityPlaylist { Id = 27, Name = "Ethical Hacking", Url = "https://www.youtube.com/playlist?list=PL27", Likes = 789, CourseId = "12" },
                new CommunityPlaylist { Id = 28, Name = "Network Security", Url = "https://www.youtube.com/playlist?list=PL28", Likes = 567, CourseId = "12" }
            };

            _playlistRequests = new ObservableCollection<PlaylistRequest>
            {
                new PlaylistRequest 
                { 
                    Id = 1, 
                    Name = "Advanced Python Techniques", 
                    Url = "https://www.youtube.com/playlist?list=PLR1", 
                    CourseId = "1", 
                    CourseName = "Introduction to Programming",
                    Status = "Approved",
                    SubmittedDate = DateTime.Now.AddDays(-15)
                },
                new PlaylistRequest 
                { 
                    Id = 2, 
                    Name = "Graph Algorithms Visualization", 
                    Url = "https://www.youtube.com/playlist?list=PLR2", 
                    CourseId = "2", 
                    CourseName = "Data Structures",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-3)
                },
                new PlaylistRequest 
                { 
                    Id = 3, 
                    Name = "MongoDB Tutorial", 
                    Url = "https://www.youtube.com/playlist?list=PLR3", 
                    CourseId = "3", 
                    CourseName = "Database Systems",
                    Status = "Rejected",
                    SubmittedDate = DateTime.Now.AddDays(-8)
                },
                new PlaylistRequest 
                { 
                    Id = 4, 
                    Name = "Vue.js Complete Course", 
                    Url = "https://www.youtube.com/playlist?list=PLR4", 
                    CourseId = "4", 
                    CourseName = "Web Development",
                    Status = "Pending",
                    SubmittedDate = DateTime.Now.AddDays(-1)
                },
                new PlaylistRequest 
                { 
                    Id = 5, 
                    Name = "Docker and Kubernetes", 
                    Url = "https://www.youtube.com/playlist?list=PLR5", 
                    CourseId = "11", 
                    CourseName = "Cloud Computing",
                    Status = "Approved",
                    SubmittedDate = DateTime.Now.AddDays(-20)
                }
            };

            CourseComboBox.ItemsSource = _allCourses;
            
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
                MessageBox.Show($"Could not open URL: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var name = PlaylistNameInput.Text.Trim();
            var url = PlaylistUrlInput.Text.Trim();
            var selectedCourse = CourseComboBox.SelectedItem as Course;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a playlist name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a playlist URL.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedCourse == null)
            {
                MessageBox.Show("Please select a course.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newRequest = new PlaylistRequest
            {
                Id = _playlistRequests.Count + 1,
                Name = name,
                Url = url,
                CourseId = selectedCourse.Id,
                CourseName = selectedCourse.Name,
                Status = "Pending",
                SubmittedDate = DateTime.Now
            };

            _playlistRequests.Add(newRequest);

            PlaylistNameInput.Clear();
            PlaylistUrlInput.Clear();
            CourseComboBox.SelectedIndex = -1;

            MessageBox.Show($"Playlist request submitted successfully!\n\nName: {name}\nCourse: {selectedCourse.Name}\nStatus: Pending", 
                "Request Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
