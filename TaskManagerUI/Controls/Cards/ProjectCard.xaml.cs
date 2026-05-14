using Repository.Models;
using Service.Services;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Models;

namespace TaskManagerUI.Controls.Cards
{
    public partial class ProjectCard : UserControl
    {
        private bool _isDark;
        public ProjectCard()
        {
            InitializeComponent();
            MainWindow.ThemeChanged += OnThemeChanged;
            CacheMode = new BitmapCache();

            _isDark = Properties.Settings.Default.IsDarkTheme;
        }

        private Geometry? GetIconByName(string? iconName)
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

            if (map.TryGetValue(iconName!, out var key))
                return TryFindResource(key) as Geometry;

            return TryFindResource("IconFolder") as Geometry; // fallback
        }

        public void LoadProject(ProjectService project)
        {
            // Row 0 — Icon + Title + Type
            IconBg.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(project.Category?.Color));
            IconPath.Data = GetIconByName(project.Category?.Icon);
            NameText.Text = project.Title;
            TypeText.Text = project.Category?.Name;

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

            UpdateShadow(_isDark);
        }

        public void LoadProject(ProjectDetails project)
        {
            // ── Row 0 : Icon + Title + Type ────────────────────────────────
            IconBg.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(project.CategoryColor));
            IconPath.Data = GetIconByName(project.CategoryIcon);
            NameText.Text = project.Title;
            TypeText.Text = project.CategoryName;

            // ── Row 2 : Priority + DueDate + DaysLeft ──────────────────────
            PriorityBadge.Status = project.Priority;
            DueDateText.Text = project.DueDate.HasValue
                                   ? project.DueDate.Value.ToString("MMM dd")
                                   : "No date";
            DaysLeftText.Text = project.DaysLeftText;

            // ── Row 4 : Progress ───────────────────────────────────────────
            UpdateProgress(project.ProgressPercentage);

            // ── Row 6 : Status Badge ───────────────────────────────────────
            CategoryStatus.Status = project.Status;

            UpdateShadow(_isDark);
        }

        public void UpdateProgress(double percent)
        {
            ProgressText.Text = $"{percent:F0} %";
            ProgressFill.ProgressWidth = percent;
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
    }
}