using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;

// REF:
// - Animancer: https://kybernetik.com.au/animancer/docs/examples/animator-controllers/3d-game-kit/idle/

namespace Nitou.AnimationModule
{
    /// <summary>
    /// Idle状態のアニメーションを管理するアセット．
    /// </summary>
    [CreateAssetMenu(
        fileName = "New IdleAnimSet",
        menuName = AssetMenu.Prefix.AnimationData + "Idle Animations"
    )]
    public sealed class IdleAnimSet : ScriptableObject, IAnimSet
    {
        [Title("Animation Clips")]
        [SerializeField, Indent] ClipTransition _mainIdleClip;

        [SerializeField, Indent] ClipTransition[] _randomMotionClips;

        /// <summary>
        /// 待機アニメーション．
        /// </summary>
        public ClipTransition MainClip => _mainIdleClip;

        /// <summary>
        /// ランダムモーション．
        /// </summary>
        public IReadOnlyList<ClipTransition> RandomMotionClips => _randomMotionClips;

        /// <summary>
        /// ランダムモーションが存在するかどうか．
        /// </summary>
        public bool HasRandomMotion => !_randomMotionClips.IsNullOrEmpty();


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// ランダムモーションを取得する．
        /// </summary>
        public bool TryGetRandomMotionClip(out ClipTransition clip)
        {
            clip = null;
            if (_randomMotionClips.IsNullOrEmpty()) return false;

            // ランダムに要素取得
            var index = Random.Range(0, _randomMotionClips.Length);
            clip = _randomMotionClips[index];
            return clip != null;
        }
    }
}