using Service.Enums.Task;
using Service.Enums.Timer;
using Service.Services;
using System.Windows.Threading;

public static class ActiveSession
{
    // ============================
    // FIELDS
    // ============================
    private static DispatcherTimer? _timer;
    private static TimeSpan _elapsed = TimeSpan.Zero;
    private static TimeSpan _totalPaused = TimeSpan.Zero;
    private static DateTime? _pausedAt = null;

    // ============================
    // PROPERTIES
    // ============================
    public static TaskService? CurrentTask { get; private set; }
    public static TimerService? Timer { get; private set; }
    public static bool IsRunning { get; private set; }
    public static DateTime? StartedAt { get; private set; }
    public static int SessionId { get; private set; }
    public static TimeSpan Elapsed => _elapsed;
    public static int ElapsedSeconds => (int)_elapsed.TotalSeconds;
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
        _elapsed = TimeSpan.Zero;
        _totalPaused = TimeSpan.Zero;
        _pausedAt = null;

        _EnsureTimer();
        _timer!.Start();

        SessionChanged?.Invoke();
        return true;
    }

    // ============================
    // PAUSE
    // ============================
    public static void Pause()
    {
        if (!IsRunning) return;

        IsRunning = false;
        _pausedAt = DateTime.Now;
        _timer?.Stop();

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
        _timer?.Start();

        SessionChanged?.Invoke();
    }

    // ============================
    // STOP
    // ============================
    public static void Stop(int exactDurationSeconds = -1)
    {
        if (!IsRunning && _pausedAt.HasValue)
        {
            TimeSpan pauseDuration = DateTime.Now - _pausedAt.Value;
            _totalPaused = _totalPaused.Add(pauseDuration);
        }

        if (Timer is not null)
            Timer.ForceEnd(TotalPausedSeconds, exactDurationSeconds);

        _timer?.Stop();

        CurrentTask = null;
        IsRunning = false;
        StartedAt = null;
        SessionId = 0;
        Timer = null;
        _elapsed = TimeSpan.Zero;
        _totalPaused = TimeSpan.Zero;
        _pausedAt = null;
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

    // ============================
    // PRIVATE — ENSURE TIMER
    // ============================
    private static void _EnsureTimer()
    {
        if (_timer is not null) return;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) =>
        {
            if (!IsRunning) return;
            _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));
            Ticked?.Invoke();
        };
    }
}