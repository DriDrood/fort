using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fort.Utils
{
    public class Timer
    {
        public TimeSpan? Remains => State == Status.Running
            ? _duration - (DateTime.UtcNow - _startAt)
            : _duration;

        public Status State { get; private set; }

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
            if (State == Status.Paused || State == Status.Running)
                return null;

            State = Status.Running;

            _startAt = DateTime.UtcNow;
            _cancelToken = new CancellationTokenSource();
            _sleeping = Task.Run(() => _cancelToken.Token.WaitHandle.WaitOne(_duration), _cancelToken.Token);

            return sleep();
        }

        public void Pause()
        {
            if (State != Status.Running)
                return;

            State = Status.Paused;
            _duration = _duration - (DateTime.UtcNow - _startAt);

            _awaitingPause = new TaskCompletionSource<bool>();

            _cancelToken.Cancel();
            _cancelToken = null;
        }

        public void Resume()
        {
            if (State != Status.Paused)
                return;

            State = Status.Running;
            _startAt = DateTime.UtcNow;
            _cancelToken = new CancellationTokenSource();
            _sleeping = Task.Run(() => _cancelToken.Token.WaitHandle.WaitOne(_duration), _cancelToken.Token);

            _awaitingPause.SetResult(true);
            _awaitingPause = null;
        }

        public void End()
        {
            if (State == Status.Running)
                _duration = _duration - (DateTime.UtcNow - _startAt);

            State = Status.Finished;

            _cancelToken?.Cancel();
            _cancelToken = null;
            _awaitingPause?.SetResult(false);
            _awaitingPause = null;
        }

        public void SetTime(TimeSpan duration)
        {
            _duration = duration;
            State = Status.Begin;
        }

        private async Task sleep()
        {
            while (State != Status.Finished)
            {
                // wait
                await _sleeping;

                // paused
                if (State == Status.Paused)
                    await _awaitingPause.Task;

                // time finished
                else if (State == Status.Running)
                {
                    State = Status.Finished;
                    _duration = TimeSpan.Zero;
                }
            }
        }

        public enum Status
        {
            Begin,
            Running,
            Paused,
            Finished
        }
    }
}