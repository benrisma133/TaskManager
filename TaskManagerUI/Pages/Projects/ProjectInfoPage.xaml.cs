using Repository.Models;
using Service.Enums.Project;
using Service.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaskManagerUI.Pages.Projects;

public partial class ProjectInfoPage : UserControl
{
    // ============================
    // FIELDS
    // ============================
    private readonly int _projectId;
    private readonly MainWindow _mainWindow;
    private ProjectService? _projectService;

    public event EventHandler? BackRequested;

    // ============================
    // CONSTRUCTOR
    // ============================
    public ProjectInfoPage(int projectId, MainWindow mainWindow)
    {
        InitializeComponent();
        _projectId = projectId;
        _mainWindow = mainWindow;
        Loaded += (s, e) => _Load();
    }

    // ============================
    // LOAD
    // ============================
    private void _Load()
    {
        _LoadProject();
        _LoadTasks();
        _LoadPointsLog();
        _LoadTimeSummary();
    }

    // ============================
    // LOAD PROJECT
    // ============================
    private void _LoadProject()
    {
        var (result, service) = ProjectService.Find(_projectId);

        if (result != enProjectRetrieveResult.Found || service is null)
        {
            MessageBox.Show("Project not found.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _projectService = service;

        // Title + category
        ProjectTitleText.Text = service.Title;
        CategoryNameText.Text = $"{service.Category?.Name ?? "Unknown"} · created {service.CreatedAt:MMM dd, yyyy}";

        // Category icon + color
        if (service.Category is not null)
        {
            _ApplyCategoryIcon(service.Category.Icon);
            _ApplyCategoryColor(service.Category.Color);
        }

        StatusBadgeControl.Status = service.Status;
        PriorityBadgeControl.Status = service.Priority;

        // Due date
        if (service.DueDate.HasValue)
        {
            DueDateText.Text = $"Due {service.DueDate.Value:MMM dd}";
            DaysLeftText.Text = service.DaysLeftText;
            DaysLeftBadge.Visibility = Visibility.Visible;
        }
        else
        {
            DueDateText.Text = "No due date";
            DaysLeftBadge.Visibility = Visibility.Collapsed;
        }

        // Description
        if (!string.IsNullOrWhiteSpace(service.Description))
        {
            DescriptionText.Text = service.Description;
            DescriptionPanel.Visibility = Visibility.Visible;
        }

        // Hide done button if already completed
        if (service.Status == "Completed")
            DoneBtn.Visibility = Visibility.Collapsed;
    }

    // ============================
    // LOAD TASKS
    // ============================
    private void _LoadTasks()
    {
        var (result, tasks) = TaskService.GetByProject(_projectId);

        int total = tasks.Count;
        int completed = tasks.Count(t => t.IsCompleted);
        int inProgress = tasks.Count(t => t.Status == "InProgress");

        TotalTasksText.Text = total.ToString();
        CompletedTasksText.Text = completed.ToString();
        InProgressTasksText.Text = inProgress.ToString();

        // Progress
        double pct = total > 0 ? (double)completed / total * 100 : 0;
        ProgressPercentText.Text = $"{pct:F0}%";
        ProgressBarControl.ProgressWidth = pct;
        ProgressSubText.Text = $"{completed} of {total} tasks completed";

        // Task list
        TaskListPanel.Children.Clear();

        if (tasks.Count == 0)
        {
            NoTasksText.Visibility = Visibility.Visible;
            return;
        }

        NoTasksText.Visibility = Visibility.Collapsed;

        foreach (var task in tasks)
        {
            TaskListPanel.Children.Add(_BuildTaskRow(task));
        }
    }

    // ============================
    // LOAD POINTS LOG
    // ============================
    private void _LoadPointsLog()
    {
        var (result, logs) = ProjectService.GetPointsLog(_projectId);

        int totalPoints = logs.Sum(l => l.Points);
        PointsEarnedText.Text = totalPoints.ToString();

        PointsLogPanel.Children.Clear();

        if (logs.Count == 0)
        {
            NoPointsText.Visibility = Visibility.Visible;
            return;
        }

        NoPointsText.Visibility = Visibility.Collapsed;

        foreach (var log in logs)
        {
            PointsLogPanel.Children.Add(_BuildPointsRow(log));
        }
    }

    // ============================
    // LOAD TIME SUMMARY
    // ============================
    private void _LoadTimeSummary()
    {
        var (result, sessions) = ProjectService.GetSessionSummary(_projectId);

        int totalSeconds = sessions.Sum(s => s.TotalSeconds);
        TotalTimeText.Text = _FormatSeconds(totalSeconds);

        TimelinePanel.Children.Clear();

        if (sessions.Count == 0)
        {
            NoTimeText.Visibility = Visibility.Visible;
            return;
        }

        NoTimeText.Visibility = Visibility.Collapsed;

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            bool isLast = i == sessions.Count - 1;
            TimelinePanel.Children.Add(_BuildTimelineRow(session, isLast));
        }
    }

    // ============================
    // BUILD TASK ROW
    // ============================
    private UIElement _BuildTaskRow(TaskItemDetails task)
    {
        var row = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 9, 12, 9),
            Margin = new Thickness(0, 0, 0, 6),
            BorderThickness = new Thickness(1)
        };
        row.SetResourceReference(Border.BackgroundProperty, "InputBackgroundBrush");
        row.SetResourceReference(Border.BorderBrushProperty, "BorderDefaultBrush");

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Status dot
        var dot = new System.Windows.Shapes.Ellipse
        {
            Width = 8,
            Height = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        string dotColor = task.Status switch
        {
            "Done" => "SuccessBrush",
            "InProgress" => "WarningBrush",
            _ => "TextMutedBrush"
        };
        dot.SetResourceReference(System.Windows.Shapes.Ellipse.FillProperty, dotColor);
        Grid.SetColumn(dot, 0);

        // Title + priority
        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var title = new TextBlock
        {
            Text = task.Title,
            FontSize = 13,
        };

        if (task.IsCompleted)
        {
            title.TextDecorations = TextDecorations.Strikethrough;
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
        }
        else
        {
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
        }

        var priority = new TextBlock
        {
            Text = $"{task.Priority} · {task.Status}",
            FontSize = 11,
            Margin = new Thickness(0, 2, 0, 0)
        };
        priority.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");

        info.Children.Add(title);
        info.Children.Add(priority);
        Grid.SetColumn(info, 1);

        // Play button
        var playBtn = new Button
        {
            Width = 28,
            Height = 28,
            Cursor = System.Windows.Input.Cursors.Hand,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center
        };

        var playBorder = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1)
        };
        playBorder.SetResourceReference(Border.BackgroundProperty, "CardBackgroundBrush");
        playBorder.SetResourceReference(Border.BorderBrushProperty, "BorderDefaultBrush");

        var playIcon = new FontAwesome.Sharp.IconImage
        {
            Icon = FontAwesome.Sharp.IconChar.Play,
            Width = 10,
            Height = 10
        };
        playIcon.SetResourceReference(FontAwesome.Sharp.IconImage.ForegroundProperty, "TextMutedBrush");

        playBorder.Child = playIcon;
        playBtn.Content = playBorder;

        int taskId = task.TaskID;
        playBtn.Click += (s, e) => _mainWindow.NavigateToTimer(taskId ,fromDashboard:true);

        Grid.SetColumn(playBtn, 2);

        grid.Children.Add(dot);
        grid.Children.Add(info);
        grid.Children.Add(playBtn);

        row.Child = grid;
        return row;
    }

    // ============================
    // BUILD POINTS ROW
    // ============================
    private UIElement _BuildPointsRow(ProjectPointsLog log)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = new TextBlock
        {
            Text = log.TaskTitle ?? log.Reason,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var pts = new TextBlock
        {
            Text = $"+{log.Points} pts",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        pts.SetResourceReference(TextBlock.ForegroundProperty, "SuccessBrush");

        Grid.SetColumn(label, 0);
        Grid.SetColumn(pts, 1);

        row.Children.Add(label);
        row.Children.Add(pts);

        return row;
    }

    // ============================
    // BUILD TIMELINE ROW
    // ============================
    private UIElement _BuildTimelineRow(ProjectSessionSummary session, bool isLast)
    {
        var outer = new Grid { Margin = new Thickness(0, 0, 0, isLast ? 0 : 12) };
        outer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        outer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Dot + line
        var dotStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };

        var dot = new System.Windows.Shapes.Ellipse { Width = 8, Height = 8 };
        dot.SetResourceReference(System.Windows.Shapes.Ellipse.FillProperty, "SuccessBrush");

        dotStack.Children.Add(dot);

        if (!isLast)
        {
            var line = new Border
            {
                Width = 1,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 3, 0, 0)
            };
            line.SetResourceReference(Border.BackgroundProperty, "BorderDefaultBrush");
            dotStack.Children.Add(line);
        }

        Grid.SetColumn(dotStack, 0);

        // Text
        var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Top };

        var taskTitle = new TextBlock
        {
            Text = session.TaskTitle,
            FontSize = 13
        };
        taskTitle.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var sub = new TextBlock
        {
            Text = $"{session.FormattedTime} · {session.LastSessionDate:MMM dd}",
            FontSize = 11,
            Margin = new Thickness(0, 2, 0, 0)
        };
        sub.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");

        textStack.Children.Add(taskTitle);
        textStack.Children.Add(sub);
        Grid.SetColumn(textStack, 1);

        outer.Children.Add(dotStack);
        outer.Children.Add(textStack);

        return outer;
    }

    // ============================
    // HELPERS
    // ============================
    private string _FormatSeconds(int totalSeconds)
    {
        if (totalSeconds <= 0) return "0s";
        if (totalSeconds < 60) return $"{totalSeconds}s";
        int m = totalSeconds / 60;
        if (m < 60) return $"{m}m";
        int h = m / 60;
        int rem = m % 60;
        return rem == 0 ? $"{h}h" : $"{h}h {rem}m";
    }

    private Geometry? _ApplyCategoryIcon(string? iconName)
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
                { "database",   "IconDatabase" },
                { "user",       "IconUser"     },
            };

        if (map.TryGetValue(iconName!, out var key))
            return TryFindResource(key) as Geometry;

        return TryFindResource("IconFolder") as Geometry; // fallback
    }

    private void _ApplyCategoryColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return;
        try
        {
            var brush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));

            IconBg.Background = brush;
        }
        catch { }
    }

    

    // ============================
    // BUTTON HANDLERS
    // ============================
    private void BackBtn_Click(object sender, RoutedEventArgs e)
        => BackRequested?.Invoke(this, EventArgs.Empty);

    private void EditBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_projectService is null) return;

        var window = new AddEditProject(_projectService.ProjectID);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();

        if (window.IsSaved)
            _Load();
    }

    private void DoneBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_projectService is null) return;

        var confirm = MessageBox.Show(
            $"Mark \"{_projectService.Title}\" as completed?",
            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var result = ProjectService.Complete(_projectId);

        if (result == Service.Enums.Project.enProjectCompleteResult.Completed)
        {
            DoneBtn.Visibility = Visibility.Collapsed;
            _Load();
        }
        else
        {
            MessageBox.Show("Failed to complete project.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}