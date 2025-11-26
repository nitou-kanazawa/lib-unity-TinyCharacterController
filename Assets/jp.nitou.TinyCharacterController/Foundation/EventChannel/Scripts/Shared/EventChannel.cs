using UnityEngine;
using Sirenix.OdinInspector;

// REF:
// - youtube:  Devlog 2｜スクリプタブルオブジェクトを使ったゲームアーキテクチャ https://www.youtube.com/watch?v=WLDgtRNK2VE

namespace Nitou.EventChannel
{
    /// <summary>
    /// イベントチャンネル用のたたき台となる<see cref="ScriptableObject"/>．
    /// </summary>
    public abstract class EventChannel : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable 0414
        // 説明文
        [Multiline]
        [SerializeField] private string _description;
#pragma warning restore 0414
#endif
        public event System.Action OnEventRaised = delegate { };

        /// <summary>
        /// イベントの発火．
        /// </summary>
        [Button("Publish")]
        public void Publish() => OnEventRaised.Invoke();
    }


    /// <summary>
    /// イベントチャンネル用のたたき台となるScriptable Object
    /// </summary>
    public abstract class EventChannel<TData> : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable 0414
        // 説明文
        [Multiline]
        [SerializeField] private string _description = default;
#pragma warning restore 0414
#endif
        public event System.Action<TData> OnEventRaised = delegate { };

        /// <summary>
        /// イベントの発火．
        /// </summary>
        [Button("Publish")]
        public void Publish(TData value)
        {
            if (value == null)
            {
                Debug.LogWarning($"[{name}] event argument is null.");
                return;
            }

            OnEventRaised.Invoke(value);
        }
    }
}