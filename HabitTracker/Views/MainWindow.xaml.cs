using System.Windows;
using HabitTracker.Views;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new MainMenuPage());
        }
    }
}