using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FLS;

namespace WpfLoginApp
{
  
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e) {
            SignUpPage signUpPage = new SignUpPage();
            signUpPage.Show();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) 
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e) { }

       
    }
}
