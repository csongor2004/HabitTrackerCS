using System.Windows;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Views;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RefreshList();
        }

        private void RefreshList()
        {
            HabitList.ItemsSource = DatabaseService.GetHabits();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddHabitWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                DatabaseService.AddHabit(dialog.HabitName);
                RefreshList();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                DatabaseService.DeleteHabit(selected.Id);
                RefreshList();
            }
        }

        private void HabitList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HabitList.SelectedItem is Habit selected)
            {
                StatusText.Text = $"{selected.Name}\nUtoljára: {selected.LastOccurrence}";
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}