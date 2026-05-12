namespace Service.Enums.Task;

public enum enTaskDeleteResult
{
    Deleted,
    AlreadyCompleted,
    HasSessions,
    Failed
}