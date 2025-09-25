using Nitou.EventChannel.Shared;
using UnityEngine;

namespace Nitou.EventChannel {
    using Nitou.EventChannel.Shared;

    /// <summary>
    /// <see cref="int"/>�^�̃C�x���g���X�i�[
    /// </summary>
    [AddComponentMenu(
        ComponentMenu.Prefix.EventChannel + "Int Event Listener"
    )]
    public class IntEventListener : EventListener<int, IntEventChannel> {}

}