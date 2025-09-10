namespace Nitou.BatchProcessor
{
    public interface IPostUpdate : ISystemBase
    {
        void OnLateUpdate();
    }
}