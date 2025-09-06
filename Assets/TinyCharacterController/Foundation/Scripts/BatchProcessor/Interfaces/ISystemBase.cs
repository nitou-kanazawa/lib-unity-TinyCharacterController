namespace Nitou.BatchProcessor
{
    public interface ISystemBase
    {
        /// <summary>
        /// 処理の優先度．
        /// </summary>
        int Order { get; }
    }

    public interface IEarlyUpdate : ISystemBase
    {
        void OnUpdate();
    }

    public interface IPostUpdate : ISystemBase
    {
        void OnLateUpdate();
    }
}