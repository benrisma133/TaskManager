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
    private void CategoriesPage_Loaded(object sender, RoutedEventArgs e)
        => LoadCategories();

    // ============================
    // LOAD CATEGORIES
    // ============================
    private async void LoadCategories()
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
        RenderCards(_allCategories);
    }

    // ============================
    // RENDER CARDS
    // ============================
    private void RenderCards(List<Category> categories)
    {
        CardsPanel.Children.Clear();
        HideSkeleton();

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
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;

        var query = SearchBox.Text.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(query)
            ? _allCategories
            : _allCategories.Where(c =>
                c.Name.ToLower().Contains(query) ||
                c.Type.ToLower().Contains(query)).ToList();

        RenderCards(filtered);
    }

    // ============================
    // ADD
    // ============================
    private void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddEditCategory();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        if (window.IsSaved) LoadCategories();
    }

    // ============================
    // EDIT
    // ============================
    private void Card_OnEdit(int categoryId)
    {
        var window = new AddEditCategory(categoryId);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        if (window.IsSaved) LoadCategories();
    }

    // ============================
    // DELETE
    // ============================
    private void Card_OnDelete(int categoryId, string categoryName)
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
                LoadCategories();
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