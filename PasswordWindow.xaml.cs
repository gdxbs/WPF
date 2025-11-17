using System.Windows;

namespace ChatClient
{
    public partial class PasswordWindow : Window
    {
        public string Password => PasswordBox.Password;

        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
