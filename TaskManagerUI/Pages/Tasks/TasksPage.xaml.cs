using Repository.Models;
using Service.Enums.Task;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TaskManagerUI.Controls.Cards;
using TaskManagerUI.Models;
using TaskManagerUI.Pages.Tasks.Timer;

namespace TaskManagerUI.Pages.Tasks;

public partial class TasksPage : UserControl
{
    // ============================
    // FIELDS
    // ============================
    private List<TaskItemDetails> _allTasks = new();
    private bool _isInitialized = false;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private const int PageSize = 9;
    private MainWindow? _mainWindow;

    // ============================
    // CONSTRUCTOR
    // ============================
    public TasksPage()
    {
        InitializeComponent();
    }

    // ============================
    // PAGE LOADED
    // ============================
    private async void TasksPage_Loaded(object sender, RoutedEventArgs e)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        _isInitialized = true;
        await LoadTasks();
    }

    // ============================
    // LOAD TASKS
    // ============================
    private async Task LoadTasks()
    {
        ShowSkeleton();

        var search = string.IsNullOrWhiteSpace(SearchBox.Text) ? null : SearchBox.Text.Trim();
        var priority = (PriorityFilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var status = (StatusFilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

        // pass null for "All" — SP treats NULL as no filter
        if (priority == "All") priority = null;
        if (status == "All") status = null;

        var (result, tasks, totalCount) = await Task.Run(()
            => TaskService.GetAll(_currentPage, PageSize, search, priority, status));

        if (result == enTaskRetrieveResult.Failed)
        {
            HideSkeleton();
            MessageBox.Show("Failed to load tasks. Please try again.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _allTasks = tasks;
        _totalCount = totalCount;
        _totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

        RenderCards(_allTasks);
        UpdateFooter();
    }

    // ============================
    // SEARCH & FILTER
    // ============================
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isInitialized) return;

        SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;

        _currentPage = 1; // reset to page 1 on new search
        _ = LoadTasks();
    }

    private void PriorityFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _currentPage = 1;
        _ = LoadTasks();
    }

    private void StatusFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _currentPage = 1;
        _ = LoadTasks();
    }

    // ============================
    // RENDER CARDS
    // ============================
    private void RenderCards(List<TaskItemDetails> tasks)
    {
        CardsPanel.Children.Clear();
        HideSkeleton();

        TotalCountText.Text = _totalCount.ToString();

        if (tasks.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            CardsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyState.Visibility = Visibility.Collapsed;
        CardsPanel.Visibility = Visibility.Visible;

        foreach (var task in tasks)
        {
            var card = new TaskCard();
            card.LoadTask(task);
            card.Margin = new Thickness(8);
            card.Width = double.NaN;
            card.HorizontalAlignment = HorizontalAlignment.Stretch;

            card.EditBtn.Click += (s, e) => Card_OnEdit(task.TaskID);
            card.DeleteBtn.Click += (s, e) => Card_OnDelete(task.TaskID, task.Title);
            card.StartTaskBtn.Click += (s, e) => Card_OnStartTask(task.TaskID);

            CardsPanel.Children.Add(card);
        }

        UpdateColumns();
    }

    // ============================
    // FOOTER & PAGINATION
    // ============================
    private void UpdateFooter()
    {
        PrevBtn.IsEnabled = _currentPage > 1;
        NextBtn.IsEnabled = _currentPage < _totalPages;

        FilterResultText.Text = $"Page {_currentPage} of {_totalPages}";

        _BuildPageNumbers();
    }

    private void _BuildPageNumbers()
    {
        PageNumbersPanel.Children.Clear();

        for (int i = 1; i <= _totalPages; i++)
        {
            int pageNum = i;
            bool isCurrent = pageNum == _currentPage;

            var btn = new Button
            {
                Width = 32,
                Height = 32,
                Content = pageNum.ToString(),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(3, 0, 3, 0),
                FontSize = 13,
                FontWeight = isCurrent ? FontWeights.Bold : FontWeights.Normal
            };

            // style
            btn.Background = isCurrent
                ? (Brush)FindResource("AccentBrush")
                : (Brush)FindResource("InputBackgroundBrush");
            btn.Foreground = isCurrent
                ? (Brush)FindResource("TextOnAccentBrush")
                : (Brush)FindResource("TextSecondaryBrush");
            btn.BorderBrush = isCurrent
                ? (Brush)FindResource("AccentBrush")
                : (Brush)FindResource("BorderDefaultBrush");

            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty,
                new System.Windows.Data.Binding("BorderThickness")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            btn.Template = template;

            btn.Click += async (s, e) =>
            {
                _currentPage = pageNum;
                await LoadTasks();
            };

            PageNumbersPanel.Children.Add(btn);
        }
    }

    private async void PrevBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage <= 1) return;
        _currentPage--;
        await LoadTasks();
    }

    private async void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage >= _totalPages) return;
        _currentPage++;
        await LoadTasks();
    }

    // ============================
    // RESPONSIVE COLUMNS
    // ============================
    private void UpdateColumns()
    {
        double width = ActualWidth;
        int columns = width > 1100 ? 3
                     : width > 750 ? 2
                     : 1;

        CardsPanel.Columns = columns;
        SkeletonPanel.Columns = columns;
    }

    private void TasksPage_SizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateColumns();

    // ============================
    // ADD
    // ============================
    private async void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddEditTask();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        if (window.IsSaved)
        {
            _currentPage = 1;
            await LoadTasks();
        }
    }

    // ============================
    // EDIT
    // ============================
    private async void Card_OnEdit(int taskId)
    {
        var window = new AddEditTask(taskId);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        if (window.IsSaved) await LoadTasks();
    }

    // ============================
    // DELETE
    // ============================
    private async void Card_OnDelete(int taskId, string taskTitle)
    {
        var confirm = MessageBox.Show(
            $"Are you sure you want to delete \"{taskTitle}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = TaskService.SoftDelete(taskId);

        switch (result)
        {
            case enTaskDeleteResult.Deleted:
                _currentPage = 1;
                await LoadTasks();
                break;

            case enTaskDeleteResult.AlreadyCompleted:
                MessageBox.Show(
                    "Completed tasks cannot be deleted.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                break;

            case enTaskDeleteResult.HasSessions:
                MessageBox.Show(
                    $"\"{taskTitle}\" has timer sessions and cannot be deleted.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                break;

            case enTaskDeleteResult.Failed:
                MessageBox.Show(
                    "Failed to delete task. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                break;
        }
    }


    // ============================
    // START TASK
    // ============================
    private void Card_OnStartTask(int taskId)
    {
        // ── just navigate, don't start session yet ────────────────
        _mainWindow?.NavigateToTimer(taskId);
    }

    // ============================
    // SKELETON HELPERS
    // ============================
    private void ShowSkeleton()
    {
        SkeletonPanel.Visibility = Visibility.Visible;
        CardsPanel.Visibility = Visibility.Collapsed;
        EmptyState.Visibility = Visibility.Collapsed;
    }

    private void HideSkeleton()
    {
        SkeletonPanel.Visibility = Visibility.Collapsed;
    }
}