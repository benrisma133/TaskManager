using Repository.Models;
using Service.Enums.Project;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TaskManagerUI.Controls.Inputs;

namespace TaskManagerUI.Pages.Projects;

public partial class AddEditProject : Window
{
    // ============================
    // FIELDS
    // ============================
    private ProjectService.enMode _formMode = ProjectService.enMode.AddNew;
    private ProjectService _projectService = new ProjectService();
    private bool _isLoadingForm = false;
    public bool IsSaved { get; private set; } = false;

    // ============================
    // CONSTRUCTORS
    // ============================
    public AddEditProject()
    {
        InitializeComponent();
        _formMode = ProjectService.enMode.AddNew;
    }

    public AddEditProject(int projectId)
    {
        InitializeComponent();
        _formMode = ProjectService.enMode.Update;
        Tag = projectId;
    }

    // ============================
    // WINDOW EVENTS
    // ============================
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoadingForm = true;
        _LoadCategories();
        _ResetForm();

        if (_formMode == ProjectService.enMode.Update && Tag is int projectId)
            _LoadData(projectId);

        _isLoadingForm = false;
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    // ============================
    // LOAD CATEGORIES
    // ============================
    private void _LoadCategories()
    {
        var (result, categories) = CategoryService.GetAll();

        ProjectCategory.Items.Clear();

        if (result != Service.Enums.Category.enCategoryRetrieveResult.Found || categories.Count == 0)
            return;

        foreach (var category in categories)
        {
            var item = new ComboBoxItem
            {
                Content = category.Name,
                Tag = category.CategoryId
            };
            item.Style = (Style)FindResource("ModernComboBoxItem");
            ProjectCategory.Items.Add(item);
        }

        ProjectCategory.SelectedIndex = 0;
    }

    // ============================
    // LOAD & RESET
    // ============================
    private void _ResetForm()
    {
        if (_formMode == ProjectService.enMode.AddNew)
        {
            _projectService = new ProjectService();
            WindowTitle.Text = "Add Project";
            FormTitle.Text = "Add New Project";
            FormSubtitle.Text = "Fill out the fields below to create a new project.";
            ProjectTitle.Text = string.Empty;
            ProjectDescription.Text = string.Empty;
            ProjectPriority.SelectedIndex = 1; // Medium
            ProjectStatus.SelectedIndex = 0; // Active
            ProjectStartDate.Clear();
            ProjectDueDate.Clear();
        }
        else
        {
            WindowTitle.Text = "Edit Project";
            FormTitle.Text = "Edit Project";
            FormSubtitle.Text = "Update the project information below.";
        }

        HideMessages();
    }

    private void _LoadData(int projectId)
    {
        var (result, service) = ProjectService.Find(projectId);

        if (result != enProjectRetrieveResult.Found || service is null)
        {
            MessageBox.Show("Project not found.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        _projectService = service;
        ProjectTitle.Text = service.Title;
        ProjectDescription.Text = service.Description ?? string.Empty;
        ProjectStartDate.SelectedDate = service.StartDate;
        ProjectDueDate.SelectedDate = service.DueDate;

        // Category
        foreach (ComboBoxItem item in ProjectCategory.Items)
            if (item.Tag is int id && id == service.CategoryID)
            { ProjectCategory.SelectedItem = item; break; }

        // Priority
        foreach (ComboBoxItem item in ProjectPriority.Items)
            if (item.Content?.ToString() == service.Priority)
            { ProjectPriority.SelectedItem = item; break; }

        // Status
        foreach (ComboBoxItem item in ProjectStatus.Items)
            if (item.Content?.ToString() == service.Status)
            { ProjectStatus.SelectedItem = item; break; }
    }

    // ============================
    // LIVE VALIDATION
    // ============================
    private void ProjectTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoadingForm) return;

        var control = sender as ModernInput;
        if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

        control.Validate(live: true, externalValidator: text =>
        {
            text = text.Trim();

            bool exists = _formMode == ProjectService.enMode.Update
                ? ProjectService.IsNameTakenByOther(_projectService.ProjectID, text)
                : ProjectService.IsNameTaken(text);

            return exists ? "A project with this title already exists." : null!;
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
            if (validation.Errors != null && validation.FirstInvalidControl != null)
            {
                PlaySound("error.wav");
                ShowErrorMessage(validation.Errors);
                ScrollToFirstError(validation.FirstInvalidControl);
                return;
            }
        }

        _ProcessFormData();
    }

    private void _ProcessFormData()
    {
        _projectService.Title = ProjectTitle.Text.Trim();
        _projectService.Description = string.IsNullOrWhiteSpace(ProjectDescription.Text)
                                        ? null
                                        : ProjectDescription.Text.Trim();
        _projectService.CategoryID = _GetSelectedCategoryId();
        _projectService.Priority = (ProjectPriority.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
        _projectService.Status = (ProjectStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";
        _projectService.StartDate = ProjectStartDate.SelectedDate;
        _projectService.DueDate = ProjectDueDate.SelectedDate;

        var result = _projectService.Save();

        switch (result)
        {
            case enProjectSaveResult.Saved:
                PlaySound("success.wav");
                _formMode = ProjectService.enMode.Update;
                IsSaved = true;
                WindowTitle.Text = "Edit Project";
                FormTitle.Text = "Edit Project";
                FormSubtitle.Text = "Update the project information below.";
                ShowSuccessMessage("Project saved successfully.");
                break;

            case enProjectSaveResult.DuplicateName:
                PlaySound("error.wav");
                ShowErrorMessage(new List<string> { "• A project with this title already exists." });
                break;

            case enProjectSaveResult.Failed:
                PlaySound("error.wav");
                ShowErrorMessage(new List<string> { "• Failed to save. Please try again." });
                break;
        }
    }

    // ============================
    // CLEAR
    // ============================
    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _formMode = ProjectService.enMode.AddNew;
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

        // Title
        ProjectTitle.ValidateForce();
        if (!ProjectTitle.IsValid)
        {
            errors.Add($"• {ProjectTitle.ValidationMessageText}");
            result.FirstInvalidControl ??= ProjectTitle;
        }

        ProjectTitle.Validate(live: false, externalValidator: text =>
        {
            bool exists = _formMode == ProjectService.enMode.Update
                ? ProjectService.IsNameTakenByOther(_projectService.ProjectID, text.Trim())
                : ProjectService.IsNameTaken(text.Trim());

            return exists ? "A project with this title already exists." : null!;
        });

        if (!ProjectTitle.IsValid)
            result.FirstInvalidControl ??= ProjectTitle;

        // Category required
        if (_GetSelectedCategoryId() <= 0)
        {
            errors.Add("• Please select a category.");
            result.FirstInvalidControl ??= ProjectCategory;
        }

        // Due date must be after start date
        //if (ProjectStartDate.SelectedDate.HasValue &&
        //    ProjectDueDate.SelectedDate.HasValue &&
        //    ProjectDueDate.SelectedDate.Value <= ProjectStartDate.SelectedDate.Value)
        //{
        //    errors.Add("• Due date must be after start date.");
        //    result.FirstInvalidControl ??= ProjectDueDate;
        //}

        if (errors.Any())
        {
            result.IsValid = false;
            result.Errors = errors;
        }

        return result;
    }

    // ============================
    // HELPERS
    // ============================
    private int _GetSelectedCategoryId()
    {
        if (ProjectCategory.SelectedItem is ComboBoxItem item && item.Tag is int id)
            return id;
        return 0;
    }

    private void PlaySound(string soundName)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/TaskManagerUI;component/Assets/Sounds/{soundName}");
            var info = Application.GetResourceStream(uri);
            var player = new SoundPlayer(info.Stream);
            player.Play();
        }
        catch { }
    }

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