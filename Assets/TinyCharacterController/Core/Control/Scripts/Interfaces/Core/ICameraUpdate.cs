namespace Nitou.TCC.Controller.Interfaces.Core
{
    /// <summary>
    /// カメラの向きを更新するためのインターフェース．
    /// <see cref="Controller.Core.BrainBase"/> 内での移動処理が完了した後に実行される．
    /// </summary>
    public interface ICameraUpdate
    {
        /// <summary>
        /// カメラの向きを更新する．
        /// </summary>
        void OnUpdate(float deltaTime);
    }
}