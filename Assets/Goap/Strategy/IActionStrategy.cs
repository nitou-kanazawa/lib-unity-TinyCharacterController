namespace Nitou.Goap
{
    public interface IActionStorategy
    {
        bool CanPerform { get; }
        
        bool Complete { get; }

        void Start() { }

        void Update(float deltaTime) { }

        void Stop() { }
    }
}