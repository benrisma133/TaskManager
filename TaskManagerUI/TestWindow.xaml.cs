using System.Windows;
using TaskManagerUI.Controls.Cards;
using TaskManagerUI.Models;

namespace TaskManagerUI
{
    public partial class TestWindow : Window
    {

        private readonly List<Models.TaskModel> _tasks = new List<Models.TaskModel>
        {
            // Project 1 - Task Manager App
            new Models.TaskModel { TaskID = 1,  ProjectID = 1, Title = "Setup WPF project",        IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 2,  ProjectID = 1, Title = "Design MainWindow layout",  IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 3,  ProjectID = 1, Title = "Implement ProjectCard",     IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 4,  ProjectID = 1, Title = "Implement CategoryCard",    IsCompleted = false, Status = "In Progress", Priority = "Medium" },
            new Models.TaskModel { TaskID = 5,  ProjectID = 1, Title = "Add CI/CD pipeline",        IsCompleted = false, Status = "In Progress", Priority = "Medium" },

            // Project 2 - E-Commerce Platform
            new Models.TaskModel { TaskID = 6,  ProjectID = 2, Title = "Setup ASP.NET Core project",IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 7,  ProjectID = 2, Title = "Design database schema",    IsCompleted = false, Status = "In Progress", Priority = "Critical"},
            new Models.TaskModel { TaskID = 8,  ProjectID = 2, Title = "Implement auth system",     IsCompleted = false, Status = "Todo",        Priority = "Critical"},

            // Project 3 - Fitness Tracker
            new Models.TaskModel { TaskID = 9,  ProjectID = 3, Title = "Design UI mockups",         IsCompleted = true,  Status = "Completed",   Priority = "Medium" },
            new Models.TaskModel { TaskID = 10, ProjectID = 3, Title = "Implement tracking logic",  IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 11, ProjectID = 3, Title = "Add charts and stats",      IsCompleted = true,  Status = "Completed",   Priority = "Medium" },
            new Models.TaskModel { TaskID = 12, ProjectID = 3, Title = "Publish to app store",      IsCompleted = true,  Status = "Completed",   Priority = "High"   },

            // Project 4 - REST API Gateway
            new Models.TaskModel { TaskID = 13, ProjectID = 4, Title = "Setup API project",         IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 14, ProjectID = 4, Title = "Implement routing",         IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 15, ProjectID = 4, Title = "Add authentication",        IsCompleted = false, Status = "In Progress", Priority = "Critical"},
            new Models.TaskModel { TaskID = 16, ProjectID = 4, Title = "Write API documentation",   IsCompleted = false, Status = "Todo",        Priority = "Low"    },

            // Project 5 - ASP.NET Core Bootcamp
            new Models.TaskModel { TaskID = 17, ProjectID = 5, Title = "Complete module 1",         IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 18, ProjectID = 5, Title = "Complete module 2",         IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 19, ProjectID = 5, Title = "Complete module 3",         IsCompleted = false, Status = "In Progress", Priority = "Medium" },
            new Models.TaskModel { TaskID = 20, ProjectID = 5, Title = "Build final project",       IsCompleted = false, Status = "Todo",        Priority = "High"   },

            // Project 6 - UI Design System
            new Models.TaskModel { TaskID = 21, ProjectID = 6, Title = "Define color tokens",       IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 22, ProjectID = 6, Title = "Build button components",   IsCompleted = true,  Status = "Completed",   Priority = "Medium" },
            new Models.TaskModel { TaskID = 23, ProjectID = 6, Title = "Build card components",     IsCompleted = false, Status = "In Progress", Priority = "Medium" },
            new Models.TaskModel { TaskID = 24, ProjectID = 6, Title = "Write component docs",      IsCompleted = false, Status = "Todo",        Priority = "Low"    },

            // Project 7 - CI/CD Pipeline Setup
            new Models.TaskModel { TaskID = 25, ProjectID = 7, Title = "Setup GitHub Actions",      IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 26, ProjectID = 7, Title = "Add build step",            IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 27, ProjectID = 7, Title = "Add test step",             IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 28, ProjectID = 7, Title = "Add publish artifact step", IsCompleted = true,  Status = "Completed",   Priority = "Medium" },

            // Project 8 - AI Chat Assistant
            new Models.TaskModel { TaskID = 29, ProjectID = 8, Title = "Research LLM APIs",         IsCompleted = false, Status = "In Progress", Priority = "Critical"},
            new Models.TaskModel { TaskID = 30, ProjectID = 8, Title = "Design chat UI",            IsCompleted = false, Status = "Todo",        Priority = "High"   },
            new Models.TaskModel { TaskID = 31, ProjectID = 8, Title = "Implement chat logic",      IsCompleted = false, Status = "Todo",        Priority = "Critical"},

            // Project 9 - ManageClient System
            new Models.TaskModel { TaskID = 32, ProjectID = 9, Title = "Setup 3-tier architecture", IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 33, ProjectID = 9, Title = "Implement CRUD operations", IsCompleted = true,  Status = "Completed",   Priority = "High"   },
            new Models.TaskModel { TaskID = 34, ProjectID = 9, Title = "Add search feature",        IsCompleted = true,  Status = "Completed",   Priority = "Medium" },
            new Models.TaskModel { TaskID = 35, ProjectID = 9, Title = "Add CI/CD pipeline",        IsCompleted = true,  Status = "Completed",   Priority = "Medium" },
        };

        public TestWindow()
        {
            InitializeComponent();
            //LoadCategories();
            LoadProjects();
        }

        private double GetProjectProgress(int projectId)
        {
            var projectTasks = _tasks.Where(t => t.ProjectID == projectId).ToList();
            if (projectTasks.Count == 0) return 0;
            return (double)projectTasks.Count(t => t.IsCompleted) / projectTasks.Count * 100;
        }

        private void LoadCategories()
        {
            //var categories = new List<Category>
            //{
            //    new Category { CategoryID = 1,  Name = "Desktop App", Color = "#6366F1", Icon = "monitor",    Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 2,  Name = "Mobile App",  Color = "#8B5CF6", Icon = "smartphone", Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 3,  Name = "Web App",     Color = "#3B82F6", Icon = "globe",      Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 4,  Name = "Backend",     Color = "#06B6D4", Icon = "server",     Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 5,  Name = "Course",      Color = "#F59E0B", Icon = "book-open",  Type = "Course",  IsCustom = false },
            //    new Category { CategoryID = 6,  Name = "Research",    Color = "#10B981", Icon = "search",     Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 7,  Name = "Design",      Color = "#EC4899", Icon = "pen-tool",   Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 8,  Name = "DevOps",      Color = "#EF4444", Icon = "settings",   Type = "Project", IsCustom = false },
            //    new Category { CategoryID = 9,  Name = "AI / ML",     Color = "#A855F7", Icon = "cpu",        Type = "Project", IsCustom = true  },
            //    new Category { CategoryID = 10, Name = "Reading",     Color = "#F97316", Icon = "book",       Type = "Course",  IsCustom = true  },
            //    new Category { CategoryID = 11, Name = "Freelance",   Color = "#14B8A6", Icon = "briefcase",  Type = "Project", IsCustom = true  },
            //};

            //foreach (var category in categories)
            //{
            //    var card = new CategoryCard();
            //    card.Margin = new Thickness(8);
            //    card.VerticalAlignment = VerticalAlignment.Top;
            //    card.LoadCategory(category);
            //    CardsPanel.Children.Add(card);
            //}
        }

        private void LoadProjects()
        {
            var categories = new Dictionary<int, Category>
            {
                { 1,  new Category { CategoryID = 1,  Name = "Desktop App", Color = "#6366F1", Icon = "monitor",    Type = "Project", IsCustom = false } },
                { 2,  new Category { CategoryID = 2,  Name = "Mobile App",  Color = "#8B5CF6", Icon = "smartphone", Type = "Project", IsCustom = false } },
                { 3,  new Category { CategoryID = 3,  Name = "Web App",     Color = "#3B82F6", Icon = "globe",      Type = "Project", IsCustom = false } },
                { 4,  new Category { CategoryID = 4,  Name = "Backend",     Color = "#06B6D4", Icon = "server",     Type = "Project", IsCustom = false } },
                { 5,  new Category { CategoryID = 5,  Name = "Course",      Color = "#F59E0B", Icon = "book-open",  Type = "Course",  IsCustom = false } },
                { 7,  new Category { CategoryID = 7,  Name = "Design",      Color = "#EC4899", Icon = "pen-tool",   Type = "Project", IsCustom = false } },
                { 8,  new Category { CategoryID = 8,  Name = "DevOps",      Color = "#EF4444", Icon = "settings",   Type = "Project", IsCustom = false } },
                { 9,  new Category { CategoryID = 9,  Name = "AI / ML",     Color = "#A855F7", Icon = "cpu",        Type = "Project", IsCustom = true  } },
                { 11, new Category { CategoryID = 11, Name = "Freelance",   Color = "#14B8A6", Icon = "briefcase",  Type = "Project", IsCustom = true  } },
            };

            var projects = new List<Project>
            {
                new Project
                {
                    ProjectID   = 1,
                    Title       = "Task Manager App",
                    Description = "Desktop task management application with WPF",
                    CategoryID  = 1,
                    Category    = categories[1],
                    Status      = "In Progress",
                    Priority    = "High",
                    StartDate   = new DateOnly(2026, 1, 1),
                    DueDate     = new DateOnly(2026, 6, 30),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 2,
                    Title       = "E-Commerce Platform",
                    Description = "Full stack web store with ASP.NET Core",
                    CategoryID  = 3,
                    Category    = categories[3],
                    Status      = "Planning",
                    Priority    = "Critical",
                    StartDate   = new DateOnly(2026, 3, 1),
                    DueDate     = new DateOnly(2026, 5, 10),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 3,
                    Title       = "Fitness Tracker",
                    Description = "Mobile app for tracking workouts",
                    CategoryID  = 2,
                    Category    = categories[2],
                    Status      = "Completed",
                    Priority    = "Medium",
                    StartDate   = new DateOnly(2025, 10, 1),
                    DueDate     = new DateOnly(2026, 1, 15),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 4,
                    Title       = "REST API Gateway",
                    Description = "Backend service for microservices communication",
                    CategoryID  = 4,
                    Category    = categories[4],
                    Status      = "In Progress",
                    Priority    = "High",
                    StartDate   = new DateOnly(2026, 2, 1),
                    DueDate     = new DateOnly(2026, 7, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 5,
                    Title       = "ASP.NET Core Bootcamp",
                    Description = "Complete course on ASP.NET Core and REST APIs",
                    CategoryID  = 5,
                    Category    = categories[5],
                    Status      = "In Progress",
                    Priority    = "Medium",
                    StartDate   = new DateOnly(2026, 4, 1),
                    DueDate     = new DateOnly(2026, 8, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 6,
                    Title       = "UI Design System",
                    Description = "WPF component library and design tokens",
                    CategoryID  = 7,
                    Category    = categories[7],
                    Status      = "In Progress",
                    Priority    = "Low",
                    StartDate   = new DateOnly(2026, 3, 15),
                    DueDate     = new DateOnly(2026, 9, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 7,
                    Title       = "CI/CD Pipeline Setup",
                    Description = "GitHub Actions pipelines for all projects",
                    CategoryID  = 8,
                    Category    = categories[8],
                    Status      = "Completed",
                    Priority    = "High",
                    StartDate   = new DateOnly(2026, 1, 1),
                    DueDate     = new DateOnly(2026, 2, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 8,
                    Title       = "AI Chat Assistant",
                    Description = "LLM-powered assistant integrated into desktop app",
                    CategoryID  = 9,
                    Category    = categories[9],
                    Status      = "Planning",
                    Priority    = "Critical",
                    StartDate   = new DateOnly(2026, 5, 1),
                    DueDate     = new DateOnly(2026, 12, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
                new Project
                {
                    ProjectID   = 9,
                    Title       = "ManageClient System",
                    Description = "Freelance client management desktop app",
                    CategoryID  = 11,
                    Category    = categories[11],
                    Status      = "Completed",
                    Priority    = "Medium",
                    StartDate   = new DateOnly(2025, 11, 1),
                    DueDate     = new DateOnly(2026, 1, 1),
                    CreatedAt   = DateTime.Now,
                    UpdatedAt   = DateTime.Now,
                },
            };

            foreach (var project in projects)
            {
                var card = new ProjectCard();
                card.Margin = new Thickness(8);
                card.VerticalAlignment = VerticalAlignment.Top;
                card.LoadProject(project);
                card.UpdateProgress(GetProjectProgress(project.ProjectID));
                ProjectsPanel.Children.Add(card);
            }
        }
    }
}