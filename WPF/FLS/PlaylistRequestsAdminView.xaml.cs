using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;
using FLS.Services;

namespace FLS
{
    public partial class PlaylistRequestsAdminView : UserControl
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "http://localhost:5000/api";

        private ObservableCollection<PlaylistRequest> _requests;

        public PlaylistRequestsAdminView()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _requests = new ObservableCollection<PlaylistRequest>();
            PlaylistRequestsListView.ItemsSource = _requests;
            LoadRequestsAsync();
        }

        private async Task LoadRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Playlist/requests");
                
                if (response.IsSuccessStatusCode)
                {
                    var requests = await response.Content.ReadFromJsonAsync<List<PlaylistRequest>>() 
                        ?? new List<PlaylistRequest>();
                    
                    _requests.Clear();
                    foreach (var request in requests)
                    {
                        _requests.Add(request);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to load playlist requests.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid requestId)
            {
                try
                {
                    var approveDto = new { AdminId = SessionManager.Instance.GetCurrentUserId() };
                    var json = JsonSerializer.Serialize(approveDto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await _httpClient.PutAsync($"{_apiBaseUrl}/Playlist/approve/{requestId}", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var request = _requests.FirstOrDefault(r => r.Id == requestId);
                        if (request != null)
                        {
                            MessageBox.Show($"Playlist '{request.PlaylistName}' for {request.CourseCode} has been approved!",
                                "Request Approved", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        
                        await LoadRequestsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to approve request.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error approving request: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid requestId)
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
                        try
                        {
                            var rejectDto = new 
                            { 
                                AdminId = SessionManager.Instance.GetCurrentUserId(),
                                Reason = "Does not meet quality standards" // TODO: Add input dialog for reason
                            };
                            var json = JsonSerializer.Serialize(rejectDto);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            
                            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/Playlist/reject/{requestId}", content);
                            
                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show($"Playlist '{request.PlaylistName}' has been rejected.",
                                    "Request Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                await LoadRequestsAsync();
                            }
                            else
                            {
                                MessageBox.Show("Failed to reject request.", "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error rejecting request: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}
