using Repository.Models;
using Repository.Repositories;
using Service.Enums.Project;
using Service.Enums.Task;

namespace Service.Services;

public class TaskService
{
    // ─── enMode ────────────────────────────────────────────────────────────
    public enum enMode { AddNew, Update }
    private enMode _Mode;

    // ─── Properties ────────────────────────────────────────────────────────
    public int TaskID { get; private set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProjectID { get; set; }
    public ProjectService? Project { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateOnly? DueDate { get; set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? EstimatedMinutes { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ─── Computed Properties ───────────────────────────────────────────────
    public string EstimatedText => EstimatedMinutes.HasValue
        ? EstimatedMinutes.Value switch
        {
            < 60 => $"{EstimatedMinutes}m",
            _ => $"{EstimatedMinutes.Value / 60}h {EstimatedMinutes.Value % 60}m"
        }
        : "No estimate";

    public bool IsOverdue => DueDate.HasValue
        && DateOnly.FromDateTime(DateTime.Today) > DueDate.Value
        && !IsCompleted;

    public int DaysLeft => DueDate.HasValue
        ? DueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber
        : 0;

    public string DaysLeftText => DueDate.HasValue
        ? DaysLeft switch
        {
            < 0 => $"{Math.Abs(DaysLeft)}d overdue",
            0 => "Due today",
            1 => "Due tomorrow",
            _ => $"{DaysLeft}d left"
        }
        : "No due date";

    // ─── Constructors ──────────────────────────────────────────────────────
    public TaskService()
    {
        _Mode = enMode.AddNew;
        Status = "Todo";
    }

    public TaskService(TaskItem task, enMode mode = enMode.AddNew)
    {
        TaskID = task.TaskID;
        Title = task.Title;
        Description = task.Description;
        ProjectID = task.ProjectID;
        Priority = task.Priority;
        Status = task.Status;
        DueDate = task.DueDate;
        IsCompleted = task.IsCompleted;
        CompletedAt = task.CompletedAt;
        EstimatedMinutes = task.EstimatedMinutes;
        CreatedAt = task.CreatedAt;
        UpdatedAt = task.UpdatedAt;
        _Mode = mode;

        // TODO: load related project when needed
         var (result, projectService) = ProjectService.Find(task.ProjectID);
        if (result == enProjectRetrieveResult.Found)
            Project = projectService;
    }

    // ─── Private: AddNew ───────────────────────────────────────────────────
    private enTaskSaveResult _AddNew()
    {
        try
        {
            var task = new TaskItem
            {
                Title = Title,
                Description = Description,
                ProjectID = ProjectID,
                Priority = Priority,
                Status = Status,
                DueDate = DueDate,
                EstimatedMinutes = EstimatedMinutes
            };

            int newId = TaskRepository.AddTask(task);

            if (newId > 0)
            {
                TaskID = newId;
                _Mode = enMode.Update;
                return enTaskSaveResult.Saved;
            }

            return enTaskSaveResult.Failed;
        }
        catch
        {
            return enTaskSaveResult.Failed;
        }
    }

    // ─── Private: Update ───────────────────────────────────────────────────
    private enTaskSaveResult _Update()
    {
        try
        {
            var task = new TaskItem
            {
                TaskID = TaskID,
                Title = Title,
                Description = Description,
                ProjectID = ProjectID,
                Priority = Priority,
                Status = Status,
                DueDate = DueDate,
                EstimatedMinutes = EstimatedMinutes
            };

            bool updated = TaskRepository.UpdateTask(task);
            return updated ? enTaskSaveResult.Saved : enTaskSaveResult.Failed;
        }
        catch
        {
            return enTaskSaveResult.Failed;
        }
    }

    // ─── Public: Save ──────────────────────────────────────────────────────
    public enTaskSaveResult Save()
    {
        switch (_Mode)
        {
            case enMode.AddNew: return _AddNew();
            case enMode.Update: return _Update();
            default: return enTaskSaveResult.Failed;
        }
    }

    // ─── Static: Delete (Hard) ─────────────────────────────────────────────
    public static enTaskDeleteResult Delete(int taskId)
    {
        try
        {
            TaskRepository.DeleteTask(taskId);
            return enTaskDeleteResult.Deleted;
        }
        catch
        {
            return enTaskDeleteResult.Failed;
        }
    }

    // ─── Static: SoftDelete ────────────────────────────────────────────────
    public static enTaskDeleteResult SoftDelete(int taskId)
    {
        try
        {
            string result = TaskRepository.SoftDeleteTask(taskId);

            return result switch
            {
                "DELETED" => enTaskDeleteResult.Deleted,
                "ALREADY_COMPLETED" => enTaskDeleteResult.AlreadyCompleted,
                "HAS_SESSIONS" => enTaskDeleteResult.HasSessions,
                _ => enTaskDeleteResult.Failed
            };
        }
        catch
        {
            return enTaskDeleteResult.Failed;
        }
    }

    // ─── Static: Find ──────────────────────────────────────────────────────
    public static (enTaskRetrieveResult result, TaskService? service) Find(int taskId)
    {
        try
        {
            TaskItem? task = TaskRepository.GetTaskById(taskId);

            if (task is null)
                return (enTaskRetrieveResult.NotFound, null);

            return (enTaskRetrieveResult.Found, new TaskService(task, enMode.Update));
        }
        catch
        {
            return (enTaskRetrieveResult.Failed, null);
        }
    }

    // ─── Static: GetAll Paged ──────────────────────────────────────────────
    public static (enTaskRetrieveResult result, List<TaskItemDetails> tasks, int totalCount) GetAll(
    int pageNumber = 1,
    int pageSize = 9,
    string? search = null,
    string? priority = null,
    string? status = null)
    {
        try
        {
            var (tasks, totalCount) = TaskRepository.GetAllTasks(
                pageNumber, pageSize, search, priority, status);
            return (enTaskRetrieveResult.Found, tasks, totalCount);
        }
        catch
        {
            return (enTaskRetrieveResult.Failed, new List<TaskItemDetails>(), 0);
        }
    }

    // ─── Static: GetByProject ──────────────────────────────────────────────
    public static (enTaskRetrieveResult result, List<TaskItemDetails> tasks) GetByProject(int projectId)
    {
        try
        {
            var list = TaskRepository.GetTasksByProject(projectId);
            return (enTaskRetrieveResult.Found, list);
        }
        catch
        {
            return (enTaskRetrieveResult.Failed, new List<TaskItemDetails>());
        }
    }

    // ─── Static: IsNameTaken (Add) ─────────────────────────────────────────
    public static bool IsNameTaken(int projectId, string title)
    {
        try
        {
            return TaskRepository.IsTaskNameTaken(projectId, title);
        }
        catch
        {
            return false;
        }
    }

    // ─── Static: IsNameTakenByOther (Update) ───────────────────────────────
    public static bool IsNameTakenByOther(int taskId, int projectId, string title)
    {
        try
        {
            return TaskRepository.IsTaskNameTakenByOther(taskId, projectId, title);
        }
        catch
        {
            return false;
        }
    }

}