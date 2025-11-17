using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ChatClient.Themes
{
    public static class ThemeManager
    {
        private static string _currentTheme = "light";
        private static string _currentAccentColor = "#0078D4";

        public static void ApplyTheme(string themeName)
        {
            _currentTheme = themeName;

            var resourceDictionary = new ResourceDictionary();
            string uri = themeName == "dark" ?
                "pack://application:,,,/Themes/DarkTheme.xaml" :
                "pack://application:,,,/Themes/LightTheme.xaml";

            resourceDictionary.Source = new Uri(uri);

            Application.Current.Resources.MergedDictionaries.RemoveAt(
                Application.Current.Resources.MergedDictionaries.Count - 1);

            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public static void ApplyAccentColor(string colorHex)
        {
            _currentAccentColor = colorHex;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                var accentBrush = new SolidColorBrush(color);

                Application.Current.Resources["AccentBrush"] = accentBrush;
            }
            catch
            {
                Application.Current.Resources["AccentBrush"] = new SolidColorBrush(Colors.CornflowerBlue);
            }
        }

        public static string GetCurrentTheme() => _currentTheme;
        public static string GetCurrentAccentColor() => _currentAccentColor;
    }
}
