using System.Windows;
using HabitTracker.Services; 

namespace HabitTracker
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            DatabaseService.InitializeDatabase();
        }
    }
}