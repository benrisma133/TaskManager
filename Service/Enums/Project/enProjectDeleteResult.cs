namespace Service.Enums.Project;

public enum enProjectDeleteResult
{
    Deleted,
    HasActiveTasks,
    HasCompletedTasks,
    Failed
}