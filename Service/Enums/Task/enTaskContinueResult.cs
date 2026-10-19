namespace Service.Enums.Task;

public enum enTaskContinueResult
{
    Continued,    // task is InProgress again, no minutes were added
    NotDone,      // task was not Done, nothing to continue
    NoTimeLeft,   // all time used up: the user must Add Extra Time
    Failed
}