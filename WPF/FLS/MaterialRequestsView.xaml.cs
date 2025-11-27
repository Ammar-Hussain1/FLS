using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class MaterialRequestsView : UserControl
    {
        private ObservableCollection<MaterialRequest> _requests;

        public MaterialRequestsView()
        {
            InitializeComponent();
            _requests = new ObservableCollection<MaterialRequest>();
            RequestsListView.ItemsSource = _requests;
            LoadDummyRequests();
        }

        private void LoadDummyRequests()
        {
            _requests.Add(new MaterialRequest(1, "CS101", "Assignment", "Ali Ahmed", DateTime.Now.AddDays(-2), "Pending", "assignment1.pdf"));
            _requests.Add(new MaterialRequest(2, "MATH201", "Quiz", "Sara Khan", DateTime.Now.AddDays(-1), "Pending", "quiz2.pdf"));
            _requests.Add(new MaterialRequest(3, "PHY101", "Lab Report", "Hassan Ali", DateTime.Now.AddDays(-3), "Pending", "lab_report.pdf"));
            _requests.Add(new MaterialRequest(4, "CS102", "Project", "Fatima Noor", DateTime.Now.AddDays(-5), "Approved", "project.zip"));
            _requests.Add(new MaterialRequest(5, "ENG102", "Essay", "Usman Tariq", DateTime.Now.AddDays(-4), "Pending", "essay.docx"));
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
                    MessageBox.Show($"Material request for {request.CourseCode} has been approved!",
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
                        $"Are you sure you want to reject the material request for {request.CourseCode}?",
                        "Confirm Rejection",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        request.Status = "Rejected";
                        MessageBox.Show($"Material request for {request.CourseCode} has been rejected.",
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
