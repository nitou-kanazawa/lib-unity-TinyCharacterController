using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Sirenix.OdinInspector;
using Nitou;
using Nitou.AnimationModule;

namespace Project.Actor
{
    using StateBase = Nitou.DesignPattern.State<ActorCore, ActorFMS.SetupParam>;

    /// <summary>
    /// 待機ステート．
    /// </summary>
    public sealed class IdleState : ActorState
    {
        [Title("Animation")]
        [SerializeField] IdleAnimSet _idleAnimSet;

        [Title("Random Motion")]
        [LabelText("Interval range")]
        [SerializeField, Indent] RangeFloat _randomizeIntervalRange = new(5, 20);

        // 内部処理用
        private float _randomizeTime;
        private AnimancerState _idleState;


        // ----------------------------------------------------------------------------
        // Override Method (ステート処理)

        protected override void OnInitialized(ActorFMS.SetupParam param)
        {
            base.OnInitialized(param);

            // Main animation
            _idleState = Anim.Animancer.States.GetOrCreate(_idleAnimSet.MainClip);

            // Random animation
            System.Action onEnd = PlayMainAnimation;
            foreach (var clip in _idleAnimSet.RandomMotionClips)
            {
                clip.Events.OnEnd = onEnd;
            }
        }

        public override void EnterBehaviour(float dt, StateBase fromState)
        {
            // アニメーション再生
            PlayMainAnimation();
            _randomizeTime += _randomizeIntervalRange.Min;
        }

        public override void ExitBehaviour(float dt, StateBase toState) { }
        
        public override void UpdateBehaviour(float dt)
        {
            // ランダムモーション
            var state = Anim.Animancer.States.Current;
            if (state == _idleState && state.Time >= _randomizeTime)
            {
                PlayRandomAnimation();
            }
        }



        // ----------------------------------------------------------------------------
        // Private Method (アニメーション)

        /// <summary>
        /// Idleアニメーションの再生．
        /// </summary>
        private void PlayMainAnimation()
        {
            Anim.Animancer.Play(_idleState);
            _randomizeTime = _randomizeIntervalRange.Random;
        }

        /// <summary>
        /// ランダムなアニメーションを再生する．
        /// </summary>
        private void PlayRandomAnimation()
        {
            if (_idleAnimSet.TryGetRandomMotionClip(out var clip))
            {
                Anim.Animancer.Play(clip);
            }
        }


        /// ----------------------------------------------------------------------------
        // Public Method (その他)
        public override string GetName() => "Idle";

        public override Color GetColor() => Color.white;
    }
}