using Repository.Models;
using Service.Enums.Category;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskManagerUI.Controls.Cards;
using TaskManagerUI.Pages.Categories;

namespace TaskManagerUI.Pages.Categories;

public partial class CategoriesPage : UserControl
{
    // ============================
    // FIELDS
    // ============================
    private List<Category> _allCategories = new();
    private List<Category>? _filtredCategories = new();
    private bool _isInitialized = false;

    // ============================
    // CONSTRUCTOR
    // ============================
    public CategoriesPage()
    {
        InitializeComponent();
    }

    // ============================
    // PAGE LOADED
    // ============================
    private async void CategoriesPage_Loaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        await LoadCategories();
    }
    

    // ============================
    // LOAD CATEGORIES
    // ============================
    private async Task LoadCategories()
    {
        ShowSkeleton();

        var (result, categories) = await Task.Run(() => CategoryService.GetAll());

        if (result == enCategoryRetrieveResult.Failed)
        {
            HideSkeleton();
            MessageBox.Show("Failed to load categories. Please try again.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _allCategories = categories;
        //RenderCards(_allCategories);
        ApplyFilters();
    }

    // ============================
    // RENDER CARDS
    // ============================
    private void RenderCards(List<Category> categories)
    {
        if (CardsPanel == null) return;

        CardsPanel.Children.Clear();
        HideSkeleton();

        // ✅ Update total count
        TotalCountText.Text = categories.Count.ToString();

        // ✅ Update filter result text
        if (categories.Count == _allCategories.Count)
            FilterResultText.Text = "Showing all";
        else
            FilterResultText.Text = $"Filtered from {_allCategories.Count}";

        if (categories.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            CardsPanel.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyState.Visibility = Visibility.Collapsed;
        CardsPanel.Visibility = Visibility.Visible;

        foreach (var category in categories)
        {
            var card = new CategoryCard();
            card.LoadCategory(category);
            card.Margin = new Thickness(8);
            card.Width = double.NaN;
            card.HorizontalAlignment = HorizontalAlignment.Stretch;

            // wire up buttons
            card.EditBtn.Click += (s, e) => Card_OnEdit(category.CategoryId);
            card.DeleteBtn.Click += (s, e) => Card_OnDelete(category.CategoryId, category.Name);

            CardsPanel.Children.Add(card);
        }

        UpdateColumns();
    }

    // ============================
    // RESPONSIVE COLUMNS
    // ============================
    private void UpdateColumns()
    {
        double width = ActualWidth;
        int columns = width > 1100 ? 3
                     : width > 750 ? 3
                     : width > 450 ? 2
                     : 1;

        CardsPanel.Columns = columns;
        SkeletonPanel.Columns = columns;
    }

    private void CategoriesPage_SizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateColumns();

    // ============================
    // SEARCH
    // ============================
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isInitialized) return;

        SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;

        ApplyFilters();
    }

    private void TypeFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = SearchBox.Text.Trim().ToLower();
        var selectedType = (TypeFilterCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

        var filtered = _allCategories.Where(c =>
            (string.IsNullOrEmpty(query) || c.Name.ToLower().Contains(query)) &&
            (selectedType == "All Types" || c.Type == selectedType)
        ).ToList();


        RenderCards(filtered);
    }

    // ============================
    // ADD
    // ============================
    private async void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddEditCategory();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        
        if (window.IsSaved)  await LoadCategories();

    }

    // ============================
    // EDIT
    // ============================
    private async void Card_OnEdit(int categoryId)
    {
        var window = new AddEditCategory(categoryId);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        if (window.IsSaved) await LoadCategories();
    }

    // ============================
    // DELETE
    // ============================
    private async void Card_OnDelete(int categoryId, string categoryName)
    {
        var confirm = MessageBox.Show(
            $"Are you sure you want to delete \"{categoryName}\"?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = CategoryService.Delete(categoryId);

        switch (result)
        {
            case enCategoryDeleteResult.Deleted:
                await LoadCategories();
                break;

            case enCategoryDeleteResult.NotCustom:
                MessageBox.Show(
                    "Built-in categories cannot be deleted.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                break;

            case enCategoryDeleteResult.HasProjects:
                MessageBox.Show(
                    $"\"{categoryName}\" is used by one or more projects and cannot be deleted.",
                    "Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                break;

            case enCategoryDeleteResult.Failed:
                MessageBox.Show(
                    "Failed to delete category. Please try again.",
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