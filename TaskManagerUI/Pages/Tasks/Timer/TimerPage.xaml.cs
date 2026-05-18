using Repository.Models;
using Service.Enums.Task;
using Service.Services;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using TaskManagerUI.Controls.Components;

namespace TaskManagerUI.Pages.Tasks.Timer
{
    public partial class TimerPage : UserControl
    {
        // ============================
        // FIELDS
        // ============================
        private int _taskId = 0;
        private TaskService _taskService = null!;
        private bool _sessionStarted = false;
        private int _totalSecondsAllTime = 0;  // ← seconds not minutes
        private int _totalSecondsToday = 0;  // ← seconds not minutes

        // ============================
        // EVENTS
        // ============================
        public event EventHandler? BackRequested;

        // ============================
        // CONSTRUCTOR
        // ============================
        public TimerPage(int taskId)
        {
            InitializeComponent();
            _taskId = taskId;

            Timer.Started += Timer_Started;
            Timer.Paused += Timer_Paused;
            Timer.Ticked += Timer_Ticked;
            Timer.Completed += Timer_Completed;
            Timer.Stopped += Timer_Stopped;
        }

        private void TimerPage_Loaded(object sender, RoutedEventArgs e)
        {
            // ── Step 1: resolve task service ──────────────────────────
            if (ActiveSession.HasSession &&
                ActiveSession.CurrentTask?.Task.TaskID == _taskId)
            {
                _taskService = ActiveSession.CurrentTask;
                _sessionStarted = true;

                ActiveSession.Ticked -= OnSessionTicked;
                ActiveSession.Ticked += OnSessionTicked;
            }
            else
            {
                var (result, service) = TaskService.Find(_taskId);

                if (result != enTaskRetrieveResult.Found || service is null)
                {
                    MessageBox.Show("Task not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _taskService = service;
            }

            // ── Step 2: load text ─────────────────────────────────────
            _LoadUIText();

            // ── Step 3: load summary ──────────────────────────────────
            _LoadSummary();

            // ── Step 4: init timer ────────────────────────────────────
            _InitTimer();

            // ── disable timer if task is done ─────────────────────────
            if (_taskService.Task.Status == "Done")
            {
                Timer.Visibility = Visibility.Collapsed;
                StatusLabel.Visibility = Visibility.Visible;
                StatusText.Status = "Done";
                AddExtraTimeBtn.Visibility = Visibility.Visible;  // ← ADD
                DoneBtn.Visibility = Visibility.Collapsed;         // ← ADD
            }

            // ── Step 5: restart TimerControl internal timer if running ─
            if (_sessionStarted && ActiveSession.IsRunning)
                Timer.ResumeInternal();  // ← ADD THIS

            // ── Step 6: update progress ───────────────────────────────
            _UpdateProgress();
        }

        private void TimerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ActiveSession.Ticked -= OnSessionTicked;

            // only pause internal timer — session keeps running in background
            if (ActiveSession.IsRunning)
                Timer.PauseInternal();
        }

        // ============================
        // LOAD UI TEXT
        // ============================
        private void _LoadUIText()
        {
            var task = _taskService.Task;

            TaskTitleText.Text = task.Title;
            ProjectTitleText.Text = _taskService.Project?.Title ?? "No Project";
            PriorityText.Status = task.Priority;
            StatusText.Status = task.Status;

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                DescriptionText.Text = task.Description;
                DescriptionPanel.Visibility = Visibility.Visible;
            }

            EstimatedText.Text = _taskService.TotalEstimatedMinutes > 0
                ? _FormatMinutes(_taskService.TotalEstimatedMinutes)
                : "—";

            DueDateText.Text = task.DueDate.HasValue
                ? task.DueDate.Value.ToString("MMM dd")
                : "No date";
        }

        // ============================
        // LOAD SUMMARY
        // ============================
        private void _LoadSummary()
        {
            try
            {
                var timerService = ActiveSession.Timer
                                   ?? new TimerService(_taskId);

                var (secsAllTime, secsToday, sessions) =
                    timerService.GetTaskTimerSummary();

                _totalSecondsAllTime = secsAllTime;
                _totalSecondsToday = secsToday;

                _RenderSessions(sessions);
            }
            catch
            {
                _totalSecondsAllTime = 0;
                _totalSecondsToday = 0;
                _RenderSessions(new List<TimerSession>());
            }
        }

        // ============================
        // INIT TIMER — full seconds accuracy
        // ============================
        private void _InitTimer()
        {
            int est = _taskService.TotalEstimatedMinutes > 0
                ? _taskService.TotalEstimatedMinutes
                : 25;

            int liveSeconds = _sessionStarted &&
                              ActiveSession.HasSession &&
                              ActiveSession.CurrentTask?.Task.TaskID == _taskId
                ? ActiveSession.ElapsedSeconds
                : 0;

            double totalLoggedSecs = _totalSecondsAllTime + liveSeconds;
            double estimatedSecs = est * 60.0;
            double remainingSecs = Math.Max(estimatedSecs - totalLoggedSecs, 0);

            Timer._skipNextReset = true;
            Timer.EstimatedMinutes = est;
            Timer.SetRemaining(TimeSpan.FromSeconds(remainingSecs));

            if (_sessionStarted && ActiveSession.IsRunning)
                Timer.PlayPauseBtn.State = TimerButtonState.Pause;
        }

        // ============================
        // RENDER SESSIONS
        // ============================
        private void _RenderSessions(List<TimerSession> pastSessions)
        {
            SessionsPanel.Children.Clear();
            PastSessionsPanel.Children.Clear();

            var today = DateTime.Today;

            var todaySessions = pastSessions
                .Where(s => s.StartTime.Date == today).ToList();
            var previousSessions = pastSessions
                .Where(s => s.StartTime.Date < today).ToList();

            // ── past (previous days) ──────────────────────────────────
            foreach (var session in previousSessions)
            {
                string duration = session.DurationSeconds.HasValue
                    ? _FormatSeconds(session.DurationSeconds.Value) : "0s";

                PastSessionsPanel.Children.Add(new SessionCard
                {
                    StartTime = session.StartTime.ToString("MMM dd hh:mm tt"),
                    Duration = duration,
                    IsRunning = false,
                    Notes = session.Notes!,
                    Margin = new Thickness(0, 0, 10, 0)
                });
            }

            NoPastSessionsText.Visibility = previousSessions.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            // ── today sessions ────────────────────────────────────────
            foreach (var session in todaySessions)
            {
                string duration = session.DurationSeconds.HasValue
                    ? _FormatSeconds(session.DurationSeconds.Value) : "0s";

                SessionsPanel.Children.Add(new SessionCard
                {
                    StartTime = session.StartTime.ToString("hh:mm tt"),
                    Duration = duration,
                    IsRunning = false,
                    Notes = session.Notes!,
                    Margin = new Thickness(0, 0, 10, 0)
                });
            }

            // ── live card ─────────────────────────────────────────────
            if (_sessionStarted &&
                ActiveSession.HasSession &&
                ActiveSession.CurrentTask?.Task.TaskID == _taskId)
            {
                SessionsPanel.Children.Add(new SessionCard
                {
                    StartTime = ActiveSession.StartedAt?.ToString("hh:mm tt") ?? "—",
                    Duration = _FormatSeconds(ActiveSession.ElapsedSeconds),
                    IsRunning = true,
                    Margin = new Thickness(0, 0, 10, 0)
                });
                NoSessionsText.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoSessionsText.Visibility = todaySessions.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
            }

            _UpdateTotalToday();
        }

        // ============================
        // UPDATE TOTAL TODAY — seconds
        // ============================
        private void _UpdateTotalToday()
        {
            int liveSeconds = _sessionStarted &&
                              ActiveSession.HasSession &&
                              ActiveSession.CurrentTask?.Task.TaskID == _taskId
                ? ActiveSession.ElapsedSeconds
                : 0;

            int totalSeconds = _totalSecondsToday + liveSeconds;
            TotalTodayText.Text = _FormatSeconds(totalSeconds);
        }

        // ============================
        // UPDATE LIVE SESSION CARD — seconds
        // ============================
        private void _UpdateLiveSessionCard()
        {
            if (SessionsPanel.Children.Count == 0) return;

            if (SessionsPanel.Children[^1] is SessionCard last && last.IsRunning)
                last.Duration = _FormatSeconds(ActiveSession.ElapsedSeconds);

            _UpdateTotalToday();
        }

        // ============================
        // UPDATE PROGRESS — seconds accuracy
        // ============================
        private void _UpdateProgress()
        {
            int est = _taskService.TotalEstimatedMinutes;

            if (est == 0)
            {
                ProgressText.Text = "0";
                ProgressBarControl.ProgressWidth = 0;
                return;
            }

            int liveSeconds = _sessionStarted &&
                              ActiveSession.HasSession &&
                              ActiveSession.CurrentTask?.Task.TaskID == _taskId
                ? ActiveSession.ElapsedSeconds
                : 0;

            double totalLogged = _totalSecondsAllTime + liveSeconds;
            double estimatedSecs = est * 60.0;
            double progress = Math.Min(totalLogged / estimatedSecs * 100, 100);

            ProgressText.Text = progress.ToString("F2");
            ProgressBarControl.ProgressWidth = progress;

            _UpdateTotalToday();
        }

        // ============================
        // ACTIVE SESSION TICKED — MASTER TICK
        // ============================
        private void OnSessionTicked()
        {
            Dispatcher.Invoke(() =>
            {
                if (!ActiveSession.HasSession) return;

                int est = _taskService.TotalEstimatedMinutes > 0
                    ? _taskService.TotalEstimatedMinutes
                    : 25;

                int liveSeconds = ActiveSession.ElapsedSeconds;
                double totalLoggedSecs = _totalSecondsAllTime + liveSeconds;
                double estimatedSecs = est * 60.0;
                double remainingSecs = Math.Max(estimatedSecs - totalLoggedSecs, 0);

                Timer.ForceSetRemaining(TimeSpan.FromSeconds(remainingSecs));

                _UpdateLiveSessionCard();
                _UpdateProgress();
            });
        }

        // ============================
        // TIMER — STARTED (▶ Play clicked)
        // ============================
        private void Timer_Started(object? sender, EventArgs e)
        {
            if (ActiveSession.CurrentTask?.Task.Status == "Done")
            {
                MessageBox.Show("Already Done Start a New Task.", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // ── if session exists but paused — just resume ────────────
            if (_sessionStarted && ActiveSession.HasSession &&
                ActiveSession.CurrentTask?.Task.TaskID == _taskId)
            {
                ActiveSession.Resume();
                return;
            }

            // ── brand new session ─────────────────────────────────────
            if (_sessionStarted) return;

            bool started = ActiveSession.Start(_taskId);

            if (!started)
            {
                MessageBox.Show("Failed to start session.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Timer.StopExternal();
                return;
            }

            _sessionStarted = true;
            ActiveSession.Ticked -= OnSessionTicked;
            ActiveSession.Ticked += OnSessionTicked;

            _taskService = ActiveSession.CurrentTask!;
            _LoadUIText();

            _LoadSummary();
            _UpdateProgress();
        }

        // ============================
        // TIMER — PAUSED
        // ============================
        private void Timer_Paused(object? sender, EventArgs e)
        {
            // pause the background session tick too
            ActiveSession.Pause();
        }

        // ============================
        // TIMER — TICKED
        // ============================
        private void Timer_Ticked(object? sender, TimeSpan remaining)
        {
            // progress handled by OnSessionTicked (master tick)
        }

        // ============================
        // TIMER — COMPLETED
        // ============================
        private void Timer_Completed(object? sender, EventArgs e)
        {
            ActiveSession.Ticked -= OnSessionTicked;

            if (ActiveSession.HasSession)
            {
                int estimatedSeconds = _taskService.TotalEstimatedMinutes * 60;
                int alreadyLogged = _totalSecondsAllTime;
                int exactSeconds = Math.Max(estimatedSeconds - alreadyLogged, 0);

                ActiveSession.Stop(exactSeconds);
            }

            _sessionStarted = false;

            var delayTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)  // ← give animations time to finish
            };
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();

                _LoadSummary();
                _InitTimer();
                _UpdateProgress();
                _ShowCompleteDialog();
            };
            delayTimer.Start();
        }

        // ============================
        // SHOW COMPLETE DIALOG
        // ============================
        private void _ShowCompleteDialog()
        {
            var dialog = new Dialog.TimerCompleteDialog(_taskService.Title);
            dialog.Owner = Window.GetWindow(this);  // ← attach to parent window
            dialog.ShowDialog();

            switch (dialog.Result)
            {
                case Dialog.TimerCompleteDialog.enDialogResult.MarkDone:
                    _MarkTaskDone();
                    break;

                case Dialog.TimerCompleteDialog.enDialogResult.AddTime:
                    _AddExtraTime(dialog.ExtraMinutes);
                    break;

                case Dialog.TimerCompleteDialog.enDialogResult.Cancel:
                    break;
            }
        }

        // ============================
        // MARK TASK DONE
        // ============================
        private void _MarkTaskDone()
        {
            var result = TaskService.Complete(_taskId);

            if (result == enTaskCompleteResult.Completed)
            {
                var (isFound, service) = TaskService.Find(_taskId);
                if (isFound != enTaskRetrieveResult.Found) return;

                _taskService = service!;
                _LoadUIText();

                Timer.Visibility = Visibility.Collapsed;
                StatusLabel.Visibility = Visibility.Visible;
                AddExtraTimeBtn.Visibility = Visibility.Visible;   // ← show button
                DoneBtn.Visibility = Visibility.Collapsed;         // ← hide done button
            }
        }

        // ============================
        // ADD EXTRA TIME
        // ============================
        private void _AddExtraTime(int minutes)
        {
            bool saved = TaskService.AddExtraMinutes(_taskId, minutes);

            if (!saved)
            {
                MessageBox.Show("Failed to save extra time.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ← reopen task back to InProgress
            TaskService.Reopen(_taskId);

            // reload task to get updated TotalEstimatedMinutes and InProgress status
            var (isFound, service) = TaskService.Find(_taskId);
            if (isFound != enTaskRetrieveResult.Found) return;

            _taskService = service!;
            _LoadUIText();
            _LoadSummary();
            _InitTimer();
            _UpdateProgress();

            // ← show timer again, hide extra time button, show done button
            Timer.Visibility = Visibility.Visible;
            StatusLabel.Visibility = Visibility.Collapsed;
            AddExtraTimeBtn.Visibility = Visibility.Collapsed;
            DoneBtn.Visibility = Visibility.Visible;

            // ← reset completed state so Play button works again
            Timer.ResetFromExternal();
        }

        // ============================
        // TIMER — STOPPED
        // ============================
        private void Timer_Stopped(object? sender, EventArgs e)
        {
            ActiveSession.Ticked -= OnSessionTicked;

            if (ActiveSession.HasSession)
                ActiveSession.Stop();      // ← closes DB row, updates _totalMinutesAllTime

            _sessionStarted = false;

            _LoadSummary();                // ← reloads from DB with just-ended session ✅
            _InitTimer();                  // ← sets timer to where you stopped ✅
            _UpdateProgress();             // ← progress stays at correct % ✅
        }

        // ============================
        // BACK BUTTON
        // Unloaded handles cleanup automatically
        // ============================
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        // ============================
        // HELPERS
        // ============================
        private string _FormatSeconds(int seconds)
        {
            if (seconds <= 0) return "0 min";

            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;

            if (h > 0)
                return s == 0 ? $"{h}h {m}m" : $"{h}h {m}m {s}s";
            if (m > 0)
                return s == 0 ? $"{m} min" : $"{m}m {s}s";

            return $"{s}s";
        }

        private string _FormatMinutes(int minutes)
        {
            if (minutes <= 0) return "0 min";
            if (minutes < 60) return $"{minutes} min";

            int h = minutes / 60;
            int m = minutes % 60;

            return m == 0 ? $"{h}h" : $"{h}h {m}m";
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            var (isFound, service) = TaskService.Find(_taskId);
            if (isFound != enTaskRetrieveResult.Found) return;

            if (service!.Status == "Done")
            {
                MessageBox.Show($"Task \"{service.Title}\" is already completed.",
                    "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _MarkTaskDone();
        }

        private void AddExtraTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialog.TimerCompleteDialog(_taskService.Title);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.Result == Dialog.TimerCompleteDialog.enDialogResult.AddTime)
                _AddExtraTime(dialog.ExtraMinutes);
        }


    }
}