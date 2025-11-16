using System;
using System.Windows;
using System.Windows.Forms;
using ChatClient.Models;
using ChatClient.Services;
using ChatClient.Themes;

namespace ChatClient
{
    public partial class SettingsWindow : Window
    {
        private PreferencesService _preferencesService;
        private UserPreferences _currentPreferences;

        public SettingsWindow()
        {
            InitializeComponent();
            _preferencesService = new PreferencesService();
            LoadSettings();
        }

        private async void LoadSettings()
        {
            try
            {
                _currentPreferences = await Task.Run(() => _preferencesService.LoadPreferencesAsync());

                LightThemeRadio.IsChecked = _currentPreferences.ThemeMode == "light";
                DarkThemeRadio.IsChecked = _currentPreferences.ThemeMode == "dark";

                FontSizeSlider.Value = _currentPreferences.FontSize;
                FontSizeValue.Text = $"{_currentPreferences.FontSize}pt";

                SoundEnabledCheckBox.IsChecked = _currentPreferences.SoundEnabled;
                VolumeSlider.Value = _currentPreferences.NotificationVolume;
                VolumeValue.Text = $"{_currentPreferences.NotificationVolume}%";

                MessageNotifyCheckBox.IsChecked = _currentPreferences.MessageNotificationEnabled;
                JoinLeaveNotifyCheckBox.IsChecked = _currentPreferences.JoinLeaveNotificationEnabled;
                ErrorNotifyCheckBox.IsChecked = _currentPreferences.ErrorNotificationEnabled;

                AutoConnectCheckBox.IsChecked = _currentPreferences.AutoConnectEnabled;
                SavedHostTextBox.Text = _currentPreferences.LastConnectionHost;
                SavedPortTextBox.Text = _currentPreferences.LastConnectionPort.ToString();

                BackgroundImagePathTextBox.Text = _currentPreferences.BackgroundImagePath ?? "";
                TransparencySlider.Value = _currentPreferences.WindowTransparency;

                FontSizeSlider.ValueChanged += FontSizeSlider_ValueChanged;
                VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FontSizeSlider_ValueChanged(object sender, System.Windows.Controls.Primitives.RangeBaseEventArgs e)
        {
            FontSizeValue.Text = $"{(int)e.NewValue}pt";
        }

        private void VolumeSlider_ValueChanged(object sender, System.Windows.Controls.Primitives.RangeBaseEventArgs e)
        {
            VolumeValue.Text = $"{(int)e.NewValue}%";
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            string theme = DarkThemeRadio.IsChecked == true ? "dark" : "light";
            ThemeManager.ApplyTheme(theme);
        }

        private void AccentColor_Blue(object sender, RoutedEventArgs e) => SetAccentColor("#0078D4");
        private void AccentColor_Green(object sender, RoutedEventArgs e) => SetAccentColor("#10B981");
        private void AccentColor_Orange(object sender, RoutedEventArgs e) => SetAccentColor("#F59E0B");
        private void AccentColor_Red(object sender, RoutedEventArgs e) => SetAccentColor("#EF4444");

        private void SetAccentColor(string color)
        {
            ThemeManager.ApplyAccentColor(color);
        }

        private void BrowseBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                BackgroundImagePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentPreferences.ThemeMode = DarkThemeRadio.IsChecked == true ? "dark" : "light";
                _currentPreferences.FontSize = (int)FontSizeSlider.Value;
                _currentPreferences.SoundEnabled = SoundEnabledCheckBox.IsChecked == true;
                _currentPreferences.NotificationVolume = (int)VolumeSlider.Value;
                _currentPreferences.MessageNotificationEnabled = MessageNotifyCheckBox.IsChecked == true;
                _currentPreferences.JoinLeaveNotificationEnabled = JoinLeaveNotifyCheckBox.IsChecked == true;
                _currentPreferences.ErrorNotificationEnabled = ErrorNotifyCheckBox.IsChecked == true;
                _currentPreferences.AutoConnectEnabled = AutoConnectCheckBox.IsChecked == true;
                _currentPreferences.LastConnectionHost = SavedHostTextBox.Text;

                if (int.TryParse(SavedPortTextBox.Text, out int port))
                {
                    _currentPreferences.LastConnectionPort = port;
                }

                _currentPreferences.BackgroundImagePath = BackgroundImagePathTextBox.Text;
                _currentPreferences.WindowTransparency = (int)TransparencySlider.Value;

                await _preferencesService.SavePreferencesAsync(_currentPreferences);

                System.Windows.MessageBox.Show("Settings saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
