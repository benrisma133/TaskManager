using FontAwesome.Sharp;
using Service.Enums.Dashboard;
using Service.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TaskManagerUI.Pages.Dashboard
{
    public partial class DashboardPage : UserControl
    {
        // ─── View Models for Binding ───────────────────────────────────────
        private class BarItem
        {
            public string DayLabel { get; set; } = string.Empty;
            public double BarWidth { get; set; }
            public Brush BarColor { get; set; } = Brushes.Gray;
            public Brush LabelColor { get; set; } = Brushes.Gray;
        }

        private class TaskItem
        {
            public string Title { get; set; } = string.Empty;
            public string ProjectTitle { get; set; } = string.Empty;
            public string Priority { get; set; } = string.Empty;
            public string DueDateLabel { get; set; } = string.Empty;
            public Brush DotColor { get; set; } = Brushes.Gray;
            public Brush PriorityForeground { get; set; } = Brushes.Gray;
            public Brush PriorityBackground { get; set; } = Brushes.Transparent;
        }

        private class SessionItem
        {
            public string TaskTitle { get; set; } = string.Empty;
            public string TimeRange { get; set; } = string.Empty;
            public string DurationLabel { get; set; } = string.Empty;
            public Brush DotColor { get; set; } = Brushes.Gray;
        }

        // ─── Fields ────────────────────────────────────────────────────────
        private DashboardService? _service;
        private MainWindow? _mainWindow;

        // ─── Constructor ───────────────────────────────────────────────────
        public DashboardPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += OnLoaded;
        }

        // ─── Load ──────────────────────────────────────────────────────────
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await Task.Run(() =>
            {
                var (result, service) = DashboardService.Load();

                if (result == enDashboardLoadResult.Success && service is not null)
                {
                    _service = service;
                    Dispatcher.Invoke(() => ApplyToUi(service));
                }
                else
                {
                    Dispatcher.Invoke(() =>
                        MessageBox.Show("Failed to load dashboard data",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning));
                }
            });
        }

        // ─── Bind Data to UI ───────────────────────────────────────────────
        private void ApplyToUi(DashboardService service)
        {
            // ── Header ─────────────────────────────────────────────────
            ApplyGreeting(service.Greeting);
            TodayText.Text = service.TodayLabel;

            // ── Level ──────────────────────────────────────────────────
            // ── Level ──────────────────────────────────────────────────
            LevelBadgeIcon.Level = service.LevelType;
            LevelNameText.Text = service.IsMaxLevel
                ? $"{service.CurrentLevel} · MAX LEVEL"
                : $"{service.CurrentLevel} · Level {service.ProgressPercent}%";

            LevelProgressBar.Width = service.IsMaxLevel
                ? 120
                : 120 * service.ProgressPercent / 100.0;

            LevelPointsText.Text = service.IsMaxLevel
                ? $"{service.TotalPoints} pts"
                : $"{service.TotalPoints} / {service.NextLevelPoints} pts → {service.NextLevel}";

            // ── Stat Cards ─────────────────────────────────────────────
            CardToday.Value = service.CompletedToday.ToString();
            CardToday.Label = "tasks done";
            CardToday.Icon = (Geometry)Application.Current.Resources["IconCheckCircle"];

            CardWeek.Value = service.CompletedLastWeek.ToString();
            CardWeek.Label = "last 7 days";
            CardWeek.Icon = (Geometry)Application.Current.Resources["IconCalendar"];

            CardMonth.Value = service.CompletedLastMonth.ToString();
            CardMonth.Label = "last 30 days";
            CardMonth.Icon = (Geometry)Application.Current.Resources["IconTrendingUp"];

            CardPoints.Value = service.TotalPoints.ToString();
            CardPoints.Label = $"+{service.PointsToday} today";
            CardPoints.Icon = (Geometry)Application.Current.Resources["IconStar"];

            // ── Streaks ────────────────────────────────────────────────
            CurrentStreakText.Text = service.CurrentStreak.ToString();
            LongestStreakText.Text = service.LongestStreak.ToString();
            BuildStreakDots(service.CurrentStreak);

            // ── Hours ──────────────────────────────────────────────────
            MinutesTodayText.Text = service.MinutesTodayFormatted;
            SessionCountText.Text = $"today · {service.TodaySessions.Count} session{(service.TodaySessions.Count == 1 ? "" : "s")}";
            BuildWeeklyBars(service);

            // ── Active Tasks ───────────────────────────────────────────
            BuildActiveTasks(service);

            // ── Today Sessions ─────────────────────────────────────────
            BuildTodaySessions(service);
        }

        // ─── Greeting + Icon ───────────────────────────────────────────────
        private void ApplyGreeting(enGreeting greeting)
        {
            switch (greeting)
            {
                case enGreeting.Morning:
                    GreetingText.Text = "Good morning";
                    GreetingIcon.Icon = IconChar.Sun;
                    GreetingIcon.Foreground = (Brush)Application.Current.Resources["WarningBrush"];
                    break;

                case enGreeting.Afternoon:
                    GreetingText.Text = "Good afternoon";
                    GreetingIcon.Icon = IconChar.CloudSun;
                    GreetingIcon.Foreground = (Brush)Application.Current.Resources["AccentBrush"];
                    break;

                case enGreeting.Evening:
                    GreetingText.Text = "Good evening";
                    GreetingIcon.Icon = IconChar.Moon;
                    GreetingIcon.Foreground = (Brush)Application.Current.Resources["InfoBrush"];
                    break;
            }
        }

        // ─── Streak Dots (Last 14 Days) ────────────────────────────────────
        private void BuildStreakDots(int currentStreak)
        {
            StreakDots.Children.Clear();

            var today = DateTime.Today;
            var streakStart = today.AddDays(-(currentStreak - 1));

            var activeBrush = (Brush)Application.Current.Resources["WarningBrush"];
            var todayBrush = (Brush)Application.Current.Resources["SuccessBrush"];
            var inactBrush = (Brush)Application.Current.Resources["CardHoverBackgroundBrush"];

            for (int i = 13; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                bool active = day >= streakStart && day <= today;
                bool isToday = day == today;

                var dot = new Border
                {
                    Width = 10,
                    Height = 10,
                    CornerRadius = new CornerRadius(3),
                    Background = isToday ? todayBrush
                                           : active ? activeBrush
                                                    : inactBrush,
                    Margin = new Thickness(0, 0, 4, 4)
                };

                if (isToday)
                    dot.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.LimeGreen,
                        BlurRadius = 6,
                        ShadowDepth = 0,
                        Opacity = 0.8
                    };

                StreakDots.Children.Add(dot);
            }
        }

        // ─── Weekly Bar Chart ──────────────────────────────────────────────
        private void BuildWeeklyBars(DashboardService service)
        {
            const double maxBarWidth = 200.0;

            var successBrush = (Brush)Application.Current.Resources["SuccessBrush"];
            var mutedBrush = (Brush)Application.Current.Resources["TextMutedBrush"];

            var bars = service.WeeklyMinutes.Select(d => new BarItem
            {
                DayLabel = d.IsToday ? "Today" : d.DayLabel,
                BarWidth = service.MaxWeeklyMinutes == 0
                                 ? 0
                                 : d.TotalMinutes / (double)service.MaxWeeklyMinutes * maxBarWidth,
                BarColor = d.IsToday ? successBrush : mutedBrush,
                LabelColor = d.IsToday ? successBrush : mutedBrush
            }).ToList();

            WeeklyBarsControl.ItemsSource = bars;
        }

        // ─── Active Tasks ──────────────────────────────────────────────────
        private void BuildActiveTasks(DashboardService service)
        {
            // Get first 6 active tasks from TaskService
            var (result, tasks, _) = TaskService.GetAll(
                pageNumber: 1,
                pageSize: 6,
                search: null,
                priority: null,
                status: null,
                projectId: null
            );

            if (result != Service.Enums.Task.enTaskRetrieveResult.Found || tasks.Count == 0)
            {
                NoTasksText.Visibility = Visibility.Visible;
                ActiveTasksControl.Visibility = Visibility.Collapsed;
                return;
            }

            NoTasksText.Visibility = Visibility.Collapsed;
            ActiveTasksControl.Visibility = Visibility.Visible;

            var items = tasks
                .Where(t => !t.IsCompleted)
                .Take(6)
                .Select(t => new TaskItem
                {
                    Title = t.Title,
                    ProjectTitle = t.ProjectTitle,
                    Priority = t.Priority.ToUpper(),
                    DueDateLabel = FormatDueDate(t.DueDate),
                    DotColor = ParseHexBrush(t.Color ?? "#000000"),
                    PriorityForeground = PriorityForeground(t.Priority),
                    PriorityBackground = PriorityBackground(t.Priority)
                }).ToList();

            ActiveTasksControl.ItemsSource = items;
        }

        // ─── Today's Sessions ──────────────────────────────────────────────
        private void BuildTodaySessions(DashboardService service)
        {
            if (service.TodaySessions.Count == 0)
            {
                NoSessionsText.Visibility = Visibility.Visible;
                TodaySessionsControl.Visibility = Visibility.Collapsed;
                return;
            }

            NoSessionsText.Visibility = Visibility.Collapsed;
            TodaySessionsControl.Visibility = Visibility.Visible;

            var items = service.TodaySessions.Select(s => new SessionItem
            {
                TaskTitle = s.TaskTitle,
                TimeRange = $"{s.StartTime:HH:mm} → {s.EndTime:HH:mm}",
                DurationLabel = FormatDuration(s.DurationMinutes),
                DotColor = ParseHexBrush(s.CategoryColor)
            }).ToList();

            TodaySessionsControl.ItemsSource = items;
        }

        // ─── Helpers ───────────────────────────────────────────────────────
        private static Brush ParseHexBrush(string hex)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        private static Brush PriorityForeground(string priority) => priority switch
        {
            "Critical" => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            "High" => new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x35)),
            "Medium" => new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
            _ => new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
        };

        private static Brush PriorityBackground(string priority) => priority switch
        {
            "Critical" => new SolidColorBrush(Color.FromArgb(0x26, 0xEF, 0x44, 0x44)),
            "High" => new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0x6B, 0x35)),
            "Medium" => new SolidColorBrush(Color.FromArgb(0x26, 0x3B, 0x82, 0xF6)),
            _ => new SolidColorBrush(Color.FromArgb(0x1A, 0x64, 0x74, 0x8B))
        };

        private static string FormatDuration(int minutes)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            return h > 0 ? $"{h}h {m:00}m" : $"{m}m";
        }

        private static string FormatDueDate(DateOnly? dueDate)
        {
            if (dueDate is null)
                return "No date";

            var today = DateOnly.FromDateTime(DateTime.Today);
            int diff = dueDate.Value.DayNumber - today.DayNumber;

            return diff switch
            {
                < 0 => "Overdue",
                0 => "Due today",
                1 => "Due tomorrow",
                _ => $"Due {dueDate.Value:MMM d}"
            };
        }

        // ─── Events ────────────────────────────────────────────────────────
        private void SeeAllTasks_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Navigate to Tasks page
            // You will wire this up when you integrate navigation
            //MessageBox.Show("Navigate to Tasks page", "Info");
            _mainWindow?.NavigateToTasksPage();
        }
    }
}