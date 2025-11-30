using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FLS.DL;
using FLS.Helpers;
using FLS.Models;
using Microsoft.Web.WebView2.Core;

namespace FLS
{
    public partial class CourseMaterialSubmissionView : UserControl
    {
        private ObservableCollection<Course> _courses;
        private Course _selectedCourse;
        private MaterialType _selectedMaterialType;
        private string _selectedFilePath = string.Empty;
        private readonly ApiClient _apiClient;

        public CourseMaterialSubmissionView()
        {
            InitializeComponent();
            _courses = new ObservableCollection<Course>();
            _apiClient = new ApiClient();
            LoadCourses();
            InitializeMaterialTypeComboBox();
            Loaded += CourseMaterialSubmissionView_Loaded;
        }

        private async void CourseMaterialSubmissionView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await PdfWebView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize PDF viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadCourses()
        {
            try
            {
                var allCourses = new List<CourseDTO>();
                int currentPage = 1;
                int pageSize = 100;
                bool hasMorePages = true;

                while (hasMorePages)
                {
                    var response = await _apiClient.GetCoursesAsync(currentPage, pageSize);
                    
                    if (response.Success && response.Data != null)
                    {
                        allCourses.AddRange(response.Data.Data);
                        
                        if (response.Data.Pagination != null)
                        {
                            hasMorePages = response.Data.Pagination.HasNextPage;
                            currentPage++;
                        }
                        else
                        {
                            hasMorePages = false;
                        }
                    }
                    else
                    {
                        hasMorePages = false;
                        if (currentPage == 1)
                        {
                            ShowStatusMessage(response.Message ?? "Failed to load courses.", false);
                        }
                    }
                }

                _courses.Clear();
                foreach (var courseDto in allCourses)
                {
                    _courses.Add(new Course
                    {
                        Id = courseDto.Id,
                        Name = courseDto.Name,
                        Code = courseDto.Code,
                        Description = courseDto.Description ?? string.Empty,
                        Credits = 0,
                        CreatedDate = DateTime.Now
                    });
                }
                
                CourseComboBox.ItemsSource = _courses;
                
                if (_courses.Count == 0)
                {
                    ShowStatusMessage("No courses found.", false);
                }
                else
                {
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error loading courses: {ex.Message}", false);
            }
        }

        private void InitializeMaterialTypeComboBox()
        {
            MaterialTypeComboBox.SelectedIndex = 0;
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCourse = CourseComboBox.SelectedItem as Course;
            ValidateForm();
        }

        private void MaterialTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialTypeComboBox.SelectedItem is ComboBoxItem item)
            {
                string materialTypeStr = item.Content.ToString();
                if (Enum.TryParse<MaterialType>(materialTypeStr, out MaterialType materialType))
                {
                    _selectedMaterialType = materialType;
                }
            }
            ValidateForm();
        }

        private void SemesterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                FilePathTextBox.Text = Path.GetFileName(_selectedFilePath);
                ShowFileInformation(_selectedFilePath);
                ValidateForm();
            }
        }

        private void ShowFileInformation(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                PdfWebView.Visibility = Visibility.Collapsed;
                NoPreviewPanel.Visibility = Visibility.Visible;
                FileInfoPanel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                string fileSize = GetFileSize(fileInfo.Length);

                if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    PdfWebView.Visibility = Visibility.Visible;
                    NoPreviewPanel.Visibility = Visibility.Collapsed;
                    if (PdfWebView.CoreWebView2 != null)
                    {
                        PdfWebView.CoreWebView2.Navigate(filePath);
                    }
                }
                else
                {
                    PdfWebView.Visibility = Visibility.Collapsed;
                    NoPreviewPanel.Visibility = Visibility.Visible;
                }

                FileNameText.Text = $"File: {Path.GetFileName(filePath)}";
                FileSizeText.Text = $"Size: {fileSize} | Type: PDF";
                FileInfoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file information: {ex.Message}",
                    "File Error", MessageBoxButton.OK, MessageBoxImage.Error);

                PdfWebView.Visibility = Visibility.Collapsed;
                NoPreviewPanel.Visibility = Visibility.Visible;
                FileInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private string GetFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Re-validate and get selected course from ComboBox
            _selectedCourse = CourseComboBox.SelectedItem as Course;
            
            if (!ValidateForm())
            {
                MessageBox.Show("Please fill in all required fields and select a valid PDF file.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Double-check that course is selected and has valid data
            if (_selectedCourse == null || string.IsNullOrWhiteSpace(_selectedCourse.Name))
            {
                MessageBox.Show("Please select a valid course.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var userId = AppSettings.GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please log in to submit materials.", "Authentication Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show("Please select a valid PDF file.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubmitButton.IsEnabled = false;
            SubmitButton.Content = "Uploading...";
            ShowStatusMessage("Uploading file...", true);

            try
            {
                if (MaterialTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string materialTypeStr = selectedItem.Content.ToString();
                    if (Enum.TryParse<MaterialType>(materialTypeStr, out MaterialType materialType))
                    {
                        _selectedMaterialType = materialType;
                    }
                }
                
                string fileType = MapMaterialTypeToFileType(_selectedMaterialType);
                
                int? year = null;
                if (!string.IsNullOrWhiteSpace(SemesterTextBox.Text))
                {
                    var yearMatch = System.Text.RegularExpressions.Regex.Match(SemesterTextBox.Text, @"\b(20\d{2})\b");
                    if (yearMatch.Success && int.TryParse(yearMatch.Value, out var parsedYear))
                    {
                        year = parsedYear;
                    }
                }

                string courseName = _selectedCourse?.Name ?? "Unknown Course";
                string fileName = !string.IsNullOrEmpty(_selectedFilePath) ? Path.GetFileName(_selectedFilePath) : "Unknown File";
                
                var response = await _apiClient.UploadMaterialAsync(
                    userId,
                    _selectedFilePath,
                    _selectedCourse.Name,
                    fileType,
                    year
                );

                if (response.Success)
                {
                    ShowStatusMessage("Material submitted successfully! It will be reviewed by an admin.", true);
                    
                    ResetForm();

                    MessageBox.Show(
                        $"Course material submitted successfully!\n\n" +
                        $"Course: {courseName}\n" +
                        $"Type: {fileType}\n" +
                        $"File: {fileName}\n\n" +
                        "Your submission is pending admin approval.",
                        "Submission Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    ShowStatusMessage(response.Message ?? "Failed to submit material.", false);
                    MessageBox.Show(
                        response.Message ?? "Failed to submit material. Please try again.",
                        "Submission Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error submitting material: {ex.Message}", false);
                MessageBox.Show($"Error submitting material: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "Submit";
            }
        }

        private string MapMaterialTypeToFileType(MaterialType materialType)
        {
            return materialType switch
            {
                MaterialType.Quiz => "quizzes",
                MaterialType.Assignment => "assignments",
                MaterialType.Mid1 => "Midterm 1",
                MaterialType.Mid2 => "Midterm 2",
                MaterialType.Final => "Final",
                _ => "assignments"
            };
        }

        private bool ValidateForm()
        {
            bool isValid = _selectedCourse != null &&
                          !string.IsNullOrWhiteSpace(SemesterTextBox.Text) &&
                          !string.IsNullOrEmpty(_selectedFilePath) &&
                          File.Exists(_selectedFilePath);

            SubmitButton.IsEnabled = isValid;
            return isValid;
        }

        private void ResetForm()
        {
            CourseComboBox.SelectedIndex = -1;
            MaterialTypeComboBox.SelectedIndex = 0;
            SemesterTextBox.Clear();
            FilePathTextBox.Clear();
            _selectedFilePath = string.Empty;
            _selectedCourse = null;
            
            PdfWebView.Visibility = Visibility.Collapsed;
            NoPreviewPanel.Visibility = Visibility.Visible;
            if (PdfWebView.CoreWebView2 != null)
            {
                PdfWebView.CoreWebView2.Navigate("about:blank");
            }

            FileInfoPanel.Visibility = Visibility.Collapsed;
            ValidateForm();
            StatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ShowStatusMessage(string message, bool isSuccess)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isSuccess
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(72, 169, 166))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 57, 70));
            StatusTextBlock.Visibility = Visibility.Visible;
        }

        public void SetCourses(ObservableCollection<Course> courses)
        {
            _courses.Clear();
            foreach (var course in courses)
            {
                _courses.Add(course);
            }
            CourseComboBox.ItemsSource = _courses;
        }
    }
}

