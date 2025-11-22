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

        public CourseDetailView()
        {
            InitializeComponent();
        }

        public void LoadCourse(Course course)
        {
            _course = course;
            
            CourseNameText.Text = course.Name;
            CourseCodeText.Text = $"Course Code: {course.Code}";
            InstructorText.Text = course.Instructor;
            CreditsText.Text = course.Credits.ToString();

            LoadDummyData();
        }

        private void LoadDummyData()
        {
            var quizzes = new List<CourseMaterial>
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
            QuizzesItemsControl.ItemsSource = quizzes;

            var assignments = new List<CourseMaterial>
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
            AssignmentsItemsControl.ItemsSource = assignments;

            var mid1 = new List<CourseMaterial>
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
            Mid1ItemsControl.ItemsSource = mid1;

            var mid2 = new List<CourseMaterial>
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
            Mid2ItemsControl.ItemsSource = mid2;

            var finalExam = new List<CourseMaterial>
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
            FinalExamItemsControl.ItemsSource = finalExam;

            var playlists = new List<Playlist>
            {
                new Playlist
                {
                    Id = 1,
                    Title = "Introduction to Programming - Complete Series",
                    Description = "Complete video series covering all fundamental programming concepts",
                    Link = "https://www.youtube.com/playlist?list=PLdummy1"
                },
                new Playlist
                {
                    Id = 2,
                    Title = "Data Structures Explained",
                    Description = "In-depth explanations of various data structures with examples",
                    Link = "https://www.youtube.com/playlist?list=PLdummy2"
                },
                new Playlist
                {
                    Id = 3,
                    Title = "Algorithm Design Patterns",
                    Description = "Common algorithm patterns and problem-solving strategies",
                    Link = "https://www.youtube.com/playlist?list=PLdummy3"
                },
                new Playlist
                {
                    Id = 4,
                    Title = "Practice Problems and Solutions",
                    Description = "Step-by-step solutions to common programming problems",
                    Link = "https://www.youtube.com/playlist?list=PLdummy4"
                }
            };
            PlaylistsItemsControl.ItemsSource = playlists;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
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
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

