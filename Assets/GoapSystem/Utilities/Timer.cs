using System;

namespace Nitou.Goap.Utilities
{
    public abstract class Timer
    {
        protected float initialTime;

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        public float Time { get; set; }
        public bool IsRunning { get; protected set; }

        public float Progress => Time / initialTime;
        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;
        }

        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        /// <summary>
        /// タイマーの更新．
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Tick(float deltaTime);
    }

    public sealed class CountdownTimer : Timer
    {
        public bool IsFinished => Time <= 0;


        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public CountdownTimer(float value) : base(value) { }

        public void Reset() => Time = initialTime;

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0)
            {
                Time -= deltaTime;
            }

            if (IsRunning && Time <= 0)
            {
                Stop();
            }
        }

        public void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }
    }


    public sealed class StopwatchTimer : Timer
    {
        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public StopwatchTimer() : base(0) { }

        public override void Tick(float deltaTime)
        {
            if (IsRunning)
            {
                Time += deltaTime;
            }
        }

        public void Reset() => Time = 0;

        public float GetTime() => Time;
    }
}