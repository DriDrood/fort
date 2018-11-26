using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fort.Utils
{
    public class Timer
    {
        public TimeSpan? Remains => IsPaused || IsEnded || NotStarted ? _duration : _duration - (DateTime.UtcNow - _startAt);
        public bool IsPaused { get; private set; }
        public bool IsEnded { get; private set; }
        public bool NotStarted { get; set; }

        private TimeSpan _duration;
        private DateTime _startAt;
        private Task _sleeping;
        private CancellationTokenSource _cancelToken;
        private TaskCompletionSource<bool> _awaitingPause;

        public Task NewStart(TimeSpan duration)
        {
            SetTime(duration);
            return Start();
        }

        public Task Start()
        {
            if (!NotStarted)
                return null;

            NotStarted = false;

            _startAt = DateTime.UtcNow;
            _cancelToken = new CancellationTokenSource();
            _sleeping = Task.Run(() => _cancelToken.Token.WaitHandle.WaitOne(_duration), _cancelToken.Token);

            return sleep();
        }

        public void Pause()
        {
            if (IsPaused || NotStarted)
                return;

            IsPaused = true;
            _duration = _duration - (DateTime.UtcNow - _startAt);

            _awaitingPause = new TaskCompletionSource<bool>();

            _cancelToken.Cancel();
            _cancelToken = null;
        }

        public void Resume()
        {
            if (!IsPaused || NotStarted)
                return;

            IsPaused = false;
            _startAt = DateTime.UtcNow;
            _cancelToken = new CancellationTokenSource();
            _sleeping = Task.Run(() => _cancelToken.Token.WaitHandle.WaitOne(_duration), _cancelToken.Token);

            _awaitingPause.SetResult(true);
            _awaitingPause = null;
        }

        public void End()
        {
            if (!IsPaused)
                _duration = _duration - (DateTime.UtcNow - _startAt);

            IsPaused = false;
            IsEnded = true;

            _cancelToken?.Cancel();
            _cancelToken = null;
            _awaitingPause?.SetResult(false);
            _awaitingPause = null;
        }

        public void SetTime(TimeSpan duration)
        {
            _duration = duration;
            IsPaused = false;
            IsEnded = false;
            NotStarted = true;
        }

        private async Task sleep()
        {
            while (!IsEnded)
            {
                // wait
                await _sleeping;

                // paused
                if (IsPaused)
                    await _awaitingPause.Task;

                // time finished
                else if (!IsEnded)
                {
                    IsEnded = true;
                    _duration = TimeSpan.Zero;
                }
            }
        }
    }
}