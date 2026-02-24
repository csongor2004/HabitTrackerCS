using HabitTracker.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HabitTracker.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            NotificationsCheckBox.IsChecked = AppSettings.NotificationsEnabled;
            DarkModeCheckBox.IsChecked = AppSettings.IsDarkMode;
            MotivationCheckBox.IsChecked = AppSettings.ShowMotivation;
        }

        private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        private void Setting_Changed(object sender, RoutedEventArgs e)
        {
            AppSettings.NotificationsEnabled = NotificationsCheckBox.IsChecked ?? true;
        }
        private void Motivation_Changed(object sender, RoutedEventArgs e)
        {
            AppSettings.ShowMotivation = MotivationCheckBox.IsChecked ?? true;
        }
        private void DarkMode_Changed(object sender, RoutedEventArgs e)
        {
            AppSettings.IsDarkMode = DarkModeCheckBox.IsChecked ?? false;

            
            var res = System.Windows.Application.Current.Resources;

            if (AppSettings.IsDarkMode)
            {
                
                res["AppBg"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 18, 18));
                res["CardBg"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                res["PrimaryText"] = new SolidColorBrush(System.Windows.Media.Colors.White);
                res["SecondaryText"] = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                res["BorderColor"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 50));
                res[System.Windows.SystemColors.WindowBrushKey] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
            }
            else
            {
                
                res["AppBg"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 242, 245));
                res["CardBg"] = new SolidColorBrush(System.Windows.Media.Colors.White);
                res["PrimaryText"] = new SolidColorBrush(System.Windows.Media.Colors.Black);
                res["SecondaryText"] = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                res["BorderColor"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
                res[System.Windows.SystemColors.WindowBrushKey] = new SolidColorBrush(System.Windows.Media.Colors.White);
            }

            
            System.Windows.Application.Current.MainWindow.Background = (SolidColorBrush)res["AppBg"];
        }
    }
}