using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfLoginApp;

namespace FLS
{
    public partial class AdminDashboard : Window
    {
        private CourseManagementView _courseManagementView;
        private MaterialRequestsView _materialRequestsView;
        private PlaylistRequestsAdminView _playlistRequestsAdminView;
        private TimetableUploadView _timetableUploadView;
        private UserControl _currentView;

        public AdminDashboard()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            LoadCourseManagementView();
        }

        private void CourseManagementTab_Click(object sender, RoutedEventArgs e)
        {
            LoadCourseManagementView();
        }

        private void LoadCourseManagementView()
        {
            if (_courseManagementView == null)
            {
                _courseManagementView = new CourseManagementView();
            }
            ContentArea.Content = _courseManagementView;
            _currentView = _courseManagementView;
            UpdateTabSelection(CourseManagementTab);
        }

        private void MaterialRequestsTab_Click(object sender, RoutedEventArgs e)
        {
            LoadMaterialRequestsView();
        }

        private void LoadMaterialRequestsView()
        {
            if (_materialRequestsView == null)
            {
                _materialRequestsView = new MaterialRequestsView(isAdminView: true);
            }
            ContentArea.Content = _materialRequestsView;
            _currentView = _materialRequestsView;
            UpdateTabSelection(MaterialRequestsTab);
        }

        private void PlaylistRequestsTab_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylistRequestsAdminView();
        }

        private void LoadPlaylistRequestsAdminView()
        {
            if (_playlistRequestsAdminView == null)
            {
                _playlistRequestsAdminView = new PlaylistRequestsAdminView();
            }
            ContentArea.Content = _playlistRequestsAdminView;
            _currentView = _playlistRequestsAdminView;
            UpdateTabSelection(PlaylistRequestsTab);
        }

        private void TimetableUploadTab_Click(object sender, RoutedEventArgs e)
        {
            LoadTimetableUploadView();
        }

        private void LoadTimetableUploadView()
        {
            if (_timetableUploadView == null)
            {
                _timetableUploadView = new TimetableUploadView();
            }
            ContentArea.Content = _timetableUploadView;
            _currentView = _timetableUploadView;
            UpdateTabSelection(TimetableUploadTab);
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
            CourseManagementTab.Background = new SolidColorBrush(Color.FromRgb(43, 45, 66));
            MaterialRequestsTab.Background = new SolidColorBrush(Color.FromRgb(43, 45, 66));
            PlaylistRequestsTab.Background = new SolidColorBrush(Color.FromRgb(43, 45, 66));
            TimetableUploadTab.Background = new SolidColorBrush(Color.FromRgb(43, 45, 66));

            selectedButton.Background = new SolidColorBrush(Color.FromRgb(66, 129, 164));
        }
    }
}
