using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="float"/>型のイベントチャンネル．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New FloatEvent",
        menuName = AssetMenu.Prefix.EventChannel + "Float Event"
    )]
    public class FloatEventChannel : EventChannel<float> { }
}