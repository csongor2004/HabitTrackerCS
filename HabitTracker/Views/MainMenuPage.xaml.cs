using System.Windows;
using System.Windows.Controls;
using HabitTracker.Models;

namespace HabitTracker.Views
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void BadHabits_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HabitListPage(HabitType.Bad));
        }

        private void GoodHabits_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HabitListPage(HabitType.Good));
        }
    }
}