using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AlexDevTools.TimerSystem
{
    public class Timer
    {
        private readonly int _delay;
        private readonly bool _repeating;
        private readonly Action _onCompleted;
        
        private const int TimeScale = 1;
        private bool _pause;
        private bool _canceled;

        public int ElapsedTime { get; private set; }
        public int Delay => _delay;
        public bool IsRunning { get; private set; }

        public Timer(int delay, bool repeating, Action onCompleted)
        {
            _delay = delay;
            _repeating = repeating;
            _onCompleted = onCompleted;
        }
        
        public void Start()
        {
            Tick();
        }

        private async void Tick()
        {
            IsRunning = true;
            do
            {
                while (ElapsedTime <= _delay)
                {
                    if (_canceled)
                        return;
                    await Task.Delay(1000);
                    if(!_pause)
                        ElapsedTime += TimeScale;
                }

                ElapsedTime = 0;
                _onCompleted?.Invoke();
            } while (_repeating);

            IsRunning = false;
        }

        public void ChangeState(bool pause)
        {
            _pause = pause;
            IsRunning = !pause;
        }

        public void Stop()
        {
            _canceled = true;
            IsRunning = false;
        }

        public void Reset()
        {
            ElapsedTime = 0;
        }
    }
}