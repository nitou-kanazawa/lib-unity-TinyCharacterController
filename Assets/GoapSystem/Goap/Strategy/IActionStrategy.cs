namespace Nitou.Goap
{
    public interface IActionStrategy
    {
        /// <summary>
        /// 実行できる状態かどうか．
        /// </summary>
        bool CanPerform { get; }
        
        /// <summary>
        /// 完了しているかどうか．
        /// </summary>
        bool Complete { get; }

        void Start() { }

        void Update(float deltaTime) { }

        void Stop() { }
    }
}