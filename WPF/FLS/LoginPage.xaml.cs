using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Threading.Tasks;
using FLS;
using FLS.DL;
using FLS.Models;
using FLS.Services;
using FLS.Helpers;

namespace WpfLoginApp
{
  
    public partial class LoginPage : Window
    {
        private readonly ApiClient _apiClient;

        public LoginPage()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.CanResize;
            this.WindowStyle = WindowStyle.SingleBorderWindow;

            _apiClient = new ApiClient();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e) {
            SignUpPage signUpPage = new SignUpPage();
            signUpPage.Show();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e) 
        {
            string email = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Validate input
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable login button during request
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Logging in...";

            try
            {
                var request = new SignInRequest
                {
                    Email = email,
                    Password = password
                };

                var response = await _apiClient.SignInAsync(request);

                if (response.Success && response.Data != null)
                {
                    // Store user info in session
                    var user = response.Data;
                    bool isAdmin = user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
                    
                    // Initialize session
                    SessionManager.Instance.Login(user.Id, user.FullName, user.Email, isAdmin);

                    // Check if admin
                    if (isAdmin)
                    {
                        AdminDashboard adminDashboard = new AdminDashboard();
                        adminDashboard.Show();
                        this.Close();
                    }
                    else
                    {
                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                        this.Close();
                    }
                }
                else
                {
                    string errorMessage = response.Message ?? "Login failed. Please check your credentials and try again.";
                    MessageBox.Show(errorMessage, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable login button
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Login";
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e) { }

       
    }
}
