using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TaskManagerUI
{
    /// <summary>
    /// Interaction logic for TestColorPicker.xaml
    /// </summary>
    public partial class TestColorPicker : Window
    {

        public static class ActiveSession
        {
            public static TaskItem? CurrentTask { get; set; }
            public static bool IsRunning { get; set; }
            public static DateTime? StartedAt { get; set; }

            public static event Action? SessionChanged;

            public static void Start(TaskItem task)
            {
                CurrentTask = task;
                IsRunning = true;
                StartedAt = DateTime.Now;
                SessionChanged?.Invoke();
            }

            public static void Pause()
            {
                IsRunning = false;
                SessionChanged?.Invoke();
            }

            public static void Stop()
            {
                CurrentTask = null;
                IsRunning = false;
                StartedAt = null;
                SessionChanged?.Invoke();
            }

            public static void Resume()
            {
                if (CurrentTask == null) return; // Nothing to resume
                IsRunning = true;
                SessionChanged?.Invoke();
            }
        }

        public class TaskItem
        {
            public string Title { get; set; } = null!;
            public string Category { get; set; } = null!;
            public DateTime DueDate { get; set; }
            public double TotalMinutes { get; set; }
        }

        // ── Fields ────────────────────────────────────────────────────────────

        private DispatcherTimer _timer;
        private TimeSpan _elapsed;

        // ── Constructor ───────────────────────────────────────────────────────

        public TestColorPicker()
        {
            InitializeComponent();

            // Setup timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;

            // Listen to session changes
            ActiveSession.SessionChanged += OnSessionChanged;

            // Sync UI on startup
            UpdateCard();

            // ── Test Data ─────────────────────────────────────────
            var task = new TaskItem
            {
                Title = "Write Unit Tests",
                Category = "Development",
                DueDate = DateTime.Now.AddDays(3),
                TotalMinutes = 1
            };
            ActiveSession.Start(task);
            // ──────────────────────────────────────────────────────
        }

        // ── Session Changed ───────────────────────────────────────────────────

        private void OnSessionChanged()
        {
            Dispatcher.Invoke(UpdateCard);
        }

        private void UpdateCard()
        {
            if (ActiveSession.CurrentTask == null)
            {
                _timer.Stop();
                _elapsed = TimeSpan.Zero;
                SessionCard.ElapsedTime = "00:00:00";
                SessionCard.Progress = 0;
                return;
            }

            var task = ActiveSession.CurrentTask;

            SessionCard.Visibility = Visibility.Visible;
            SessionCard.TaskTitle = task.Title;
            SessionCard.Subtitle = $"{task.Category}  •  Due {task.DueDate:MMM dd}";
            SessionCard.IsRunning = ActiveSession.IsRunning;

            if (ActiveSession.IsRunning)
                _timer.Start();
            else
                _timer.Stop();
        }

        // ── Timer Tick ────────────────────────────────────────────────────────

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (ActiveSession.CurrentTask == null) return;

            // ── Step 1: Add 1 second ──────────────────────────────────
            _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));

            // ── Step 2: Update elapsed time display ───────────────────
            SessionCard.ElapsedTime = _elapsed.ToString(@"hh\:mm\:ss");

            // ── Step 3: Calculate and update progress ─────────────────
            double totalSeconds = ActiveSession.CurrentTask.TotalMinutes * 60;
            double progress = (_elapsed.TotalSeconds / totalSeconds) * 100;
            double clampedProgress = Math.Min(progress, 100);

            SessionCard.Progress = clampedProgress;
            SessionCard.ProgressText = $"{(int)clampedProgress}%"; // ← Update text

            // ── Step 4: Check completion ──────────────────────────────
            if (_elapsed.TotalSeconds >= totalSeconds)
            {
                OnSessionCompleted();
            }
        }

        private void OnSessionCompleted()
        {
            string completedAt = _elapsed.ToString(@"hh\:mm\:ss");

            _timer.Stop();

            ActiveSession.SessionChanged -= OnSessionChanged;
            ActiveSession.Stop();
            ActiveSession.SessionChanged += OnSessionChanged;

            SessionCard.ElapsedTime = completedAt;
            SessionCard.Progress = 100;
            SessionCard.ProgressText = "100%";          // ← Final text
            SessionCard.SessionStatus = "Completed";
            SessionCard.IsRunning = false;

            SessionCard.PauseBtn.Visibility = Visibility.Collapsed;
            SessionCard.StopBtn.Visibility = Visibility.Collapsed;
            SessionCard.RunningDot.Visibility = Visibility.Collapsed;

            _elapsed = TimeSpan.Zero;
        }

        // ── Button Handlers ───────────────────────────────────────────────────

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            // ✅ Now uses ActiveSession directly, no Resume() missing
            if (ActiveSession.IsRunning)
            {
                ActiveSession.Pause();
                SessionCard.SessionStatus = "Pause";
            }

            else
            {
                if (ActiveSession.CurrentTask != null)
                {
                    ActiveSession.Resume();
                    SessionCard.SessionStatus = "InProgress";
                }

            }

        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            // Stop → Show Cancelled, keep card visible
            ActiveSession.Stop();
            SessionCard.SessionStatus = "Cancelled";
            SessionCard.IsRunning = false;
            _timer.Stop();
            _elapsed = TimeSpan.Zero;
            // ✅ Card stays visible showing Cancelled status
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            // Close → Just hide the card
            SessionCard.Visibility = Visibility.Collapsed;
            // ✅ Card hidden, no status change
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        protected override void OnClosed(EventArgs e)
        {
            ActiveSession.SessionChanged -= OnSessionChanged;
            _timer.Stop();
            base.OnClosed(e);
        }


    }
}
