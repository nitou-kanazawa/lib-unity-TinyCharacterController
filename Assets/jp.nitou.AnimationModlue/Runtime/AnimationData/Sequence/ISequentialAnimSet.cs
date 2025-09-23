using UnityEngine;

namespace Nitou.AnimationModule
{
    /// <summary>
    /// (�J�n / �ҋ@ / �I��) ���[�V�����ō\�������V���v���ȃA�j���[�V�����Z�b�g�D
    /// </summary>
    public interface ISequentialAnimSet : IAnimSet
    {
        /// <summary>
        /// 開始アニメーション．
        /// </summary>
        public AnimationClip StartClip { get; }
        public AnimationClip LoopClip { get; }
        public AnimationClip EndClip { get; }
    }
}