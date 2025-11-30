using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FLS
{
    public partial class SectionInputDialog : Window
    {
        public string Section { get; private set; } = string.Empty;
        public bool IsSaved { get; private set; } = false;

        private readonly List<string> _availableSections;

        public SectionInputDialog(string courseName, IEnumerable<string>? availableSections = null)
        {
            InitializeComponent();
            CourseNameText.Text = $"Course: {courseName}";
            
            _availableSections = availableSections?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList() ?? new List<string>();

            if (_availableSections.Any())
            {
                SectionComboBox.ItemsSource = _availableSections;
                SectionComboBox.SelectedIndex = 0;
            }

            SectionComboBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Section = SectionComboBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(Section))
            {
                MessageBox.Show("Please select or enter a section.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SectionComboBox.Focus();
                return;
            }

            IsSaved = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SectionComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveButton_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                CancelButton_Click(sender, e);
            }
        }
    }
}

