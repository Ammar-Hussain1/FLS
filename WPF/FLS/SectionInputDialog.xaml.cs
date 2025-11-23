using System.Windows;
using System.Windows.Input;

namespace FLS
{
    public partial class SectionInputDialog : Window
    {
        public string Section { get; private set; } = string.Empty;
        public bool IsSaved { get; private set; } = false;

        public SectionInputDialog(string courseName)
        {
            InitializeComponent();
            CourseNameText.Text = $"Course: {courseName}";
            SectionTextBox.Text = string.Empty;
            SectionTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Section = SectionTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(Section))
            {
                MessageBox.Show("Please enter a section number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SectionTextBox.Focus();
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

        private void SectionTextBox_KeyDown(object sender, KeyEventArgs e)
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

