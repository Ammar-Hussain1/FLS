using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FLS.Models;

namespace FLS
{
    public partial class TimetableView : UserControl
    {
        private ObservableCollection<TimetableRow> _timetableRows;
        private List<TimetableSlot> _allSlots;

        public TimetableView()
        {
            InitializeComponent();
            _timetableRows = new ObservableCollection<TimetableRow>();
            TimetableGrid.ItemsSource = _timetableRows;
            LoadDummyData();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDummyData();
            MessageBox.Show("Timetable refreshed successfully!", "Refresh", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadDummyData()
        {
            _timetableRows.Clear();
            _allSlots = new List<TimetableSlot>();

            _allSlots.Add(new TimetableSlot("Monday", "08:30-10:00", "CS101", "Programming Fundamentals", "A", "Room 301", "Dr. Ahmed"));
            _allSlots.Add(new TimetableSlot("Monday", "10:30-12:00", "MATH201", "Calculus II", "B", "Room 205", "Dr. Sara"));
            _allSlots.Add(new TimetableSlot("Monday", "14:00-15:30", "ENG102", "English Composition", "C", "Room 110", "Ms. Fatima"));

            _allSlots.Add(new TimetableSlot("Tuesday", "08:30-10:00", "CS102", "Data Structures", "A", "Room 302", "Dr. Khan"));
            _allSlots.Add(new TimetableSlot("Tuesday", "12:30-14:00", "PHY101", "Physics I", "B", "Lab 201", "Dr. Ali"));

            _allSlots.Add(new TimetableSlot("Wednesday", "08:30-10:00", "CS101", "Programming Fundamentals", "A", "Room 301", "Dr. Ahmed"));
            _allSlots.Add(new TimetableSlot("Wednesday", "10:30-12:00", "MATH201", "Calculus II", "B", "Room 205", "Dr. Sara"));
            _allSlots.Add(new TimetableSlot("Wednesday", "15:30-17:00", "CS102", "Data Structures", "A", "Lab 305", "Dr. Khan"));

            _allSlots.Add(new TimetableSlot("Thursday", "10:30-12:00", "PHY101", "Physics I", "B", "Room 203", "Dr. Ali"));
            _allSlots.Add(new TimetableSlot("Thursday", "14:00-15:30", "ENG102", "English Composition", "C", "Room 110", "Ms. Fatima"));

            _allSlots.Add(new TimetableSlot("Friday", "08:30-10:00", "CS102", "Data Structures", "A", "Room 302", "Dr. Khan"));
            _allSlots.Add(new TimetableSlot("Friday", "10:30-12:00", "MATH201", "Calculus II", "B", "Room 205", "Dr. Sara"));

            var uniqueCourses = _allSlots.Select(s => $"{s.CourseCode} ({s.Section})").Distinct().ToList();
            EnrolledSectionsText.Text = string.Join(", ", uniqueCourses);

            var timeSlots = new List<string>
            {
                "08:30-10:00",
                "10:30-12:00",
                "12:30-14:00",
                "14:00-15:30",
                "15:30-17:00"
            };

            foreach (var time in timeSlots)
            {
                var row = new TimetableRow { TimeSlot = time };

                row.Monday = CreateSlotCard(_allSlots.FirstOrDefault(s => s.Day == "Monday" && s.Time == time));
                row.Tuesday = CreateSlotCard(_allSlots.FirstOrDefault(s => s.Day == "Tuesday" && s.Time == time));
                row.Wednesday = CreateSlotCard(_allSlots.FirstOrDefault(s => s.Day == "Wednesday" && s.Time == time));
                row.Thursday = CreateSlotCard(_allSlots.FirstOrDefault(s => s.Day == "Thursday" && s.Time == time));
                row.Friday = CreateSlotCard(_allSlots.FirstOrDefault(s => s.Day == "Friday" && s.Time == time));

                _timetableRows.Add(row);
            }
        }

        private Border CreateSlotCard(TimetableSlot slot)
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

            stackPanel.Children.Add(courseCode);
            stackPanel.Children.Add(courseName);
            stackPanel.Children.Add(details);

            border.Child = stackPanel;
            return border;
        }
    }

    public class TimetableRow
    {
        public string TimeSlot { get; set; }
        public Border Monday { get; set; }
        public Border Tuesday { get; set; }
        public Border Wednesday { get; set; }
        public Border Thursday { get; set; }
        public Border Friday { get; set; }
    }
}
