using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FLS.DL;
using FLS.Models;
using FLS.Services;

namespace FLS
{
    public partial class TimetableView : UserControl
    {
        private ObservableCollection<TimetableRow> _timetableRows;
        private List<TimetableSlot> _allSlots;
        private readonly ApiClient _apiClient;
        private readonly HttpClient _httpClient;

        public TimetableView()
        {
            InitializeComponent();
            _timetableRows = new ObservableCollection<TimetableRow>();
            _apiClient = new ApiClient();
            _httpClient = new HttpClient();
            TimetableGrid.ItemsSource = _timetableRows;
            LoadTimetableDataAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadTimetableDataAsync();
            MessageBox.Show("Timetable refreshed successfully!", "Refresh", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task LoadTimetableDataAsync()
        {
            try
            {
                // Show loading state
                RefreshButton.IsEnabled = false;
                RefreshButton.Content = "Loading...";

                // Determine current user
                string userId;
                try
                {
                    userId = SessionManager.Instance.GetCurrentUserId();
                }
                catch (InvalidOperationException)
                {
                    _timetableRows.Clear();
                    EnrolledSectionsText.Text = "Please log in to view your timetable.";
                    return;
                }

                // Fetch user-specific timetable data from API
                var timetableData = await _apiClient.GetMyTimetableAsync(userId);

                if (timetableData == null || !timetableData.Any())
                {
                    _timetableRows.Clear();
                    EnrolledSectionsText.Text = "No timetable data available. Please contact administrator to upload timetable.";
                    return;
                }

                // Fetch all courses to get course codes
                var allCourses = new List<CourseDTO>();
                int currentPage = 1;
                int pageSize = 100;
                bool hasMorePages = true;

                while (hasMorePages)
                {
                    var coursesResponse = await _apiClient.GetCoursesAsync(currentPage, pageSize);
                    if (coursesResponse.Success && coursesResponse.Data != null)
                    {
                        allCourses.AddRange(coursesResponse.Data.Data);
                        hasMorePages = coursesResponse.Data.Pagination?.HasNextPage ?? false;
                        currentPage++;
                    }
                    else
                    {
                        hasMorePages = false;
                    }
                }

                var courseDict = allCourses.ToDictionary(c => c.Id, c => c);

                // Map timetable data to TimetableSlot objects
                _allSlots = new List<TimetableSlot>();
                
                foreach (var item in timetableData)
                {
                    var course = courseDict.GetValueOrDefault(item.CourseId);
                    var courseCode = course?.Code ?? "N/A";
                    var courseName = item.Subject ?? course?.Name ?? "Unknown Course";
                    var sectionName = item.SectionName ?? "N/A";
                    var instructorName = item.InstructorName ?? "N/A";
                    
                    // Normalize day name (Mon -> Monday, etc.)
                    var dayName = NormalizeDayName(item.Day);
                    
                    _allSlots.Add(new TimetableSlot(
                        dayName,
                        item.Time ?? "TBA",
                        courseCode,
                        courseName,
                        sectionName,
                        item.Room ?? "TBA",
                        instructorName
                    ));
                }

                // Update enrolled sections text (course code + section)
                var enrolledDescriptions = _allSlots
                    .Where(s => s.CourseCode != "N/A" && !string.IsNullOrWhiteSpace(s.Section))
                    .Select(s => $"{s.CourseCode} ({s.Section})")
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                EnrolledSectionsText.Text = enrolledDescriptions.Any()
                    ? string.Join(", ", enrolledDescriptions)
                    : "No enrolled courses found for your account.";

                // Build timetable rows directly from the filtered data we received
                BuildTimetableRows(_allSlots);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading timetable: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                EnrolledSectionsText.Text = "Error loading timetable data.";
            }
            finally
            {
                RefreshButton.IsEnabled = true;
                RefreshButton.Content = "🔄 Refresh";
            }
        }

        private string NormalizeDayName(string day)
        {
            if (string.IsNullOrWhiteSpace(day)) return day;
            
            var dayLower = day.Trim().ToLower();
            return dayLower switch
            {
                "mon" or "monday" => "Monday",
                "tue" or "tuesday" => "Tuesday",
                "wed" or "wednesday" => "Wednesday",
                "thu" or "thursday" => "Thursday",
                "fri" or "friday" => "Friday",
                "sat" or "saturday" => "Saturday",
                "sun" or "sunday" => "Sunday",
                _ => day // Return original if not recognized
            };
        }

        private void BuildTimetableRows(IReadOnlyCollection<TimetableSlot> slots)
        {
            _timetableRows.Clear();

            // Extract all unique time slots from the filtered data
            var timeSlots = slots
                .Select(s => s.Time)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            if (!timeSlots.Any())
            {
                return;
            }

            foreach (var time in timeSlots)
            {
                var row = new TimetableRow { TimeSlot = time };

                row.Monday = CreateSlotCard(slots.FirstOrDefault(s => s.Day == "Monday" && s.Time == time));
                row.Tuesday = CreateSlotCard(slots.FirstOrDefault(s => s.Day == "Tuesday" && s.Time == time));
                row.Wednesday = CreateSlotCard(slots.FirstOrDefault(s => s.Day == "Wednesday" && s.Time == time));
                row.Thursday = CreateSlotCard(slots.FirstOrDefault(s => s.Day == "Thursday" && s.Time == time));
                row.Friday = CreateSlotCard(slots.FirstOrDefault(s => s.Day == "Friday" && s.Time == time));

                _timetableRows.Add(row);
            }
        }


        private Border CreateSlotCard(TimetableSlot? slot)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(2)
            };

            if (slot == null)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                border.BorderThickness = new Thickness(1);
                return border;
            }

            border.Background = new SolidColorBrush(Color.FromRgb(66, 129, 164)); // Accent color
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(58, 109, 144));
            border.BorderThickness = new Thickness(1);

            var stackPanel = new StackPanel();

            // Add time display at the top
            var timeText = new TextBlock
            {
                Text = $"🕐 {slot.Time}",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Opacity = 0.9,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var courseCode = new TextBlock
            {
                Text = slot.CourseCode,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 3)
            };

            var courseName = new TextBlock
            {
                Text = slot.CourseName,
                FontSize = 11,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.95,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var details = new TextBlock
            {
                FontSize = 10,
                Foreground = Brushes.White,
                Opacity = 0.85
            };
            details.Inlines.Add(new System.Windows.Documents.Run($"📍 {slot.Room}\n"));
            details.Inlines.Add(new System.Windows.Documents.Run($"👤 {slot.Instructor}"));

            stackPanel.Children.Add(timeText);
            stackPanel.Children.Add(courseCode);
            stackPanel.Children.Add(courseName);
            stackPanel.Children.Add(details);

            border.Child = stackPanel;
            return border;
        }
    }

    public class TimetableRow
    {
        public string TimeSlot { get; set; } = string.Empty;
        public Border? Monday { get; set; }
        public Border? Tuesday { get; set; }
        public Border? Wednesday { get; set; }
        public Border? Thursday { get; set; }
        public Border? Friday { get; set; }
    }
}
