using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FLS.Models;

namespace FLS
{
    public partial class CourseDetailView : UserControl
    {
        private Course _course;

        // Data Collections
        private List<CourseMaterial> _quizzes;
        private List<CourseMaterial> _assignments;
        private List<CourseMaterial> _mid1;
        private List<CourseMaterial> _mid2;
        private List<CourseMaterial> _finalExam;
        private List<Playlist> _playlists;

        public CourseDetailView()
        {
            InitializeComponent();
        }
        public void SetCourse(Course course)
        {
            _course = course;
            CourseNameText.Text = _course.Name;
            CourseCodeText.Text = _course.Code;
            CreditsText.Text = _course.Credits.ToString();
        }

        public void LoadDummyData()
        {
            _quizzes = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Id = 1,
                    Title = "Quiz 1: Introduction to Programming Concepts",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/quizzes/quiz1.pdf",
                    PreviewLink = "https://example.com/quizzes/quiz1.pdf",
                    Type = MaterialType.Quiz
                },
                new CourseMaterial
                {
                    Id = 2,
                    Title = "Quiz 2: Variables and Data Types",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/quizzes/quiz2.pdf",
                    PreviewLink = "https://example.com/quizzes/quiz2.pdf",
                    Type = MaterialType.Quiz
                },
                new CourseMaterial
                {
                    Id = 3,
                    Title = "Quiz 3: Control Structures",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/quizzes/quiz3.pdf",
                    PreviewLink = "https://example.com/quizzes/quiz3.pdf",
                    Type = MaterialType.Quiz
                }
            };

            _assignments = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Id = 1,
                    Title = "Assignment 1: Calculator Program",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/assignments/assignment1.pdf",
                    PreviewLink = "https://example.com/assignments/assignment1.pdf",
                    Type = MaterialType.Assignment
                },
                new CourseMaterial
                {
                    Id = 2,
                    Title = "Assignment 2: Array Manipulation",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/assignments/assignment2.pdf",
                    PreviewLink = "https://example.com/assignments/assignment2.pdf",
                    Type = MaterialType.Assignment
                },
                new CourseMaterial
                {
                    Id = 3,
                    Title = "Assignment 3: File Handling",
                    Semester = "Spring 2025",
                    DownloadLink = "https://example.com/assignments/assignment3.pdf",
                    PreviewLink = "https://example.com/assignments/assignment3.pdf",
                    Type = MaterialType.Assignment
                }
            };

            _mid1 = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Id = 1,
                    Title = "Mid Term 1 Exam",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/exams/mid1.pdf",
                    PreviewLink = "https://example.com/exams/mid1.pdf",
                    Type = MaterialType.Mid1
                }
            };

            _mid2 = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Id = 1,
                    Title = "Mid Term 2 Exam",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/exams/mid2.pdf",
                    PreviewLink = "https://example.com/exams/mid2.pdf",
                    Type = MaterialType.Mid2
                }
            };

            _finalExam = new List<CourseMaterial>
            {
                new CourseMaterial
                {
                    Id = 1,
                    Title = "Final Examination",
                    Semester = "Fall 2025",
                    DownloadLink = "https://example.com/exams/final.pdf",
                    PreviewLink = "https://example.com/exams/final.pdf",
                    Type = MaterialType.Final
                }
            };

            _playlists = new List<Playlist>
            {
                new Playlist
                {
                    Id = 1,
                    Title = "Introduction to Programming - Complete Series",
                    Description = "Complete video series covering all fundamental programming concepts",
                    Link = "https://www.youtube.com/playlist?list=PLdummy1",
                    ThumbnailUrl = "https://img.youtube.com/vi/dQw4w9WgXcQ/mqdefault.jpg"
                },
                new Playlist
                {
                    Id = 2,
                    Title = "Data Structures Explained",
                    Description = "In-depth explanations of various data structures with examples",
                    Link = "https://www.youtube.com/playlist?list=PLdummy2",
                    ThumbnailUrl = "https://img.youtube.com/vi/dQw4w9WgXcQ/mqdefault.jpg"
                },
                new Playlist
                {
                    Id = 3,
                    Title = "Algorithm Design Patterns",
                    Description = "Common algorithm patterns and problem-solving strategies",
                    Link = "https://www.youtube.com/playlist?list=PLdummy3",
                    ThumbnailUrl = "https://img.youtube.com/vi/dQw4w9WgXcQ/mqdefault.jpg"
                },
                new Playlist
                {
                    Id = 4,
                    Title = "Practice Problems and Solutions",
                    Description = "Step-by-step solutions to common programming problems",
                    Link = "https://www.youtube.com/playlist?list=PLdummy4",
                    ThumbnailUrl = "https://img.youtube.com/vi/dQw4w9WgXcQ/mqdefault.jpg"
                }
            };
        }

        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string category)
            {
                ShowCategory(category);
            }
        }

        private void ShowCategory(string category)
        {
            FoldersGrid.Visibility = Visibility.Collapsed;
            ItemsGrid.Visibility = Visibility.Visible;

            switch (category)
            {
                case "Quizzes":
                    CategoryTitleText.Text = "Quizzes";
                    CategoryItemsControl.ItemsSource = _quizzes;
                    break;
                case "Assignments":
                    CategoryTitleText.Text = "Assignments";
                    CategoryItemsControl.ItemsSource = _assignments;
                    break;
                case "Mid1":
                    CategoryTitleText.Text = "Mid Term 1";
                    CategoryItemsControl.ItemsSource = _mid1;
                    break;
                case "Mid2":
                    CategoryTitleText.Text = "Mid Term 2";
                    CategoryItemsControl.ItemsSource = _mid2;
                    break;
                case "Final":
                    CategoryTitleText.Text = "Final Exam";
                    CategoryItemsControl.ItemsSource = _finalExam;
                    break;
                case "Playlists":
                    CategoryTitleText.Text = "Playlists";
                    CategoryItemsControl.ItemsSource = _playlists;
                    break;
            }
        }

        private void BackToFolders_Click(object sender, RoutedEventArgs e)
        {
            ItemsGrid.Visibility = Visibility.Collapsed;
            FoldersGrid.Visibility = Visibility.Visible;
            CategoryItemsControl.ItemsSource = null;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string link)
            {
                try
                {
                    string title = "Document Preview";
                    if (button.DataContext is CourseMaterial material && material.Title != null)
                    {
                        title = material.Title;
                    }

                    var viewer = new PdfViewerWindow(link, title);
                    viewer.Owner = Window.GetWindow(this);
                    viewer.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open preview: {ex.Message}\n\nLink: {link}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string link)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true
                    });
                    MessageBox.Show("Download started! The file will open in your browser.", "Download", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not start download: {ex.Message}\n\nLink: {link}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void OpenPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string link)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open playlist: {ex.Message}\n\nLink: {link}", "Playlist Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public event EventHandler BackRequested;

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.Visibility == Visibility.Visible)
            {
                BackToFolders_Click(sender, e);
            }
            else
            {
                BackRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

