using System.Windows;
using TaskManagerUI.Controls.Cards;
using TaskManagerUI.Models;

namespace TaskManagerUI
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = new List<Category>
            {
                new Category { CategoryID = 1,  Name = "Desktop App", Color = "#6366F1", Icon = "monitor",    Type = "Project", IsCustom = false },
                new Category { CategoryID = 2,  Name = "Mobile App",  Color = "#8B5CF6", Icon = "smartphone", Type = "Project", IsCustom = false },
                new Category { CategoryID = 3,  Name = "Web App",     Color = "#3B82F6", Icon = "globe",      Type = "Project", IsCustom = false },
                new Category { CategoryID = 4,  Name = "Backend",     Color = "#06B6D4", Icon = "server",     Type = "Project", IsCustom = false },
                new Category { CategoryID = 5,  Name = "Course",      Color = "#F59E0B", Icon = "book-open",  Type = "Course",  IsCustom = false },
                new Category { CategoryID = 6,  Name = "Research",    Color = "#10B981", Icon = "search",     Type = "Project", IsCustom = false },
                new Category { CategoryID = 7,  Name = "Design",      Color = "#EC4899", Icon = "pen-tool",   Type = "Project", IsCustom = false },
                new Category { CategoryID = 8,  Name = "DevOps",      Color = "#EF4444", Icon = "settings",   Type = "Project", IsCustom = false },
                new Category { CategoryID = 9,  Name = "AI / ML",     Color = "#A855F7", Icon = "cpu",        Type = "Project", IsCustom = true  },
                new Category { CategoryID = 10, Name = "Reading",     Color = "#F97316", Icon = "book",       Type = "Course",  IsCustom = true  },
                new Category { CategoryID = 11, Name = "Freelance",   Color = "#14B8A6", Icon = "briefcase",  Type = "Project", IsCustom = true  },
            };

            foreach (var category in categories)
            {
                var card = new CategoryCard();
                card.Margin = new Thickness(8);
                card.VerticalAlignment = VerticalAlignment.Top;
                card.LoadCategory(category);
                CardsPanel.Children.Add(card);
            }
        }
    }
}
