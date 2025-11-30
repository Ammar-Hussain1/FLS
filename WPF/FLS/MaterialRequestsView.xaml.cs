using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FLS.DL;
using FLS.Helpers;
using FLS.Models;

namespace FLS
{
    public partial class MaterialRequestsView : UserControl
    {
        private ObservableCollection<MaterialRequest> _requests;
        private readonly ApiClient _apiClient;
        private bool _isAdminView;

        public MaterialRequestsView(bool isAdminView = false)
        {
            InitializeComponent();
            _requests = new ObservableCollection<MaterialRequest>();
            _apiClient = new ApiClient();
            _isAdminView = isAdminView;
            RequestsListView.ItemsSource = _requests;
            Loaded += MaterialRequestsView_Loaded;
        }

        private async void MaterialRequestsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRequests();
        }

        private async Task LoadRequests()
        {
            try
            {
                if (_isAdminView)
                {   
                    await LoadPendingRequests();
                }
                else
                {
                    await LoadMyRequests();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMyRequests()
        {
            var userId = AppSettings.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please log in to view your requests.", "Authentication Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var response = await _apiClient.GetMyRequestsAsync(userId);
            if (response.Success && response.Data != null)
            {
                _requests.Clear();
                foreach (var request in response.Data)
                {
                    _requests.Add(request);
                }
            }
            else
            {
                MessageBox.Show(response.Message ?? "Failed to load requests.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task LoadPendingRequests()
        {
            var response = await _apiClient.GetPendingRequestsAsync();
            if (response.Success && response.Data != null)
            {
                _requests.Clear();
                foreach (var request in response.Data)
                {
                    _requests.Add(request);
                }
            }
            else
            {
                MessageBox.Show(response.Message ?? "Failed to load pending requests.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string requestId)
            {
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    button.IsEnabled = false;
                    button.Content = "Approving...";

                    try
                    {
                        var response = await _apiClient.ApproveRequestAsync(requestId);
                        if (response.Success)
                        {
                            request.Status = "Approved";
                            MessageBox.Show($"Material request for {request.CourseName} has been approved!",
                                "Request Approved", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            await LoadRequests();
                        }
                        else
                        {
                            MessageBox.Show(response.Message ?? "Failed to approve request.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error approving request: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        button.IsEnabled = true;
                        button.Content = "Approve";
                    }
                }
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string requestId)
            {
                var request = _requests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to reject the material request for {request.CourseName}?",
                        "Confirm Rejection",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        button.IsEnabled = false;
                        button.Content = "Rejecting...";

                        try
                        {
                            var response = await _apiClient.RejectRequestAsync(requestId);
                            if (response.Success)
                            {
                                request.Status = "Rejected";
                                MessageBox.Show($"Material request for {request.CourseName} has been rejected.",
                                    "Request Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
                                        
                                await LoadRequests();
                            }
                            else
                            {
                                MessageBox.Show(response.Message ?? "Failed to reject request.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error rejecting request: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            button.IsEnabled = true;
                            button.Content = "Reject";
                        }
                    }
                }
            }
        }

        public async Task RefreshView()
        {
            await LoadRequests();
        }
    }
}
