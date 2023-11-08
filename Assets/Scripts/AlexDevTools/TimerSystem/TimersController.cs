using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AlexDevTools.TimerSystem
{
    public class TimersController : MonoBehaviour
    {
        private int _lastId;
        private Dictionary<int, Timer> _timers;

        public event Action OnTimersChanged;
        public Timer[] Timers => _timers.Values.ToArray();
        public static TimersController Instance { get; private set; }
        
        private void Awake()
        {
            if(Instance != null)
                Destroy(Instance.gameObject);
            Instance = this;
            
            _timers ??= new Dictionary<int, Timer>();
        }

        public int StartTimer(int delay, bool repeating, Action onFired)
        {
            int timerId = _lastId;
            _lastId++;
            
            Timer timer = new Timer(delay, repeating, () =>
            {
                _timers.Remove(timerId);
                OnTimersChanged?.Invoke();
                onFired?.Invoke();
            });

            if (_timers.TryAdd(timerId, timer))
            {
                timer.Start();
                OnTimersChanged?.Invoke();
                return timerId;
            }

            return -1;
        }

        public Timer GetTimer(int timerId)
        {
            return _timers.TryGetValue(timerId, out Timer timer) ? timer : null;
        }

        public void ChangeState(int timerId, bool pause)
        {
            if (!_timers.TryGetValue(timerId, out Timer timer)) return;
            
            timer.ChangeState(pause);
            OnTimersChanged?.Invoke();
        }

        public void StopTimer(int timerId)
        {
            if (!_timers.TryGetValue(timerId, out Timer timer)) return;
            
            timer.Stop();
            _timers.Remove(timerId);
            OnTimersChanged?.Invoke();
        }

        public void ResetTimer(int timerId)
        {
            if (_timers.TryGetValue(timerId, out Timer timer))
            {
                timer.Reset();
                OnTimersChanged?.Invoke();
            }
        }

        private void OnDisable()
        {
            Parallel.ForEach(_timers, timer =>
            {
                timer.Value.Stop();
            });
        }

        private void OnApplicationQuit()
        {
            OnDisable();
        }
    }
}