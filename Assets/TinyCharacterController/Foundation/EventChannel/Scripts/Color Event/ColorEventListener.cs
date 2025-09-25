using Nitou.EventChannel.Shared;
using UnityEngine;

namespace Nitou.EventChannel {
    using Nitou.EventChannel.Shared;

    /// <summary>
    /// <see cref="Color"/>�^�̃C�x���g���X�i�[
    /// </summary>
    [AddComponentMenu(
        ComponentMenu.Prefix.EventChannel + "Color Event Listener"
    )]
    public class ColorEventListener : EventListener<Color, ColorEventChannel> { }

}