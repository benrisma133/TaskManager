using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Models;

namespace TaskManagerUI.Controls.Cards
{
    public partial class TaskCard : UserControl
    {
        public TaskCard()
        {
            InitializeComponent();
        }

        public void LoadTask(TaskModel task)
        {
            // Row 0 — Title + Subtitle
            TitleText.Text = task.Title;
            SubtitleText.Text = task.Project.Title;

            // Row 2 — Priority + Status + DueDate
            PriorityBadgeControl.Status = task.Priority;
            StatusBadgeControl.Status = task.Status;
            DueDateText.Text = task.DueDate.HasValue
                                          ? task.DueDate.Value.ToString("MMM dd")
                                          : "No date";

            // Row 4 — Estimated + DaysLeft
            EstimatedTimeText.Text = task.EstimatedText;
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

            UpdateShadow(true);
        }

        private void UpdateShadow(bool isDark)
        {
            CardShadow.Color = isDark ? Colors.Black : Colors.Gray;
            CardShadow.Opacity = isDark ? 0.4 : 0.15;
        }

        private void OnThemeChanged(bool isDark)
        {
            UpdateShadow(isDark);
        }
    }
}