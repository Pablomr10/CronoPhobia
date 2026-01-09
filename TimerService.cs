using System;
using System.Timers;

namespace Cronophobia
{
    public class TimerService
    {
        private readonly Timer _timer;
        private int _seconds;

        public bool IsRunning { get; private set; }

        public event Action<string>? TimeUpdated;

        public TimerService()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += OnTick;
        }

        private void OnTick(object? sender, ElapsedEventArgs e)
        {
            _seconds++;

            int minutes = _seconds / 60;
            int seconds = _seconds % 60;

            TimeUpdated?.Invoke($"{minutes:D2}:{seconds:D2}");
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            _timer.Stop();
        }

        public void Reset()
        {
            Stop();
            _seconds = 0;
            TimeUpdated?.Invoke("00:00");
        }
    }
}
