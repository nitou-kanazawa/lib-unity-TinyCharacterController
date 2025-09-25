using Nitou.EventChannel.Shared;
using UnityEngine;

namespace Nitou.EventChannel {
    using Nitou.EventChannel.Shared;

    /// <summary>
    /// <see cref="float"/>�^�̃C�x���g���X�i�[
    /// </summary>
    [AddComponentMenu(
        ComponentMenu.Prefix.EventChannel + "Float Event Listener"
    )]
    public class FloatEventListener : EventListener<float, FloatEventChannel> { }

}