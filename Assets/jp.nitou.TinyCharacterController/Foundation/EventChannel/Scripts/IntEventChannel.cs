using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="int"/>型のイベントチャンネル．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New IntEvent",
        menuName = AssetMenu.Prefix.EventChannel + "Int Event"
    )]
    public class IntEventChannel : EventChannel<int> { }
}