using Repository.Models;
using Repository.Repositories;
using Service.Enums.Category;
using Service.Enums.Project;

namespace Service.Services;

public class ProjectService
{
    // ─── enMode ────────────────────────────────────────────────────────────
    public enum enMode { AddNew, Update }
    private enMode _Mode;

    // ─── Properties ────────────────────────────────────────────────────────
    public int ProjectID { get; private set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int CategoryID { get; set; }
    public Category? Category { get; private set; }
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ─── Computed Properties ───────────────────────────────────────────────
    public int DaysLeft => DueDate.HasValue
        ? DueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber
        : 0;

    public bool IsOverdue => DueDate.HasValue
        && DateOnly.FromDateTime(DateTime.Today) > DueDate.Value
        && Status != "Completed"
        && Status != "Archived";

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
    public ProjectService()
    {
        _Mode = enMode.AddNew;
        Status = "Active";
    }

    public ProjectService(Project project, enMode mode = enMode.AddNew)
    {
        ProjectID = project.ProjectID;
        Title = project.Title;
        Description = project.Description;
        CategoryID = project.CategoryID;
        Status = project.Status;
        Priority = project.Priority;
        StartDate = project.StartDate;
        DueDate = project.DueDate;
        CreatedAt = project.CreatedAt;
        UpdatedAt = project.UpdatedAt;
        _Mode = mode;

        // Composition — load category info
        var category = CategoryRepository.GetCategoryById(project.CategoryID);
        if (category is not null)
            Category = category;
    }

    // ─── Private: AddNew ───────────────────────────────────────────────────
    private enProjectSaveResult _AddNew()
    {
        try
        {
            if (ProjectRepository.IsProjectNameTaken(Title))
                return enProjectSaveResult.DuplicateName;

            var project = new Project
            {
                Title = Title,
                Description = Description,
                CategoryID = CategoryID,
                Priority = Priority,
                Status = Status,
                StartDate = StartDate,
                DueDate = DueDate
            };

            int newId = ProjectRepository.AddProject(project);

            if (newId > 0)
            {
                ProjectID = newId;
                _Mode = enMode.Update;
                return enProjectSaveResult.Saved;
            }

            return enProjectSaveResult.Failed;
        }
        catch
        {
            return enProjectSaveResult.Failed;
        }
    }

    // ─── Private: Update ───────────────────────────────────────────────────
    private enProjectSaveResult _Update()
    {
        try
        {
            if (ProjectRepository.IsProjectNameTakenByOther(ProjectID, Title))
                return enProjectSaveResult.DuplicateName;

            var project = new Project
            {
                ProjectID = ProjectID,
                Title = Title,
                Description = Description,
                CategoryID = CategoryID,
                Priority = Priority,
                Status = Status,
                StartDate = StartDate,
                DueDate = DueDate
            };

            bool updated = ProjectRepository.UpdateProject(project);
            return updated ? enProjectSaveResult.Saved : enProjectSaveResult.Failed;
        }
        catch
        {
            return enProjectSaveResult.Failed;
        }
    }

    // ─── Public: Save ──────────────────────────────────────────────────────
    public enProjectSaveResult Save()
    {
        switch (_Mode)
        {
            case enMode.AddNew: return _AddNew();
            case enMode.Update: return _Update();
            default: return enProjectSaveResult.Failed;
        }
    }

    // ─── Static: HardDelete ────────────────────────────────────────────────
    public static enProjectDeleteResult HardDelete(int projectId)
    {
        try
        {
            ProjectRepository.HardDeleteProject(projectId);
            return enProjectDeleteResult.Deleted;
        }
        catch
        {
            return enProjectDeleteResult.Failed;
        }
    }

    // ─── Static: SoftDelete ────────────────────────────────────────────────
    public static enProjectDeleteResult SoftDelete(int projectId)
    {
        try
        {
            string result = ProjectRepository.SoftDeleteProject(projectId);

            return result switch
            {
                "DELETED" => enProjectDeleteResult.Deleted,
                "HAS_ACTIVE_TASKS" => enProjectDeleteResult.HasActiveTasks,
                "HAS_COMPLETED_TASKS" => enProjectDeleteResult.HasCompletedTasks,
                _ => enProjectDeleteResult.Failed
            };
        }
        catch
        {
            return enProjectDeleteResult.Failed;
        }
    }

    // ─── Static: Find ──────────────────────────────────────────────────────
    public static (enProjectRetrieveResult result, ProjectService? service) Find(int projectId)
    {
        try
        {
            Project? project = ProjectRepository.GetProjectById(projectId);

            if (project is null)
                return (enProjectRetrieveResult.NotFound, null);

            return (enProjectRetrieveResult.Found, new ProjectService(project, enMode.Update));
        }
        catch
        {
            return (enProjectRetrieveResult.Failed, null);
        }
    }

    // ─── Static: GetAll Paged ─────────────────────────────────────────────
    public static (enProjectRetrieveResult result,
                   List<ProjectDetails> projects,
                   int totalCount) GetAll(
        int pageNumber = 1,
        int pageSize = 9,
        string? search = null,
        string? priority = null,
        string? status = null,
        int? categoryId = null)
    {
        try
        {
            var (projects, totalCount) = ProjectRepository.GetAllProjects(
                pageNumber, pageSize, search, priority, status, categoryId);

            return (enProjectRetrieveResult.Found, projects, totalCount);
        }
        catch
        {
            return (enProjectRetrieveResult.Failed, new List<ProjectDetails>(), 0);
        }
    }

    // ─── Static: GetByCategory ─────────────────────────────────────────────
    public static (enProjectRetrieveResult result, List<ProjectDetails> projects) GetByCategory(int categoryId)
    {
        try
        {
            var list = ProjectRepository.GetProjectsByCategory(categoryId);
            return (enProjectRetrieveResult.Found, list);
        }
        catch
        {
            return (enProjectRetrieveResult.Failed, new List<ProjectDetails>());
        }
    }

    // ─── Static: GetByStatus ───────────────────────────────────────────────
    public static (enProjectRetrieveResult result, List<ProjectDetails> projects) GetByStatus(string status)
    {
        try
        {
            var list = ProjectRepository.GetProjectsByStatus(status);
            return (enProjectRetrieveResult.Found, list);
        }
        catch
        {
            return (enProjectRetrieveResult.Failed, new List<ProjectDetails>());
        }
    }

    // ─── Static: IsNameTaken ───────────────────────────────────────────────
    public static bool IsNameTaken(string title)
    {
        try
        {
            return ProjectRepository.IsProjectNameTaken(title);
        }
        catch
        {
            return false;
        }
    }

    // ─── Static: IsNameTakenByOther ────────────────────────────────────────
    public static bool IsNameTakenByOther(int projectId, string title)
    {
        try
        {
            return ProjectRepository.IsProjectNameTakenByOther(projectId, title);
        }
        catch
        {
            return false;
        }
    }

    // ─── Static: GetLookup ─────────────────────────────────────────────────
    public static (enProjectRetrieveResult result, List<ProjectLookup> projects) GetLookup()
    {
        try
        {
            var list = ProjectRepository.GetProjectsLookup();
            return (enProjectRetrieveResult.Found, list);
        }
        catch
        {
            return (enProjectRetrieveResult.Failed, new List<ProjectLookup>());
        }
    }


}