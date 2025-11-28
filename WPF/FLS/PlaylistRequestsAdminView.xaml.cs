using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class PlaylistRequestsAdminView : UserControl
    {
        private ObservableCollection<PlaylistRequest> _requests;

        public PlaylistRequestsAdminView()
        {
            InitializeComponent();
            _requests = new ObservableCollection<PlaylistRequest>();
            PlaylistRequestsListView.ItemsSource = _requests;
            LoadDummyRequests();
        }

        private void LoadDummyRequests()
        {
            _requests.Add(new PlaylistRequest
            {
                Id = 1,
                CourseCode = "CS101",
                PlaylistName = "Programming Basics",
                Url = "https://youtube.com/playlist?list=ABC123",
                SubmittedBy = "Ali Ahmed",
                SubmissionDate = DateTime.Now.AddDays(-3),
                Status = "Pending"
            });

            _requests.Add(new PlaylistRequest
            {
                Id = 2,
                CourseCode = "MATH201",
                PlaylistName = "Calculus Tutorials",
                Url = "https://youtube.com/playlist?list=XYZ789",
                SubmittedBy = "Sara Khan",
                SubmissionDate = DateTime.Now.AddDays(-1),
                Status = "Pending"
            });

            _requests.Add(new PlaylistRequest
            {
                Id = 3,
                CourseCode = "PHY101",
                PlaylistName = "Physics Lectures",
                Url = "https://youtube.com/playlist?list=PHY456",
                SubmittedBy = "Hassan Ali",
                SubmissionDate = DateTime.Now.AddDays(-5),
                Status = "Approved"
            });

            _requests.Add(new PlaylistRequest
            {
                Id = 4,
                CourseCode = "CS102",
                PlaylistName = "Data Structures Guide",
                Url = "https://youtube.com/playlist?list=DS2024",
                SubmittedBy = "Fatima Noor",
                SubmissionDate = DateTime.Now.AddDays(-2),
                Status = "Pending"
            });
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int requestId)
            {
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    request.Status = "Approved";
                    MessageBox.Show($"Playlist '{request.PlaylistName}' for {request.CourseCode} has been approved!",
                        "Request Approved", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    RefreshView();
                }
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int requestId)
            {
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to reject the playlist '{request.PlaylistName}' for {request.CourseCode}?",
                        "Confirm Rejection",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        request.Status = "Rejected";
                        MessageBox.Show($"Playlist '{request.PlaylistName}' has been rejected.",
                            "Request Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        RefreshView();
                    }
                }
            }
        }

        private void RefreshView()
        {
            var temp = _requests.ToList();
            _requests.Clear();
            foreach (var item in temp)
            {
                _requests.Add(item);
            }
        }
    }
}
