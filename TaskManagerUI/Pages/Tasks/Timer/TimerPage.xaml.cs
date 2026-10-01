using Repository.Models;
using Service.Enums.Task;
using Service.Services;
using System.Windows;
using System.Windows.Controls;
using TaskManagerUI.Controls.Components;

namespace TaskManagerUI.Pages.Tasks.Timer
{
    public partial class TimerPage : UserControl
    {
        private int _taskId = 0;
        private TaskService _taskService = null!;
        private bool _sessionStarted = false;
        private int _totalSecondsAllTime = 0;
        private int _totalSecondsToday = 0;

        public event EventHandler? BackRequested;

        public TimerPage(int taskId)
        {
            InitializeComponent();
            _taskId = taskId;

            Timer.PlayRequested += Timer_PlayRequested;
            Timer.PauseRequested += Timer_PauseRequested;
            Timer.StopRequested += Timer_StopRequested;
        }

        private void TimerPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ActiveSession.HasSession && ActiveSession.CurrentTask?.Task.TaskID == _taskId)
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
                    MessageBox.Show("Task not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _taskService = service;
            }

            _LoadUIText();
            _LoadSummary();
            _InitTimer();

            if (_taskService.Task.Status == "Done")
            {
                Timer.Visibility = Visibility.Collapsed;
                StatusLabel.Visibility = Visibility.Visible;
                StatusText.Status = "Done";
                AddExtraTimeBtn.Visibility = Visibility.Visible;
                DoneBtn.Visibility = Visibility.Collapsed;
            }

            _UpdateProgress();
        }

        private void TimerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // ActiveSession + GlobalTimer keep running in the background regardless.
            // This page's TimerControl instance is being torn down along with the page,
            // so there is nothing to pause here — a fresh TimerControl is created next
            // time this task's TimerPage is opened, and TimerPage_Loaded re-syncs it.
            ActiveSession.Ticked -= OnSessionTicked;
        }

        // ============================
        // TIMER CONTROL EVENTS — this page is the only decision-maker
        // ============================
        private void Timer_PlayRequested(object? sender, EventArgs e)
        {
            if (_taskService.Task.Status == "Done")
            {
                MessageBox.Show("This task is already done.", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // resume an existing paused session
            if (_sessionStarted && ActiveSession.HasSession &&
                ActiveSession.CurrentTask?.Task.TaskID == _taskId)
            {
                ActiveSession.Resume();
                Timer.SetRunning(true);
                return;
            }

            if (_sessionStarted) return;

            bool started = ActiveSession.Start(_taskId);
            if (!started)
            {
                MessageBox.Show("Failed to start session.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Timer.SetIdle();
                return;
            }

            _sessionStarted = true;
            ActiveSession.Ticked -= OnSessionTicked;
            ActiveSession.Ticked += OnSessionTicked;

            _taskService = ActiveSession.CurrentTask!;
            _LoadUIText();
            _LoadSummary();
            _UpdateProgress();

            Timer.SetRunning(true);
        }

        private void Timer_PauseRequested(object? sender, EventArgs e)
        {
            ActiveSession.Pause();
            Timer.SetRunning(false);
        }

        private void Timer_StopRequested(object? sender, EventArgs e)
        {
            ActiveSession.Ticked -= OnSessionTicked;

            if (ActiveSession.HasSession)
                ActiveSession.Stop(); // always saves the real measured elapsed seconds

            _sessionStarted = false;

            _LoadSummary();
            _InitTimer();
            _UpdateProgress();

            Timer.SetIdle();
        }

        // ============================
        // MASTER TICK — the only place remaining time / completion is decided
        // ============================
        private void OnSessionTicked()
        {
            Dispatcher.Invoke(() =>
            {
                if (!ActiveSession.HasSession) return;

                int est = _taskService.TotalEstimatedMinutes > 0 ? _taskService.TotalEstimatedMinutes : 25;
                int liveSeconds = ActiveSession.ElapsedSeconds;
                double totalLoggedSecs = _totalSecondsAllTime + liveSeconds;
                double estimatedSecs = est * 60.0;
                double remainingSecs = estimatedSecs - totalLoggedSecs;

                if (remainingSecs <= 0)
                {
                    Timer.SetRemaining(TimeSpan.Zero);
                    _CompleteSession();
                    return;
                }

                Timer.SetRemaining(TimeSpan.FromSeconds(remainingSecs));
                _UpdateLiveSessionCard();
                _UpdateProgress();
            });
        }

        // ============================
        // COMPLETION — single path for "time ran out" (Trigger A)
        // ============================
        private void _CompleteSession()
        {
            ActiveSession.Ticked -= OnSessionTicked;

            if (ActiveSession.HasSession)
                ActiveSession.Stop(); // real elapsed seconds, never force-corrected

            _sessionStarted = false;
            Timer.SetCompleted();

            var delayTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800) // let the completion animation finish
            };
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();
                _LoadSummary();
                _InitTimer();
                _UpdateProgress();
                _ShowTimeUpDialog();
            };
            delayTimer.Start();
        }

        // Trigger A dialog: time's up, still "live" — Mark as Done / Add Extra Time
        private void _ShowTimeUpDialog()
        {
            var dialog = new Dialog.TimerCompleteDialog(_taskService.Title);
            dialog.Owner = Window.GetWindow(this);
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
        // COMPLETION — Trigger B: user clicks "Mark as Done" early, time still left
        // ============================
        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_taskService.Task.Status == "Done")
            {
                MessageBox.Show($"Task \"{_taskService.Title}\" is already completed.",
                    "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Mark \"{_taskService.Title}\" as done now, with {_FormatSeconds(_totalSecondsAllTime + _LiveSeconds())} logged?",
                "Mark as Done", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            // stop any in-progress session first — this was previously missing,
            // and left the session running in the background even after Done.
            ActiveSession.Ticked -= OnSessionTicked;
            if (ActiveSession.HasSession && ActiveSession.CurrentTask?.Task.TaskID == _taskId)
                ActiveSession.Stop();

            _sessionStarted = false;
            Timer.SetIdle();

            _MarkTaskDone();
        }

        private void _MarkTaskDone()
        {
            var result = TaskService.Complete(_taskId);
            if (result != enTaskCompleteResult.Completed) return;

            var (isFound, service) = TaskService.Find(_taskId);
            if (isFound != enTaskRetrieveResult.Found) return;

            _taskService = service!;
            _LoadUIText();
            _LoadSummary();
            _UpdateProgress();

            Timer.Visibility = Visibility.Collapsed;
            StatusLabel.Visibility = Visibility.Visible;
            AddExtraTimeBtn.Visibility = Visibility.Visible;
            DoneBtn.Visibility = Visibility.Collapsed;
        }

        // ============================
        // ADD EXTRA TIME — from Done state, minutes-only dialog, no Mark-as-Done option
        // ============================
        private void AddExtraTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialog.AddExtraTimeDialog(_taskService.Title);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.Confirmed)
                _AddExtraTime(dialog.ExtraMinutes);
        }

        private void _AddExtraTime(int minutes)
        {
            bool saved = TaskService.AddExtraMinutes(_taskId, minutes);
            if (!saved)
            {
                MessageBox.Show("Failed to save extra time.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool isReopen = TaskService.Reopen(_taskId);

            if (!isReopen)
            {
                MessageBox.Show("Something went wrong try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var (isFound, service) = TaskService.Find(_taskId);
            if (isFound != enTaskRetrieveResult.Found) return;

            _taskService = service!;
            // _LoadSummary() reloads _totalSecondsAllTime fresh from the DB —
            // this must run before _InitTimer(), since remaining time is computed
            // as (new estimate) - (DB total), never from stale in-memory numbers.
            _LoadUIText();
            _LoadSummary();
            _InitTimer();
            _UpdateProgress();

            Timer.Visibility = Visibility.Visible;
            StatusLabel.Visibility = Visibility.Collapsed;
            AddExtraTimeBtn.Visibility = Visibility.Collapsed;
            DoneBtn.Visibility = Visibility.Visible;

            Timer.SetIdle();
        }

        // ============================
        // INIT / DISPLAY HELPERS
        // ============================
        private void _InitTimer()
        {
            int est = _taskService.TotalEstimatedMinutes > 0 ? _taskService.TotalEstimatedMinutes : 25;
            int liveSeconds = _LiveSeconds();

            double totalLoggedSecs = _totalSecondsAllTime + liveSeconds;
            double estimatedSecs = est * 60.0;
            double remainingSecs = Math.Max(estimatedSecs - totalLoggedSecs, 0);

            Timer.EstimatedMinutes = est;
            Timer.SetRemaining(TimeSpan.FromSeconds(remainingSecs));

            if (_sessionStarted && ActiveSession.IsRunning)
                Timer.SetRunning(true);
            else if (_sessionStarted)
                Timer.SetRunning(false); // paused
            else
                Timer.SetIdle();
        }

        private int _LiveSeconds() =>
            _sessionStarted && ActiveSession.HasSession && ActiveSession.CurrentTask?.Task.TaskID == _taskId
                ? ActiveSession.ElapsedSeconds
                : 0;

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

        private void _LoadSummary()
        {
            try
            {
                var timerService = ActiveSession.Timer ?? new TimerService(_taskId);
                var (secsAllTime, secsToday, sessions) = timerService.GetTaskTimerSummary();

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

        private void _RenderSessions(List<TimerSession> pastSessions)
        {
            SessionsPanel.Children.Clear();
            PastSessionsPanel.Children.Clear();

            var today = DateTime.Today;
            var todaySessions = pastSessions.Where(s => s.StartTime.Date == today).ToList();
            var previousSessions = pastSessions.Where(s => s.StartTime.Date < today).ToList();

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

            NoPastSessionsText.Visibility = previousSessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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

            if (_sessionStarted && ActiveSession.HasSession && ActiveSession.CurrentTask?.Task.TaskID == _taskId)
            {
                SessionsPanel.Children.Insert(0 ,new SessionCard
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
                NoSessionsText.Visibility = todaySessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            _UpdateTotalToday();
        }

        private void _UpdateTotalToday()
        {
            int totalSeconds = _totalSecondsToday + _LiveSeconds();
            TotalTodayText.Text = _FormatSeconds(totalSeconds);
        }

        private void _UpdateLiveSessionCard()
        {
            if (SessionsPanel.Children.Count == 0) return;

            if (SessionsPanel.Children[0] is SessionCard last && last.IsRunning)
                last.Duration = _FormatSeconds(ActiveSession.ElapsedSeconds);

            _UpdateTotalToday();
        }

        private void _UpdateProgress()
        {
            int est = _taskService.TotalEstimatedMinutes;

            if (est == 0)
            {
                ProgressText.Text = "0";
                ProgressBarControl.ProgressWidth = 0;
                return;
            }

            double totalLogged = _totalSecondsAllTime + _LiveSeconds();
            double estimatedSecs = est * 60.0;
            double progress = Math.Min(totalLogged / estimatedSecs * 100, 100);

            ProgressText.Text = progress.ToString("F2");
            ProgressBarControl.ProgressWidth = progress;

            _UpdateTotalToday();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);

        private string _FormatSeconds(int seconds)
        {
            if (seconds <= 0) return "0 min";
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;

            if (h > 0) return s == 0 ? $"{h}h {m}m" : $"{h}h {m}m {s}s";
            if (m > 0) return s == 0 ? $"{m} min" : $"{m}m {s}s";
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
    }
}