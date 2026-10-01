using System.Diagnostics;
using System.Windows.Threading;

namespace TaskManagerUI.Services
{
    /// <summary>
    /// Single authoritative timer for the entire application.
    /// Elapsed time is measured with a Stopwatch (real wall-clock time),
    /// not accumulated by counting DispatcherTimer ticks — ticks can drift
    /// or be skipped, a Stopwatch cannot.
    /// </summary>
    public static class GlobalTimer
    {
        private static readonly DispatcherTimer _timer;
        private static readonly Stopwatch _clock = new();

        public static event Action? Tick;

        public static TimeSpan TotalElapsed => _clock.Elapsed;
        public static int TotalElapsedSeconds => (int)Math.Round(_clock.Elapsed.TotalSeconds);
        public static bool IsRunning { get; private set; }

        static GlobalTimer()
        {
            _timer = new DispatcherTimer 
            { 
                Interval = TimeSpan.FromMicroseconds(200) 
            };

            _timer.Tick += (s, e) => Tick?.Invoke();
        }

        public static void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _clock.Start();
            _timer.Start();
        }

        public static void Stop()
        {
            IsRunning = false;
            _clock.Stop();
            _timer.Stop();
        }

        public static void Reset()
        {
            Stop();
            _clock.Reset();
        }

    }
}