using Repository.Models;
using Service.Enums.Task;
using Service.Services;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TaskManagerUI.Controls.Inputs;

namespace TaskManagerUI.Pages.Tasks;

public partial class AddEditTask : Window
{
    // ============================
    // FIELDS
    // ============================
    private TaskService.enMode _formMode = TaskService.enMode.AddNew;
    private TaskService _taskService = new TaskService();
    private bool _isLoadingForm = false;
    public bool IsSaved { get; private set; } = false;

    // ============================
    // CONSTRUCTORS
    // ============================
    public AddEditTask()
    {
        InitializeComponent();
        _formMode = TaskService.enMode.AddNew;
    }

    public AddEditTask(int taskId)
    {
        InitializeComponent();
        _formMode = TaskService.enMode.Update;
        Tag = taskId;
    }

    // ============================
    // WINDOW EVENTS
    // ============================
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoadingForm = true;
        _ResetForm();

        if (_formMode == TaskService.enMode.Update && Tag is int taskId)
            _LoadData(taskId);

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
        if (_formMode == TaskService.enMode.AddNew)
        {
            _taskService = new TaskService();
            WindowTitle.Text = "Add Task";
            FormTitle.Text = "Add New Task";
            FormSubtitle.Text = "Fill out the fields below to create a new task.";
            TaskTitle.Text = string.Empty;
            TaskDescription.Text = string.Empty;
            TaskEstimatedMinutes.Text = string.Empty;
            TaskDueDate.Clear();
            TaskPriority.SelectedIndex = 1; // Medium default

            // AddNew mode — show project combo + status badge, hide update-only panels
            ProjectPanel.Visibility = Visibility.Visible;
            StatusBadgePanel.Visibility = Visibility.Visible;
            ProjectReadOnlyPanel.Visibility = Visibility.Collapsed;
            StatusComboPanel.Visibility = Visibility.Collapsed;

            _LoadProjects();
        }
        else
        {
            WindowTitle.Text = "Edit Task";
            FormTitle.Text = "Edit Task";
            FormSubtitle.Text = "Update the task information below.";

            // Update mode — show read-only project + status combo, hide addnew-only panels
            ProjectPanel.Visibility = Visibility.Collapsed;
            StatusBadgePanel.Visibility = Visibility.Collapsed;
            ProjectReadOnlyPanel.Visibility = Visibility.Visible;
            StatusComboPanel.Visibility = Visibility.Visible;
        }

        HideMessages();
    }

    private void _LoadProjects()
    {
        var (result, projects) = ProjectService.GetLookup();

        TaskProject.Items.Clear();

        if (result != Service.Enums.Project.enProjectRetrieveResult.Found || projects.Count == 0)
            return;

        foreach (var project in projects)
        {
            var item = new ComboBoxItem
            {
                Content = project.Title,
                Tag = project.ProjectId
            };
            item.Style = (Style)FindResource("ModernComboBoxItem");
            TaskProject.Items.Add(item);
        }

        TaskProject.SelectedIndex = 0;
    }

    private void _LoadData(int taskId)
    {
        var (result, service) = TaskService.Find(taskId);

        if (result != enTaskRetrieveResult.Found || service is null)
        {
            MessageBox.Show("Task not found.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        _taskService = service;
        TaskTitle.Text = service.Title;
        TaskDescription.Text = service.Description ?? string.Empty;
        TaskEstimatedMinutes.Text = service.EstimatedMinutes?.ToString() ?? string.Empty;
        TaskDueDate.SelectedDate = service.DueDate;

        // Read-only project name
        var (_, projectService) = ProjectService.Find(service.ProjectID);
        ProjectReadOnlyText.Text = projectService?.Title ?? string.Empty;

        // Priority
        foreach (ComboBoxItem item in TaskPriority.Items)
            if (item.Content?.ToString() == service.Priority)
            { TaskPriority.SelectedItem = item; break; }

        // Status
        foreach (ComboBoxItem item in TaskStatus.Items)
            if (item.Content?.ToString() == service.Status)
            { TaskStatus.SelectedItem = item; break; }
    }

    // ============================
    // LIVE VALIDATION
    // ============================
    private void TaskTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoadingForm) return;

        var control = sender as ModernInput;
        if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

        int projectId = _GetSelectedProjectId();
        if (projectId <= 0) return;

        control.Validate(live: true, externalValidator: text =>
        {
            text = text.Trim();

            bool exists = _formMode == TaskService.enMode.Update
                ? TaskService.IsNameTakenByOther(_taskService.TaskID, projectId, text)
                : TaskService.IsNameTaken(projectId, text);

            return exists ? "A task with this title already exists in this project." : null!;
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
        _taskService.Title = TaskTitle.Text.Trim();
        _taskService.Description = string.IsNullOrWhiteSpace(TaskDescription.Text)
                                            ? null
                                            : TaskDescription.Text.Trim();
        _taskService.Priority = (TaskPriority.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Medium";
        _taskService.DueDate = TaskDueDate.SelectedDate;
        _taskService.EstimatedMinutes = int.TryParse(TaskEstimatedMinutes.Text, out int mins)
                                            ? mins
                                            : null;

        if (_formMode == TaskService.enMode.AddNew)
            _taskService.ProjectID = _GetSelectedProjectId();
        else
            _taskService.Status = (TaskStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todo";

        var result = _taskService.Save();

        switch (result)
        {
            case enTaskSaveResult.Saved:
                PlaySound("success.wav");
                _formMode = TaskService.enMode.Update;
                IsSaved = true;

                // switch UI to update mode
                WindowTitle.Text = "Edit Task";
                FormTitle.Text = "Edit Task";
                FormSubtitle.Text = "Update the task information below.";

                ProjectPanel.Visibility = Visibility.Collapsed;
                StatusBadgePanel.Visibility = Visibility.Collapsed;
                ProjectReadOnlyPanel.Visibility = Visibility.Visible;
                StatusComboPanel.Visibility = Visibility.Visible;

                var (_, projectService) = ProjectService.Find(_taskService.ProjectID);
                ProjectReadOnlyText.Text = projectService?.Title ?? string.Empty;

                foreach (ComboBoxItem item in TaskStatus.Items)
                    if (item.Content?.ToString() == _taskService.Status)
                    { TaskStatus.SelectedItem = item; break; }

                ShowSuccessMessage("Task saved successfully.");
                break;

            case enTaskSaveResult.Failed:
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
        _formMode = TaskService.enMode.AddNew;

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
        TaskTitle.ValidateForce();
        if (!TaskTitle.IsValid)
        {
            errors.Add($"• {TaskTitle.ValidationMessageText}");
            result.FirstInvalidControl ??= TaskTitle;
        }

        int projectId = _formMode == TaskService.enMode.AddNew
            ? _GetSelectedProjectId()
            : _taskService.ProjectID;

        if (projectId > 0)
        {
            TaskTitle.Validate(live: false, externalValidator: text =>
            {
                bool exists = _formMode == TaskService.enMode.Update
                    ? TaskService.IsNameTakenByOther(_taskService.TaskID, projectId, text.Trim())
                    : TaskService.IsNameTaken(projectId, text.Trim());

                return exists ? "A task with this title already exists in this project." : null!;
            });

            if (!TaskTitle.IsValid)
                result.FirstInvalidControl ??= TaskTitle;
        }

        // Project required in AddNew
        if (_formMode == TaskService.enMode.AddNew && projectId <= 0)
        {
            errors.Add("• Please select a project.");
            result.FirstInvalidControl ??= TaskProject;
        }

        // Priority required
        if (TaskPriority.SelectedItem == null)
        {
            errors.Add("• Please select a priority.");
            result.FirstInvalidControl ??= TaskPriority;
        }

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
    private int _GetSelectedProjectId()
    {
        if (TaskProject.SelectedItem is ComboBoxItem item && item.Tag is int id)
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