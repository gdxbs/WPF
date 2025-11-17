using System.Windows;

namespace ChatClient
{
    public partial class AdminLoginWindow : Window
    {
        public AdminLoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // For simplicity, using a hardcoded password.
            // In a real application, this should be handled securely.
            if (PasswordBox.Password == "admin")
            {
                var adminWindow = new AdminWindow();
                adminWindow.Owner = this.Owner;
                this.DialogResult = true;
                adminWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
