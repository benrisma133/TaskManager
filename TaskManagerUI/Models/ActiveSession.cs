using Service.Enums.Task;
using Service.Enums.Timer;
using Service.Services;
using TaskManagerUI.Services;

public static class ActiveSession
{
    // ============================
    // FIELDS
    // ============================
    private static TimeSpan _sessionElapsedOffset = TimeSpan.Zero;  // ← Track when session started
    private static TimeSpan _totalPaused = TimeSpan.Zero;
    private static DateTime? _pausedAt = null;
    private static TimeSpan _globalTimerAtSessionStart = TimeSpan.Zero;

    // ============================
    // PROPERTIES
    // ============================
    public static TaskService? CurrentTask { get; private set; }
    public static TimerService? Timer { get; private set; }
    public static bool IsRunning { get; private set; }
    public static DateTime? StartedAt { get; private set; }
    public static int SessionId { get; private set; }

    /// <summary>
    /// Session elapsed = GlobalTimer elapsed since session started - paused time
    /// </summary>
    public static TimeSpan Elapsed
    {
        get
        {
            if (!HasSession) return TimeSpan.Zero;

            // GlobalTimer only advances while running, so paused time
            // is already excluded. Don't subtract _totalPaused here.
            return GlobalTimer.TotalElapsed - _globalTimerAtSessionStart;
        }
    }

    public static int ElapsedSeconds => (int)Elapsed.TotalSeconds;
    public static int TotalPausedSeconds => (int)_totalPaused.TotalSeconds;
    public static int TotalLoggedAllTime { get; private set; }
    public static int TotalLoggedToday { get; private set; }

    // ============================
    // EVENTS
    // ============================
    public static event Action? SessionChanged;
    public static event Action? Ticked;

    // ============================
    // START
    // ============================
    public static bool Start(int taskId)
    {
        Timer = new TimerService(taskId);
        var saveResult = Timer.Save();

        if (saveResult != enTimerSaveResult.Started)
            return false;

        var (result, service) = TaskService.Find(taskId);

        if (result != enTaskRetrieveResult.Found || service is null)
            return false;

        CurrentTask = service;
        IsRunning = true;
        StartedAt = DateTime.Now;
        SessionId = Timer.SessionId;
        _sessionElapsedOffset = TimeSpan.Zero;
        _totalPaused = TimeSpan.Zero;
        _pausedAt = null;

        // ← Record when GlobalTimer was at when this session started
        _globalTimerAtSessionStart = GlobalTimer.TotalElapsed;

        // ← Subscribe to GlobalTimer for all time tracking
        GlobalTimer.Tick -= OnGlobalTimerTick;
        GlobalTimer.Tick += OnGlobalTimerTick;

        GlobalTimer.Start();

        SessionChanged?.Invoke();
        return true;
    }

    // ============================
    // GLOBAL TIMER TICK
    // ============================
    private static void OnGlobalTimerTick()
    {
        if (!IsRunning) return;

        Ticked?.Invoke();
    }

    // ============================
    // PAUSE
    // ============================
    public static void Pause()
    {
        if (!IsRunning) return;

        IsRunning = false;
        _pausedAt = DateTime.Now;
        GlobalTimer.Stop();  // ← Pause the global timer

        SessionChanged?.Invoke();
    }

    // ============================
    // RESUME
    // ============================
    public static void Resume()
    {
        if (CurrentTask is null || IsRunning) return;

        if (_pausedAt.HasValue)
        {
            TimeSpan pauseDuration = DateTime.Now - _pausedAt.Value;
            _totalPaused = _totalPaused.Add(pauseDuration);
            _pausedAt = null;
        }

        IsRunning = true;
        GlobalTimer.Start();  // ← Resume the global timer

        SessionChanged?.Invoke();
    }

    // ============================
    // STOP — The key method
    // exactDurationSeconds lets you override what gets saved
    // ============================
    public static void Stop(int exactDurationSeconds = -1)
    {
        if (!IsRunning && _pausedAt.HasValue)
        {
            TimeSpan pauseDuration = DateTime.Now - _pausedAt.Value;
            _totalPaused = _totalPaused.Add(pauseDuration);
        }

        // ← Use actual elapsed from global timer
        int sessionDuration = exactDurationSeconds >= 0
            ? exactDurationSeconds
            : ElapsedSeconds;

        if (Timer is not null)
            Timer.ForceEnd(TotalPausedSeconds, sessionDuration);

        GlobalTimer.Tick -= OnGlobalTimerTick;
        // Don't stop GlobalTimer here — other sessions might be using it

        CurrentTask = null;
        IsRunning = false;
        StartedAt = null;
        SessionId = 0;
        Timer = null;
        _sessionElapsedOffset = TimeSpan.Zero;
        _totalPaused = TimeSpan.Zero;
        _pausedAt = null;
        _globalTimerAtSessionStart = TimeSpan.Zero;
        TotalLoggedAllTime = 0;
        TotalLoggedToday = 0;

        SessionChanged?.Invoke();
    }

    // ============================
    // LOAD SUMMARY
    // ============================
    public static void LoadSummary(int taskId)
    {
        try
        {
            var tempService = new TimerService(taskId);

            var (secsAllTime, secsToday, _) =
                tempService.GetTaskTimerSummary();

            TotalLoggedAllTime = secsAllTime;
            TotalLoggedToday = secsToday;
        }
        catch
        {
            TotalLoggedAllTime = 0;
            TotalLoggedToday = 0;
        }
    }

    // ============================
    // HAS SESSION
    // ============================
    public static bool HasSession => CurrentTask is not null;
}