using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FLS
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Window
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
            "Successfully Created Account!",
            "Account Created",
            MessageBoxButton.OK,
            MessageBoxImage.Information
            );

            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        private void AlreadyAccountButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
