using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Models;

namespace TaskManagerUI.Controls.Cards
{
    public partial class ProjectCard : UserControl
    {
        public ProjectCard()
        {
            InitializeComponent();
        }

        private Geometry? GetIconByName(string iconName)
        {
            var map = new Dictionary<string, string>
            {
                { "monitor",    "IconDesktop"  },
                { "smartphone", "IconMobile"   },
                { "globe",      "IconWeb"      },
                { "server",     "IconBackend"  },
                { "book-open",  "IconCourse"   },
                { "book",       "IconCourse"   },
                { "search",     "IconSearch"   },
                { "pen-tool",   "IconDesign"   },
                { "settings",   "IconSettings" },
                { "cpu",        "IconStats"    },
                { "briefcase",  "IconFolder"   },
            };

            if (map.TryGetValue(iconName, out var key))
                return TryFindResource(key) as Geometry;

            return TryFindResource("IconFolder") as Geometry; // fallback
        }

        public void LoadProject(Project project)
        {
            // Row 0 — Icon + Title + Type
            IconBg.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(project.Category.Color));
            IconPath.Data = GetIconByName(project.Category.Icon);
            NameText.Text = project.Title;
            TypeText.Text = project.Category.Name;

            // Row 2 — Priority + DueDate + DaysLeft
            PriorityBadge.Status = project.Priority;
            DueDateText.Text = project.DueDate.HasValue
                                   ? project.DueDate.Value.ToString("MMM dd")
                                   : "No date";
            DaysLeftText.Text = project.DaysLeftText;

            // Row 4 — Progress
            // Progress is based on completed tasks — pass 0 for now until tasks are loaded
            UpdateProgress(0);

            // Row 6 — Status badge
            CategoryStatus.Status = project.Status;

            UpdateShadow(true);
        }

        public void UpdateProgress(double percent)
        {
            ProgressText.Text = $"{percent:F0} %";
            ProgressFill.ProgressWidth = percent;
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