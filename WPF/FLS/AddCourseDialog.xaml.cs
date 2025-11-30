using System.Windows;

namespace FLS
{
    public partial class AddCourseDialog : Window
    {
        public string CourseCode { get; private set; }
        public string CourseName { get; private set; }
        public int CreditHours { get; private set; }
        public string Instructor { get; private set; }

        public AddCourseDialog()
        {
            InitializeComponent();

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CourseCodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(CourseNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(InstructorTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(CreditHoursTextBox.Text, out int credits) || credits <= 0)
            {
                MessageBox.Show("Please enter a valid number for credit hours.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CourseCode = CourseCodeTextBox.Text.Trim();
            CourseName = CourseNameTextBox.Text.Trim();
            CreditHours = credits;
            Instructor = InstructorTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
