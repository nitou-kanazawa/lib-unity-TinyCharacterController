using Nitou.Goap.Utilities;

namespace Nitou.Goap
{
    public sealed class IdleStrategy : IActionStrategy
    {
        public bool CanPerform => true;
        public bool Complete { get; private set; }

        private readonly CountdownTimer _timer;
        
        public IdleStrategy(float duration)
        {
            _timer = new CountdownTimer(duration);
            _timer.OnTimerStart += () => Complete = false;
            _timer.OnTimerStop += () => Complete = true;
        }
        
        public void Start() => _timer.Start();
        public void Update(float deltatime) => _timer.Tick(deltatime);
    }
}