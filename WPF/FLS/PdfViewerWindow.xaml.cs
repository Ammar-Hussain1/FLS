using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace FLS
{
    public partial class PdfViewerWindow : Window
    {
        private readonly string _pdfUrl;

        public PdfViewerWindow(string url, string title)
        {
            InitializeComponent();
            _pdfUrl = url;
            TitleText.Text = title;
            Title = title;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await PdfWebView.EnsureCoreWebView2Async();
                if (!string.IsNullOrEmpty(_pdfUrl))
                {
                    PdfWebView.CoreWebView2.Navigate(_pdfUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing PDF viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
