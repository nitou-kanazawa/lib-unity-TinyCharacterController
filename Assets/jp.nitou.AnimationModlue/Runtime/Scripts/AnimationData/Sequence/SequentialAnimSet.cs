using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;

namespace Nitou.AnimationModule{

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class SequentialAnimSet : ISequentialAnimSet{

        // [DarkBox]
        [SerializeField, Indent] private AnimationClip _startClip;
        // [DarkBox]
        [SerializeField, Indent] private AnimationClip _loopClip;
        // [DarkBox]
        [SerializeField, Indent] private AnimationClip _endClip;


        public AnimationClip StartClip => _startClip;

        /// <summary>
        /// ���[�v�A�j���[�V����
        /// </summary>
        public AnimationClip LoopClip => _loopClip;

        /// <summary>
        /// �I���A�j���[�V����
        /// </summary>
        public AnimationClip EndClip => _endClip;
    }
}
