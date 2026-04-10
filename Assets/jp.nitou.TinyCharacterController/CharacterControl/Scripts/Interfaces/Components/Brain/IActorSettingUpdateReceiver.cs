using Nitou.TCC.CharacterControl.Core;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Interfaces.Components
{
    /// <summary>
    /// CharacterSettings の値が変化したときに呼び出されるコールバック．
    /// 主に CharacterController やコライダーのサイズ変更に使用する．
    /// </summary>
    public interface IActorSettingUpdateReceiver
    {
        /// <summary>
        /// CharacterSettings の値が変化したときに呼び出される．
        /// </summary>
        void OnUpdateSettings(CharacterSettings settings);
    }
}