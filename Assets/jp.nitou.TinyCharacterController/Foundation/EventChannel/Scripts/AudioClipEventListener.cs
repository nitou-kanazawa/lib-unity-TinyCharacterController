using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="AudioClip"/>型のイベントチャンネル．
    /// </summary>
    [AddComponentMenu(ComponentMenu.Prefix.EventChannel + "AudioClip Event Listener")]
    public class AudioClipEventListener : EventListener<AudioClip, AudioClipEventChannel> { }
}