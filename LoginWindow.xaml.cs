using System;
using System.Windows;
using ChatClient.Services;

namespace ChatClient
{
    public partial class LoginWindow : Window
    {
        private AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = await _authService.LoginAsync(username, password);

            if (user != null)
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var user = await _authService.SignUpAsync(username, password);

            if (user != null)
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Username already exists.", "Sign Up Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
