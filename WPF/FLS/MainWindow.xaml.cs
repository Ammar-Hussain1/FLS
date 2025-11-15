using System.Windows;

namespace WpfLoginApp
{
    public partial class MainWindow : Window
    {
        private readonly string correctUsername = "admin";
        private readonly string correctPassword = "1234";

        public MainWindow()
        {
            InitializeComponent();   // <-- This MUST exist
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (username == correctUsername && password == correctPassword)
            {
                MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                lblMessage.Text = "Invalid username or password!";
            }
        }
    }
}
