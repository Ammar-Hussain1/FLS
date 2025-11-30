using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;
using WpfLoginApp;

namespace FLS
{
    public partial class Dashboard : Window
    {
        private AllCoursesView _allCoursesView;
        private SavedCoursesView _savedCoursesView;
        private CourseDetailView _courseDetailView;
        private CourseMaterialSubmissionView _courseMaterialSubmissionView;
        private ChatView _chatView;
        private UserPlaylistView _userPlaylistView;
        private TimetableView _timetableView;
        private UserControl _currentView;
        private UserControl _previousView;

        public Dashboard()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            LoadAllCoursesView();
        }

        private void LoadAllCoursesView()
        {
            if (_allCoursesView == null)
            {
                _allCoursesView = new AllCoursesView();
                _allCoursesView.SavedCoursesChanged += OnSavedCoursesChanged;
                _allCoursesView.CourseSelected += OnCourseSelected;
            }
            ContentArea.Content = _allCoursesView;
            _currentView = _allCoursesView;
            UpdateTabSelection(AllCoursesTab);
        }

        private void OnCourseSelected(Models.Course course)
        {
            _previousView = _currentView;
            
            if (_courseDetailView == null)
            {
                _courseDetailView = new CourseDetailView();
                _courseDetailView.BackRequested += OnCourseDetailBackRequested;
            }
            _courseDetailView.SetCourse(course);
            _ = _courseDetailView.LoadCourseMaterialsAsync();
            ContentArea.Content = _courseDetailView;
            _currentView = _courseDetailView;
        }

        private void OnCourseDetailBackRequested(object sender, EventArgs e)
        {
            if (_previousView != null)
            {
                ContentArea.Content = _previousView;
                _currentView = _previousView;
                
                if (_previousView == _allCoursesView)
                {
                    UpdateTabSelection(AllCoursesTab);
                }
                else if (_previousView == _savedCoursesView)
                {
                    UpdateTabSelection(SavedCoursesTab);
                }
                else if (_previousView == _courseMaterialSubmissionView)
                {
                    UpdateTabSelection(CourseMaterialTab);
                }
              
            }
            else
            {
                LoadAllCoursesView();
            }
        }

        private void LoadSavedCoursesView()
        {
            if (_savedCoursesView == null)
            {
                _savedCoursesView = new SavedCoursesView();
                _savedCoursesView.SetParentDashboard(this);
                _savedCoursesView.CourseSelected += OnCourseSelected;
            }
            
            if (_allCoursesView != null)
            {
                var savedCourses = _allCoursesView.GetSavedCourses();
                _savedCoursesView.SetSavedCourses(savedCourses);
            }
            
            ContentArea.Content = _savedCoursesView;
            _currentView = _savedCoursesView;
            UpdateTabSelection(SavedCoursesTab);
        }

        public void RemoveFromSavedCourses(string courseId)
        {
            if (_allCoursesView != null)
            {
                _allCoursesView.RemoveFromSaved(courseId);
            }
        }

        private void OnSavedCoursesChanged(ObservableCollection<UserCourse> savedCourses)
        {
            if (_savedCoursesView != null)
            {
                _savedCoursesView.SetSavedCourses(savedCourses);
            }
        }

        private void AllCoursesTab_Click(object sender, RoutedEventArgs e)
        {
            LoadAllCoursesView();
        }

        private void SavedCoursesTab_Click(object sender, RoutedEventArgs e)
        {
            LoadSavedCoursesView();
        }

        private void CourseMaterialTab_Click(object sender, RoutedEventArgs e)
        {
            LoadCourseMaterialSubmissionView();
        }

        private void LoadCourseMaterialSubmissionView()
        {
            if (_courseMaterialSubmissionView == null)
            {
                _courseMaterialSubmissionView = new CourseMaterialSubmissionView();
            }

            if (_allCoursesView != null)
            {
                var courses = _allCoursesView.GetCourses();
                _courseMaterialSubmissionView.SetCourses(courses);
            }

            ContentArea.Content = _courseMaterialSubmissionView;
            _currentView = _courseMaterialSubmissionView;
            UpdateTabSelection(CourseMaterialTab);
        }

        private void MyProgressTab_Click(object sender, RoutedEventArgs e)
        {
            LoadTimetableView();
        }

        private void LoadTimetableView()
        {
            if (_timetableView == null)
            {
                _timetableView = new TimetableView();
            }
            ContentArea.Content = _timetableView;
            _currentView = _timetableView;
            UpdateTabSelection(MyProgressTab);
        }

        private void SettingsTab_Click(object sender, RoutedEventArgs e)
        {
            var settingsView = new TextBlock
            {
                Text = "Settings - Coming Soon!",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"))
            };
            ContentArea.Content = settingsView;
            _currentView = null;
            UpdateTabSelection(SettingsTab);
        }

        private void AIChatTab_Click(object sender, RoutedEventArgs e)
        {
            LoadChatView();
        }

        private void LoadChatView()
        {
            if (_chatView == null)
            {
                _chatView = new ChatView();
            }
            ContentArea.Content = _chatView;
            _currentView = _chatView;
            UpdateTabSelection(AIChatTab);
        }

        private void UserPlaylistsTab_Click(object sender, RoutedEventArgs e)
        {
            LoadUserPlaylistView();
        }

        private void LoadUserPlaylistView()
        {
            if (_userPlaylistView == null)
            {
                _userPlaylistView = new UserPlaylistView();
            }
            ContentArea.Content = _userPlaylistView;
            _currentView = _userPlaylistView;
            UpdateTabSelection(UserPlaylistsTab);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginPage loginPage = new LoginPage();
                loginPage.Show();
                this.Close();
            }
        }

        private void UpdateTabSelection(Button selectedButton)
        {
            AllCoursesTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            SavedCoursesTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            CourseMaterialTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            MyProgressTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            SettingsTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            AIChatTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));
            UserPlaylistsTab.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2B2D42"));

            selectedButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4281A4"));
        }
    }
}

