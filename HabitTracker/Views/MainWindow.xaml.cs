using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Views;

namespace HabitTracker
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private DispatcherTimer _notificationTimer;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new MainMenuPage());
            SetupTrayIcon();
            SetupNotificationEngine();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Information;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Habit Tracker";

            _notifyIcon.DoubleClick += (s, args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Kilépés", null, (s, e) =>
            {
                _notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            });
            _notifyIcon.ContextMenuStrip = contextMenu;
        }


        private void SetupNotificationEngine()
        {
            _notificationTimer = new DispatcherTimer();
            // TESZT: 60 perc helyett 5 másodpercre állítjuk!
            _notificationTimer.Interval = TimeSpan.FromSeconds(5);
            _notificationTimer.Tick += CheckNotifications;
            _notificationTimer.Start();

            // TESZT AZONNALI ÜZENET: Ha ezt látod, a rendszer jól működik!
            _notifyIcon.ShowBalloonTip(3000, "Habit Tracker", "Az értesítési rendszer tökéletesen működik!", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void CheckNotifications(object sender, EventArgs e)
        {
            if (!AppSettings.NotificationsEnabled) return;

            bool alertTriggered = false;

            // 1. Rossz szokások holtpont ellenőrzése
            var badHabits = DatabaseService.GetHabits(HabitType.Bad);
            foreach (var habit in badHabits)
            {
                var logs = DatabaseService.GetLogsForHabit(habit.Id);
                var stats = StatisticsEngine.CalculateStats(logs);
                if (stats.PredictedNextEvent.HasValue)
                {
                    var diff = stats.PredictedNextEvent.Value - DateTime.Now;
                    if (diff.TotalHours > 0 && diff.TotalHours < 12)
                    {
                        _notifyIcon.ShowBalloonTip(5000, "⚠️ Veszélyzóna!", $"A(z) '{habit.Name}' szokásnál az AI szerint 12 órán belül visszaesés várható. Tarts ki!", System.Windows.Forms.ToolTipIcon.Warning);
                        alertTriggered = true;
                        break;
                    }
                }
            }

            if (alertTriggered) return;

            
            var goodHabits = DatabaseService.GetHabits(HabitType.Good);
            foreach (var habit in goodHabits)
            {
                var logs = DatabaseService.GetLogsForHabit(habit.Id);
                if (logs.Count == 0 || logs.First().Timestamp.Date < DateTime.Today)
                {
                    _notifyIcon.ShowBalloonTip(5000, "💡 Emlékeztető", $"Ma még nem teljesítetted a(z) '{habit.Name}' szokásodat! Ne törd meg a sorozatot!", System.Windows.Forms.ToolTipIcon.Info);
                    break;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            if (AppSettings.NotificationsEnabled)
            {
                _notifyIcon.ShowBalloonTip(2000, "Habit Tracker", "Az alkalmazás a háttérben figyeli a statisztikáidat.", System.Windows.Forms.ToolTipIcon.Info);
            }
        }
    }
}