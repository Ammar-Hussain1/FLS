using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FLS.BL;
using FLS.DL;
using FLS.Models;
using WpfLoginApp;

namespace FLS
{
    public partial class SignUpPage : Window
    {
        private readonly UserService _userService;
        private readonly HttpClient _httpClient;

        public SignUpPage()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.CanResize;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            
            _httpClient = new HttpClient();
            var userApiClient = new UserApiClient(_httpClient);
            _userService = new UserService(userApiClient);
        }

        private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validate input
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Please enter your full name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Basic email validation
            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Disable button during request
            CreateAccountButton.IsEnabled = false;
            CreateAccountButton.Content = "Creating Account...";

            try
            {
                var response = await _userService.SignUpAsync(fullName, email, password);

                if (response.Success && response.Data != null)
                {
                    MessageBox.Show(
                        response.Message ?? "Account created successfully!",
                        "Account Created",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Navigate to login page or dashboard
                    LoginPage loginPage = new LoginPage();
                    loginPage.Show();
                    this.Close();
                }
                else
                {
                    string errorMessage = response.Message ?? "Sign up failed. Please try again.";
                    MessageBox.Show(errorMessage, "Sign Up Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable button
                CreateAccountButton.IsEnabled = true;
                CreateAccountButton.Content = "Create Account";
            }
        }

        private void AlreadyAccountButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            this.Close();
        }
    }
}
