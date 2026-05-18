// Service/Services/TimerService.cs
using Repository.Models;
using Repository.Repositories;
using Service.Enums.Timer;

namespace Service.Services
{
    public class TimerService
    {
        // ============================
        // FIELDS
        // ============================
        private enTimerMode _mode = enTimerMode.Start;
        private int _sessionId = 0;
        private int _taskId = 0;
        private int _totalPausedSeconds = 0;

        // ============================
        // PROPERTIES
        // ============================
        public enTimerMode Mode => _mode;
        public int SessionId => _sessionId;
        public bool IsRunning => _mode == enTimerMode.End;

        // ============================
        // CONSTRUCTOR
        // ============================
        public TimerService(int taskId)
        {
            _taskId = taskId;
            _mode = enTimerMode.Start;
        }

        // ============================
        // SAVE  (switches by mode)
        // ============================
        public enTimerSaveResult Save()
        {
            return _mode switch
            {
                enTimerMode.Start => _Start(),
                enTimerMode.End => _End(),
                _ => enTimerSaveResult.Failed
            };
        }

        // ============================
        // FORCE END  (Stop button)
        // ============================
        public enTimerSaveResult ForceEnd(int totalPausedSeconds, int actualDurationSeconds = -1)
        {
            _totalPausedSeconds = totalPausedSeconds;

            if (_mode == enTimerMode.End && _sessionId > 0)
            {
                try
                {
                    bool ok = TimerRepository.EndSession(_sessionId, _totalPausedSeconds, actualDurationSeconds);

                    if (!ok) return enTimerSaveResult.Failed;

                    _sessionId = 0;
                    _mode = enTimerMode.Start;
                    _totalPausedSeconds = 0;
                    return enTimerSaveResult.Ended;
                }
                catch
                {
                    return enTimerSaveResult.Failed;
                }
            }

            _mode = enTimerMode.Start;
            _sessionId = 0;
            _totalPausedSeconds = 0;
            return enTimerSaveResult.Ended;
        }

        // ============================
        // PRIVATE — START
        // ============================
        private enTimerSaveResult _Start()
        {
            try
            {
                int newSessionId = TimerRepository.StartSession(_taskId);

                if (newSessionId <= 0)
                    return enTimerSaveResult.Failed;

                _sessionId = newSessionId;
                _mode = enTimerMode.End;
                return enTimerSaveResult.Started;
            }
            catch
            {
                return enTimerSaveResult.Failed;
            }
        }

        // ============================
        // PRIVATE — END
        // ============================
        private enTimerSaveResult _End()
        {
            try
            {
                if (_sessionId <= 0)
                    return enTimerSaveResult.Failed;

                bool ok = TimerRepository.EndSession(_sessionId, _totalPausedSeconds);

                if (!ok)
                    return enTimerSaveResult.Failed;

                _sessionId = 0;
                _mode = enTimerMode.Start;
                _totalPausedSeconds = 0;
                return enTimerSaveResult.Ended;
            }
            catch
            {
                return enTimerSaveResult.Failed;
            }
        }

        // ============================
        // GET TODAY SESSIONS
        // ============================
        public (List<TimerSession> sessions, int totalSecondsToday, int totalMinutesToday) GetTodaySessions()
        {
            try
            {
                return TimerRepository.GetTodaySessionsByTask(_taskId);
            }
            catch
            {
                return (new List<TimerSession>(), 0, 0);
            }
        }

        // ============================
        // GET TASK TIMER SUMMARY
        // ============================
        public (int totalSecondsAllTime, int totalSecondsToday,
        List<TimerSession> pastSessions) GetTaskTimerSummary()
        {
            try
            {
                return TimerRepository.GetTaskTimerSummary(_taskId);
            }
            catch
            {
                return (0, 0, new List<TimerSession>());
            }
        }
    }
}