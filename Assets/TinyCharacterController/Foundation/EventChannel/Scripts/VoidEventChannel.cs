using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="void"/>型のイベントチャンネル．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New VoidEvent",
        menuName = AssetMenu.Prefix.EventChannel + "Void Event"
    )]
    public class VoidEventChannel : EventChannel { }
}