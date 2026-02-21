using System.Windows;

namespace HabitTracker.Views
{
    public partial class AddHabitWindow : Window
    {
        public string HabitName { get; private set; }

        public AddHabitWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(HabitNameInput.Text))
            {
                HabitName = HabitNameInput.Text;
                DialogResult = true;
            }
        }
    }
}