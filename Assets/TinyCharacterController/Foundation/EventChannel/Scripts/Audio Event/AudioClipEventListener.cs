using Nitou.EventChannel.Shared;
using UnityEngine;

namespace Nitou.EventChannel {
    using Nitou.EventChannel.Shared;

    /// <summary>
    /// AudioClip�^�̃C�x���g���X�i�[ 
    /// </summary>
    [AddComponentMenu(
        ComponentMenu.Prefix.EventChannel + "AudioClip Event Listener"
    )]
    public class AudioClipEventListener : EventListener<AudioClip, AudioClipEventChannel> { }
}
