using System.Windows.Threading;

namespace TaskManagerUI.Services
{
    /// <summary>
    /// Single authoritative timer for the entire application.
    /// All time-dependent components subscribe to this.
    /// </summary>
    public static class GlobalTimer
    {
        private static readonly DispatcherTimer _timer;
        private static TimeSpan _totalElapsed = TimeSpan.Zero;

        // ============================
        // EVENTS — components subscribe to this
        // ============================
        public static event Action? Tick;

        // ============================
        // PROPERTIES
        // ============================
        public static TimeSpan TotalElapsed => _totalElapsed;
        public static int TotalElapsedSeconds => (int)_totalElapsed.TotalSeconds;
        public static bool IsRunning { get; private set; }

        // ============================
        // STATIC CONSTRUCTOR
        // ============================
        static GlobalTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        // ============================
        // START / STOP / RESET
        // ============================
        public static void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _timer.Start();
        }

        public static void Stop()
        {
            IsRunning = false;
            _timer.Stop();
        }

        public static void Reset()
        {
            Stop();
            _totalElapsed = TimeSpan.Zero;
        }

        // ============================
        // INTERNAL TICK
        // ============================
        private static void OnTimerTick(object? sender, EventArgs e)
        {
            _totalElapsed = _totalElapsed.Add(TimeSpan.FromSeconds(1));
            Tick?.Invoke();
        }
    }
}