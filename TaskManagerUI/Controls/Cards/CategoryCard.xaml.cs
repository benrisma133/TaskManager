using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Repository.Models;

namespace TaskManagerUI.Controls.Cards
{
    public partial class CategoryCard : UserControl
    {
        public CategoryCard()
        {
            InitializeComponent();
            //MainWindow.ThemeChanged += OnThemeChanged;
            CacheMode = new BitmapCache();
        }

        private void OnThemeChanged(bool isDark)
        {
            UpdateShadow(isDark);
        }

        private Geometry? GetIconByName(string iconName)
        {
            var map = new Dictionary<string, string>
            {
                { "monitor",     "IconDesktop"    },
                { "smartphone",  "IconMobile"     },
                { "globe",       "IconWeb"        },
                { "server",      "IconBackend"    },
                { "book-open",   "IconCourse"     },
                { "book",        "IconCourse"     },
                { "search",      "IconSearch"     },
                { "pen-tool",    "IconDesign"     },
                { "settings",    "IconSettings"   },
                { "cpu",         "IconStats"      },
                { "briefcase",   "IconFolder"     },
            };

            if (map.TryGetValue(iconName, out var key))
                return TryFindResource(key) as Geometry;

            return TryFindResource("IconFolder") as Geometry; // fallback
        }

        public void LoadCategory(Category category)
        {
            // Row 0
            IconBg.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(category.Color));
            IconPath.Data = GetIconByName(category.Icon);
            NameText.Text = category.Name;
            TypeText.Text = category.Type;

            // Row 2
            TypeSecondText.Text = category.Type;
            IsCustomText.Text = category.IsCustom ? "Custom" : "System";

            // Row 4
            CategoryStatus.Status = category.IsCustom ? "Custom" : "System";

            //UpdateShadow(true);
        }

        private void UpdateShadow(bool isDark)
        {
            CardShadow.Color = isDark ? Colors.Black : Colors.Gray;
            CardShadow.Opacity = isDark ? 0.4 : 0.15;
        }

        
    }
}