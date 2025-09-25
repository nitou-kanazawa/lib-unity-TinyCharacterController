using UnityEngine;

namespace Nitou.EventChannel
{
    /// <summary>
    /// <see cref="AudioClip"/>のイベントチャンネル．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New AudioClipEvent",
        menuName = AssetMenu.Prefix.EventChannel + "AudioClip Event"
    )]
    public class AudioClipEventChannel : EventChannel<AudioClip> { }
}