using R3;

namespace Nitou.TCC.Controller.Interfaces.Components
{
    /// <summary>
    /// Callback events for landing and leaving
    /// </summary>
    public interface IGravityEvent
    {
        /// <summary>
        /// 着地時に通知するObservable．
        /// </summary>
        public Observable<float> OnLanding { get; }

        /// <summary>
        /// 離陸時に通知するObservable．
        /// </summary>
        public Observable<Unit> OnLeave { get; }
    }
}