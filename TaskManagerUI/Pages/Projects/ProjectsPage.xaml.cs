using Repository.Models;
using Service.Enums.Project;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Controls.Cards;

namespace TaskManagerUI.Pages.Projects;

public partial class ProjectsPage : UserControl
{
    // ============================
    // FIELDS
    // ============================
    private List<ProjectDetails> _allProjects = new();
    private List<Category> _allCategories = new();
    private bool _isInitialized = false;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private const int PageSize = 9;

    // ============================
    // CONSTRUCTOR
    // ============================
    public ProjectsPage()
    {
        InitializeComponent();
        _LoadCategories();
    }

    // ============================
    // PAGE LOADED
    // ============================
    private async void ProjectsPage_Loaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        await LoadProjects();
    }

    // ============================
    // LOAD CATEGORIES
    // ============================
    private void _LoadCategories()
    {
        var (result, categories) = CategoryService.GetAll();

        CategoryFilterCombo.Items.Clear();

        // ── "All" option ────────────────────────────────────────
        var allItem = new ComboBoxItem
        {
            Content = "All",
            Tag = null
        };
        allItem.Style = (Style)FindResource("ModernComboBoxItem");
        CategoryFilterCombo.Items.Add(allItem);

        if (result != Service.Enums.Category.enCategoryRetrieveResult.Found || categories.Count == 0)
        {
            CategoryFilterCombo.SelectedIndex = 0;
            return;
        }

        _allCategories = categories;

        foreach (var category in categories)
        {
            var item = new ComboBoxItem
            {
                Content = category.Name,
                Tag = category.CategoryId
            };
            item.Style = (Style)FindResource("ModernComboBoxItem");
            CategoryFilterCombo.Items.Add(item);
        }

        CategoryFilterCombo.SelectedIndex = 0;
    }

    // ============================
    // GET SELECTED CATEGORY ID
    // ============================
    private int? _GetSelectedCategoryId()
    {
        if (CategoryFilterCombo.SelectedItem is ComboBoxItem item)
        {
            if (item.Tag == null) return null; // "All"
            if (item.Tag is int id) return id;
        }
        return null;
    }

    // ============================
    // LOAD PROJECTS
    // ============================
    private async Task LoadProjects()
    {
        ShowSkeleton();

        var search = string.IsNullOrWhiteSpace(SearchBox.Text) ? null : SearchBox.Text.Trim();
        var priority = (PriorityFilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var status = (StatusFilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        int? categoryId = _GetSelectedCategoryId();

        // pass null for "All" — SP treats NULL as no filter
        if (priority == "All") priority = null;
        if (status == "All") status = null;

        // ═══ Delay to see shimmer animation ═══════════════════════════
        //await Task.Delay(1000); // 5 seconds

        var (result, projects, totalCount) = await Task.Run(()
            => ProjectService.GetAll(_currentPage, PageSize, search, priority, status, categoryId));

        if (result == enProjectRetrieveResult.Failed)
        {
            HideSkeleton();
            MessageBox.Show("Failed to load projects. Please try again.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _allProjects = projects;
        _totalCount = totalCount;
        _totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

        RenderCards(_allProjects);
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

        _currentPage = 1;
        _ = LoadProjects();
    }

    private void PriorityFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _currentPage = 1;
        _ = LoadProjects();
    }

    private void StatusFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _currentPage = 1;
        _ = LoadProjects();
    }

    private void CategoryFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _currentPage = 1;
        _ = LoadProjects();
    }

    private void FilterBtn_Click(object sender, RoutedEventArgs e)
    {
        FilterPopup.IsOpen = !FilterPopup.IsOpen;
    }

    private void CloseFilterPopupBtn_Click(object sender, RoutedEventArgs e)
    {
        FilterPopup.IsOpen = false;
    }

    private void ClearFiltersBtn_Click(object sender, RoutedEventArgs e)
    {
        PriorityFilterCombo.SelectedIndex = 0;  // "All"
        StatusFilterCombo.SelectedIndex = 0;  // "All"
        CategoryFilterCombo.SelectedIndex = 0;  // "All"
        SearchBox.Text = string.Empty;

        _currentPage = 1;
        _ = LoadProjects();
    }

    // ============================
    // RENDER CARDS
    // ============================
    private void RenderCards(List<ProjectDetails> projects)
    {
        CardsPanel.Children.Clear();
        HideSkeleton();

        TotalCountText.Text = _totalCount.ToString();

        if (projects.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            CardsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyState.Visibility = Visibility.Collapsed;
        CardsPanel.Visibility = Visibility.Visible;

        foreach (var project in projects)
        {
            var card = new ProjectCard();
            card.LoadProject(project);
            card.Margin = new Thickness(8);
            card.Width = double.NaN;
            card.HorizontalAlignment = HorizontalAlignment.Stretch;

            card.EditBtn.Click += (s, e) => Card_OnEdit(project.ProjectID);
            card.DeleteBtn.Click += (s, e) => Card_OnDelete(project.ProjectID, project.Title);

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
                FontWeight = isCurrent ? FontWeights.Bold : FontWeights.Normal,
                Background = isCurrent
                                    ? (Brush)FindResource("AccentBrush")
                                    : (Brush)FindResource("InputBackgroundBrush"),
                Foreground = isCurrent
                                    ? (Brush)FindResource("TextOnAccentBrush")
                                    : (Brush)FindResource("TextSecondaryBrush"),
                BorderBrush = isCurrent
                                    ? (Brush)FindResource("AccentBrush")
                                    : (Brush)FindResource("BorderDefaultBrush")
            };

            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(Border.BorderThicknessProperty,
                new System.Windows.Data.Binding("BorderThickness")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
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
                await LoadProjects();
            };

            PageNumbersPanel.Children.Add(btn);
        }
    }

    private async void PrevBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage <= 1) return;
        _currentPage--;
        await LoadProjects();
    }

    private async void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage >= _totalPages) return;
        _currentPage++;
        await LoadProjects();
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

    private void ProjectsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateColumns();

    // ============================
    // ADD
    // ============================
    private async void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddEditProject();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();

        if (window.IsSaved)
        {
            _currentPage = 1;
            await LoadProjects();
        }
    }

    // ============================
    // EDIT
    // ============================
    private async void Card_OnEdit(int projectId)
    {
        var window = new AddEditProject(projectId);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();

        if (window.IsSaved) await LoadProjects();
    }

    // ============================
    // DELETE
    // ============================
    private async void Card_OnDelete(int projectId, string projectTitle)
    {
        var confirm = MessageBox.Show(
            $"Are you sure you want to delete \"{projectTitle}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = ProjectService.SoftDelete(projectId);

        switch (result)
        {
            case enProjectDeleteResult.Deleted:
                _currentPage = 1;
                await LoadProjects();
                break;

            case enProjectDeleteResult.HasActiveTasks:
                MessageBox.Show(
                    $"\"{projectTitle}\" has active tasks and cannot be deleted.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                break;

            case enProjectDeleteResult.Failed:
                MessageBox.Show(
                    "Failed to delete project. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                break;
        }
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