using System.Windows;
using HabitTracker.Models;
using HabitTracker.Views;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BadHabits_Click(object sender, RoutedEventArgs e)
        {
            var window = new HabitListWindow(HabitType.Bad);
            window.ShowDialog();
        }

        private void GoodHabits_Click(object sender, RoutedEventArgs e)
        {
            var window = new HabitListWindow(HabitType.Good);
            window.ShowDialog();
        }
    }
}