using Service.Enums.Category;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TaskManagerUI.Controls.Inputs;

namespace TaskManagerUI.Pages.Categories;

public partial class AddEditCategory : Window
{
    // ============================
    // FIELDS
    // ============================
    private CategoryService.enMode _formMode = CategoryService.enMode.AddNew;
    private CategoryService _categoryService = new CategoryService();
    private bool _isLoadingForm = false;
    public bool IsSaved { get; private set; } = false;

    // ============================
    // CONSTRUCTORS
    // ============================
    public AddEditCategory()
    {
        InitializeComponent();
        _formMode = CategoryService.enMode.AddNew;
    }

    public AddEditCategory(int categoryId)
    {
        InitializeComponent();
        _formMode = CategoryService.enMode.Update;
        Tag = categoryId; // carry the id to Window_Loaded
    }

    // ============================
    // WINDOW EVENTS
    // ============================
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoadingForm = true;
        _ResetForm();

        if (_formMode == CategoryService.enMode.Update && Tag is int categoryId)
            _LoadData(categoryId);

        _isLoadingForm = false;
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    // ============================
    // LOAD & RESET
    // ============================
    private void _ResetForm()
    {
        if (_formMode == CategoryService.enMode.AddNew)
        {
            _categoryService = new CategoryService();
            WindowTitle.Text = "Add Category";
            FormTitle.Text = "Add New Category";
            FormSubtitle.Text = "Fill out the fields below to create a new category.";
            CategoryName.Text = string.Empty;
            CategoryType.SelectedIndex = 0;
            CategoryIcon.SelectedIndex = 0;
            CategoryColor.SelectedColor = "#6366F1";
        }
        else
        {
            WindowTitle.Text = "Edit Category";
            FormTitle.Text = "Edit Category";
            FormSubtitle.Text = "Update the category information below.";
        }

        HideMessages();
    }

    private void _LoadData(int categoryId)
    {
        var (result, service) = CategoryService.Find(categoryId);

        if (result != enCategoryRetrieveResult.Found || service is null)
        {
            MessageBox.Show("Category not found.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        _categoryService = service;
        CategoryName.Text = service.Name;

        // Type
        foreach (ComboBoxItem item in CategoryType.Items)
            if (item.Content?.ToString() == service.Type)
            { CategoryType.SelectedItem = item; break; }

        // Icon
        foreach (ComboBoxItem item in CategoryIcon.Items)
            if (item.Content?.ToString() == service.Icon)
            { CategoryIcon.SelectedItem = item; break; }

        // Color
        CategoryColor.SelectedColor = service.Color;
    }

    // ============================
    // LIVE VALIDATION
    // ============================
    private void CategoryName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoadingForm) return;

        var control = sender as ModernInput;
        if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

        control.Validate(live: true, externalValidator: text =>
        {
            text = text.Trim();

            bool exists = _formMode == CategoryService.enMode.Update
                ? CategoryService.IsNameTakenByOther(_categoryService.CategoryId, text)
                : CategoryService.IsNameTaken(text);

            return exists ? "This category name already exists." : null!;
        });
    }

    // ============================
    // SAVE
    // ============================
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        HideMessages();

        var validation = _ValidateAllFields();

        if (!validation.IsValid)
        {
            if(validation.Errors != null && validation.FirstInvalidControl != null)
            {
                ShowErrorMessage(validation.Errors);
                ScrollToFirstError(validation.FirstInvalidControl);
                return;
            }
        }

        _ProcessFormData();
    }

    private void _ProcessFormData()
    {
        _categoryService.Name = CategoryName.Text.Trim();
        _categoryService.Type = (CategoryType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Project";
        _categoryService.Icon = (CategoryIcon.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "folder";
        _categoryService.Color = CategoryColor.SelectedColor;

        var result = _categoryService.Save();

        switch (result)
        {
            case enCategorySaveResult.Saved:
                _formMode = CategoryService.enMode.Update;
                IsSaved = true;
                WindowTitle.Text = "Edit Category";
                FormTitle.Text = "Edit Category";
                FormSubtitle.Text = "Update the category information below.";
                ShowSuccessMessage(_formMode == CategoryService.enMode.Update
                    ? "Category updated successfully."
                    : "Category added successfully.");
                break;

            case enCategorySaveResult.DuplicateName:
                ShowErrorMessage(new List<string> { "• This category name already exists." });
                break;

            case enCategorySaveResult.Failed:
                ShowErrorMessage(new List<string> { "• Failed to save. Please try again." });
                break;
        }
    }

    // ============================
    // CLEAR
    // ============================
    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _formMode = CategoryService.enMode.AddNew;

        _isLoadingForm = true;
        _ResetForm();
        _isLoadingForm = false;
    }

    // ============================
    // VALIDATION
    // ============================
    private ValidationResult _ValidateAllFields()
    {
        var result = new ValidationResult { IsValid = true };
        var errors = new List<string>();

        CategoryName.ValidateForce();
        if (!CategoryName.IsValid)
        {
            errors.Add($"• {CategoryName.ValidationMessageText}");
            if (result.FirstInvalidControl == null)
                result.FirstInvalidControl ??= CategoryName;
        }

        CategoryName.Validate(live: false, externalValidator: text =>
        {
            bool exists = _formMode == CategoryService.enMode.Update
                ? CategoryService.IsNameTakenByOther(_categoryService.CategoryId, text.Trim())
                : CategoryService.IsNameTaken(text.Trim());

            return exists ? "This category name already exists." : null!;
        });

        if (!CategoryName.IsValid && result.FirstInvalidControl == null)
            result.FirstInvalidControl ??= CategoryName;

        if (errors.Any())
        {
            result.IsValid = false;
            result.Errors = errors;
        }

        return result;
    }

    // ============================
    // UI HELPERS
    // ============================
    private void ShowErrorMessage(List<string> errors)
    {
        ErrorMessageText.Text = string.Join("\n", errors);
        ErrorMessageBox.Visibility = Visibility.Visible;

        ErrorMessageBox.BeginAnimation(OpacityProperty,
            new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            });

        MainScrollViewer.ScrollToTop();
    }

    private void ShowSuccessMessage(string message)
    {
        SuccessMessageText.Text = message;
        SuccessMessageBox.Visibility = Visibility.Visible;

        SuccessMessageBox.BeginAnimation(OpacityProperty,
            new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            });

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(8)
        };
        timer.Tick += (s, e) => { HideMessages(); timer.Stop(); };
        timer.Start();

        MainScrollViewer.ScrollToTop();
    }

    private void HideMessages()
    {
        ErrorMessageBox.Visibility = Visibility.Collapsed;
        SuccessMessageBox.Visibility = Visibility.Collapsed;
    }

    private void ScrollToFirstError(FrameworkElement control)
    {
        control?.BringIntoView();
    }

    // ============================
    // HELPER CLASS
    // ============================
    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string>? Errors { get; set; }
        public FrameworkElement? FirstInvalidControl { get; set; }
    }

}