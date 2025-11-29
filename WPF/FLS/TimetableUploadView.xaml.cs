using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace FLS
{
    /// <summary>
    /// Interaction logic for TimetableUploadView.xaml
    /// </summary>
    public partial class TimetableUploadView : UserControl
    {
        private string _selectedFilePath;
        private ObservableCollection<TimetableUploadRecord> _uploadHistory;

        public TimetableUploadView()
        {
            InitializeComponent();
            _uploadHistory = new ObservableCollection<TimetableUploadRecord>();
            UploadHistoryGrid.ItemsSource = _uploadHistory;
            LoadDummyHistory();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Timetable File",
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                DisplayFileInfo(_selectedFilePath);
            }
        }

        private void DisplayFileInfo(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Update UI
            SelectedFileText.Text = fileInfo.Name;
            FileNameText.Text = fileInfo.Name;
            FileSizeText.Text = FormatFileSize(fileInfo.Length);
            FileTypeText.Text = fileInfo.Extension.ToUpper();

            // Show file details panel
            FileDetailsPanel.Visibility = Visibility.Visible;
            ClearButton.Visibility = Visibility.Visible;

            // Enable upload button
            UploadButton.IsEnabled = true;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFileSelection();
        }

        private void ClearFileSelection()
        {
            _selectedFilePath = null;
            SelectedFileText.Text = "No file selected";
            FileDetailsPanel.Visibility = Visibility.Collapsed;
            ClearButton.Visibility = Visibility.Collapsed;
            UploadButton.IsEnabled = false;
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select a file first.", "No File Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Implement actual file upload and parsing logic
            // For now, just add to history
            FileInfo fileInfo = new FileInfo(_selectedFilePath);
            
            var uploadRecord = new TimetableUploadRecord
            {
                FileName = fileInfo.Name,
                UploadDate = DateTime.Now,
                FileSize = FormatFileSize(fileInfo.Length),
                Status = "Uploaded",
                UploadedBy = "Admin"
            };

            _uploadHistory.Insert(0, uploadRecord);

            MessageBox.Show($"Timetable file '{fileInfo.Name}' uploaded successfully!\n\nThe file will be processed shortly.",
                "Upload Successful", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear selection after upload
            ClearFileSelection();
        }

        private void LoadDummyHistory()
        {
            // Dummy data for demonstration
            _uploadHistory.Add(new TimetableUploadRecord
            {
                FileName = "Fall2024_Timetable.xlsx",
                UploadDate = DateTime.Now.AddDays(-5),
                FileSize = "245 KB",
                Status = "Completed",
                UploadedBy = "Admin"
            });

            _uploadHistory.Add(new TimetableUploadRecord
            {
                FileName = "Spring2024_Timetable.csv",
                UploadDate = DateTime.Now.AddDays(-15),
                FileSize = "189 KB",
                Status = "Completed",
                UploadedBy = "Admin"
            });

            _uploadHistory.Add(new TimetableUploadRecord
            {
                FileName = "Summer2024_Timetable.xlsx",
                UploadDate = DateTime.Now.AddDays(-30),
                FileSize = "312 KB",
                Status = "Completed",
                UploadedBy = "Admin"
            });
        }
    }

    public class TimetableUploadRecord
    {
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string FileSize { get; set; }
        public string Status { get; set; }
        public string UploadedBy { get; set; }
    }
}
