using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="int"/>型のイベントリスナー．
    /// </summary>
    [AddComponentMenu(ComponentMenu.Prefix.EventChannel + "Int Event Listener")]
    public class IntEventListener : EventListener<int, IntEventChannel> { }
}