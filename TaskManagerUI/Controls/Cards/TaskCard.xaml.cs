using Repository.Models;
using Service.Services;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Models;

namespace TaskManagerUI.Controls.Cards
{
    public partial class TaskCard : UserControl
    {
        private bool _isDark;
        public TaskCard()
        {
            InitializeComponent();
            MainWindow.ThemeChanged += OnThemeChanged;
            CacheMode = new BitmapCache();

            _isDark = Properties.Settings.Default.IsDarkTheme;
        }

        public void LoadTask(TaskService task)
        {
            // Row 0 — Title + Subtitle
            TitleText.Text = task.Title;
            if(task.Project != null)
                SubtitleText.Text = task.Project.Title;

            ApplyCategoryColor(task.Project!.Category!.Color);

            // Row 2 — Priority + Status + DueDate
            PriorityBadgeControl.Status = task.Priority;
            StatusBadgeControl.Status = task.Status;
            DueDateText.Text = task.DueDate.HasValue
                                          ? task.DueDate.Value.ToString("MMM dd")
                                          : "No date";

            

            // Row 4 — Estimated + DaysLeft
            EstimatedTimeText.Text = task.EstimatedText;

            if (task.ExtraMinutes > 0)
            {
                ExtraTimeText.Text = $"+{_FormatMinutes(task.ExtraMinutes)} extra";
                TotalTimeText.Text = $"Total: {_FormatMinutes(task.TotalEstimatedMinutes)}";
                ExtraTimePanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ExtraTimePanel.Visibility = System.Windows.Visibility.Collapsed;
            }


            DaysLeftText.Text = task.DaysLeftText;

            // Row 4 — DaysLeft color
            if (task.IsOverdue)
                DaysLeftText.Foreground = (Brush)FindResource("ErrorBrush");
            else
                DaysLeftText.Foreground = (Brush)FindResource("TextSecondaryBrush");

            // Row 6 — Buttons
            StartTaskBtn.Click += (s, e) => { };
            EditBtn.Click += (s, e) => { };
            DeleteBtn.Click += (s, e) => { };

            UpdateShadow(_isDark);
        }

        public void LoadTask(TaskItemDetails task)
        {
            // Row 0 — Title + Subtitle
            TitleText.Text = task.Title;
            SubtitleText.Text = task.ProjectTitle;

            ApplyCategoryColor(task.Color);

            // Row 2 — Priority + Status + DueDate
            PriorityBadgeControl.Status = task.Priority;
            StatusBadgeControl.Status = task.Status;
            DueDateText.Text = task.DueDate.HasValue
                                            ? task.DueDate.Value.ToString("MMM dd")
                                            : "No date";

            EstimatedTimeText.Text = task.EstimatedText;

            if (task.ExtraMinutes > 0)
            {
                ExtraTimeText.Text = $"+{_FormatMinutes(task.ExtraMinutes)} extra";
                TotalTimeText.Text = $"Total: {_FormatMinutes(task.TotalEstimatedMinutes)}";
                ExtraTimePanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ExtraTimePanel.Visibility = System.Windows.Visibility.Collapsed;
            }



            DaysLeftText.Text = task.DaysLeftText;

            // Row 4 — DaysLeft color
            DaysLeftText.Foreground = task.IsOverdue
                ? (Brush)FindResource("ErrorBrush")
                : (Brush)FindResource("TextSecondaryBrush");

            // Row 6 — Buttons
            StartTaskBtn.Click += (s, e) => { };
            EditBtn.Click += (s, e) => { };
            DeleteBtn.Click += (s, e) => { };

            UpdateShadow(_isDark);
        }

        private void UpdateShadow(bool isDark)
        {
            if (isDark)
            {
                CardShadow.Color = Colors.Black;
                CardShadow.Opacity = 0.55;
                CardShadow.BlurRadius = 14;
                CardShadow.ShadowDepth = 2;
            }
            else
            {
                // Light mode: use a LIGHT color, not dark
                CardShadow.Color = (Color)ColorConverter.ConvertFromString("#000000");
                CardShadow.Opacity = 0.15;
                CardShadow.BlurRadius = 8;
                CardShadow.ShadowDepth = 2;
            }
            CardShadow.Direction = 270;
        }

        private void OnThemeChanged(bool isDark)
        {
            UpdateShadow(isDark);
        }

        private string _FormatMinutes(int minutes)
        {
            if (minutes <= 0) return "0 min";
            if (minutes < 60) return $"{minutes} min";

            int h = minutes / 60;
            int m = minutes % 60;
            return m == 0 ? $"{h}h" : $"{h}h {m}m";
        }

        private void ApplyCategoryColor(string? color)
        {
            if (!string.IsNullOrWhiteSpace(color))
            {
                try
                {
                    var brush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(color));
                    ColorAccentBorder.Background = brush;
                }
                catch
                {
                    ColorAccentBorder.SetResourceReference(
                        Border.BackgroundProperty, "AccentBrush");
                }
            }
            else
            {
                ColorAccentBorder.SetResourceReference(
                    Border.BackgroundProperty, "AccentBrush");
            }
        }
    }
}