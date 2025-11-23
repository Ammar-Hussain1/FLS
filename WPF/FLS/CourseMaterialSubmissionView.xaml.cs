using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        private int _nextSubmissionId = 1;

        public CourseMaterialSubmissionView()
        {
            InitializeComponent();
            _courses = new ObservableCollection<Course>();
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

        private void LoadCourses()
        {
            // Load courses from AllCoursesView or API
            // For now, using dummy data similar to AllCoursesView
            if (_courses.Count == 0)
            {
                var dummyCourses = new[]
                {
                    new Course
                    {
                        Id = 1,
                        Name = "Introduction to Programming",
                        Code = "CS101",
                        Description = "Fundamentals of programming and problem-solving",
                        Credits = 3,
                        CreatedDate = DateTime.Now.AddDays(-30)
                    },
                    new Course
                    {
                        Id = 2,
                        Name = "Data Structures and Algorithms",
                        Code = "CS201",
                        Description = "Advanced data structures and algorithm design",
                        Credits = 4,
                        CreatedDate = DateTime.Now.AddDays(-20)
                    },
                    new Course
                    {
                        Id = 3,
                        Name = "Database Systems",
                        Code = "CS301",
                        Description = "Design and implementation of database systems",
                        Credits = 3,
                        CreatedDate = DateTime.Now.AddDays(-10)
                    },
                    new Course
                    {
                        Id = 4,
                        Name = "Web Development",
                        Code = "CS401",
                        Description = "Modern web development with HTML, CSS, and JavaScript",
                        Credits = 3,
                        CreatedDate = DateTime.Now.AddDays(-5)
                    },
                    new Course
                    {
                        Id = 5,
                        Name = "Machine Learning",
                        Code = "CS501",
                        Description = "Introduction to machine learning algorithms and applications",
                        Credits = 4,
                        CreatedDate = DateTime.Now.AddDays(-2)
                    }
                };

                foreach (var course in dummyCourses)
                {
                    _courses.Add(course);
                }
            }

            CourseComboBox.ItemsSource = _courses;
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

                // Update file info panel in form
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

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("Please fill in all required fields and select a valid PDF file.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create submission object
                var submission = new CourseMaterialSubmission
                {
                    Id = _nextSubmissionId++,
                    CourseId = _selectedCourse.Id,
                    Course = _selectedCourse,
                    MaterialType = _selectedMaterialType,
                    Semester = SemesterTextBox.Text.Trim(),
                    FilePath = _selectedFilePath,
                    FileName = Path.GetFileName(_selectedFilePath),
                    SubmittedDate = DateTime.Now,
                    IsApproved = false,
                    SubmittedBy = "CurrentUser" // TODO: Get from authentication
                };

                // TODO: Save to database/API
                // For now, just show success message
                ShowStatusMessage($"Material submitted successfully! It will be reviewed by an admin.", true);

                // Reset form
                ResetForm();

                MessageBox.Show(
                    $"Course material submitted successfully!\n\n" +
                    $"Course: {_selectedCourse.Name}\n" +
                    $"Type: {_selectedMaterialType}\n" +
                    $"Semester: {submission.Semester}\n" +
                    $"File: {submission.FileName}\n\n" +
                    "Your submission is pending admin approval.",
                    "Submission Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting material: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        // Public method to set courses from external source (e.g., AllCoursesView)
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
