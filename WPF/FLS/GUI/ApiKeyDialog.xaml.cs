using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using FLS.Helpers;

namespace FLS
{
    public partial class ApiKeyDialog : Window
    {
        public string ApiKey { get; private set; }

        public ApiKeyDialog()
        {
            InitializeComponent();
            
            // Load existing API key if available
            string existingKey = AppSettings.GetApiKey();
            if (!string.IsNullOrWhiteSpace(existingKey))
            {
                ApiKeyInput.Text = existingKey;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Please enter an API key.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ApiKey = apiKey;
            AppSettings.SetApiKey(apiKey);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
            catch
            {
                MessageBox.Show("Unable to open link. Please visit: " + e.Uri.AbsoluteUri,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
