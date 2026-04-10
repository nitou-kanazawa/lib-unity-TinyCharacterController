using R3;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// 着地・離陸のコールバックイベント．
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