using UnityEngine;

namespace Nitou.EventChannel {

    /// <summary>
    /// <see cref="Color"/>型のイベントチャンネル．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New ColorEvent",
        menuName = AssetMenu.Prefix.EventChannel + "Color Event"
    )]
    public class ColorEventChannel : EventChannel<Color> { }

}