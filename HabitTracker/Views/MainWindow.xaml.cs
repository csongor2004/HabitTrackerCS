using System;
using System.Windows;
using HabitTracker.Views;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new MainMenuPage());
            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Information; // Alapértelmezett Windows ikon
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Habit Tracker - Fut a háttérben";

            // Dupla kattintásra visszahozza az ablakot
            _notifyIcon.DoubleClick += (s, args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            // Jobb klikkes menü a kilépéshez
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Kilépés", null, (s, e) =>
            {
                _notifyIcon.Dispose();
                Application.Current.Shutdown();
            });
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        // Amikor a felhasználó rányom az X-re, nem zárjuk be, csak elrejtjük
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            _notifyIcon.ShowBalloonTip(2000, "Habit Tracker", "Az alkalmazás a háttérben fut tovább.", System.Windows.Forms.ToolTipIcon.Info);
        }
    }
}