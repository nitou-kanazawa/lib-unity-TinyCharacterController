using UnityEngine;
using System.Collections.Generic;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// <see cref="ITurn"/>を統括するマネージャークラス．
    /// </summary>
    internal sealed class TurnManager
    {
        private ITransform _transform;
        private IPriorityLifecycle<ITurn> _turnLifecycle;
        private readonly List<ITurn> _turns = new();

        /// <summary>
        /// 現在アクティブな Turn コンポーネント．
        /// </summary>
        public ITurn CurrentTurn { get; private set; }

        /// <summary>
        /// 最も高い優先度の回転が保持されているかどうかを示す．
        /// </summary>
        public bool HasHighestPriority { get; private set; } = false;

        /// <summary>
        /// 目標のYaw角度を示す．
        /// </summary>
        public float TargetYawAngle { get; private set; }

        /// <summary>
        /// 前回のYaw角度との差分を示す．
        /// </summary>
        public float DeltaTurnAngle { get; private set; }

        /// <summary>
        /// 次のYaw角度を計算する．
        /// </summary>
        public float NextYawAngle { get; private set; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 初期化処理．
        /// </summary>
        /// <param name="obj">回転を取得する GameObject．</param>
        /// <param name="transform">オブジェクトの Transform．</param>
        public void Initialize(GameObject obj, ITransform transform)
        {
            _transform = transform;
            obj.GetComponentsInChildren(_turns);
        }

        /// <summary>
        /// 最も高い優先度の回転制御を更新する．
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間．</param>
        public void UpdateHighestTurnControl(float deltaTime)
        {
            using var _ = new ProfilerScope("Update Highest Turn Control");

            HasHighestPriority = _turns.TryGetHighestPriority(out var highestTurn);
            var isChangeHighestPriorityComponent = highestTurn != CurrentTurn;

            if (isChangeHighestPriorityComponent)
            {
                HandleLoseHighestPriority();
                HandleAcquireHighestPriority(highestTurn);
            }

            CurrentTurn = highestTurn;

            if (HasHighestPriority)
                _turnLifecycle?.OnUpdateWithHighestPriority(deltaTime);
        }

        /// <summary>
        /// 角度計算．
        /// </summary>
        public void CalculateAngle(float deltaTime)
        {
            using var _ = new ProfilerScope("Calculate Rotation");

            // Update Control information
            if (HasHighestPriority)
            {
                NextYawAngle = CalculateNewAngle(CurrentTurn, _transform.Rotation, deltaTime);
                DeltaTurnAngle = Mathf.DeltaAngle(NextYawAngle, TargetYawAngle);
                TargetYawAngle = CurrentTurn.YawAngle;
            }
            else
            {
                DeltaTurnAngle = 0;
            }
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// 最も高い優先度の回転が変更されたときの処理を行う．
        /// </summary>
        private void HandleLoseHighestPriority()
        {
            _turnLifecycle?.OnLoseHighestPriority();
        }

        /// <summary>
        /// 新しい最も高い優先度の回転を取得したときの処理を行う．
        /// </summary>
        /// <param name="highestTurn">新しい最も高い優先度の回転．</param>
        private void HandleAcquireHighestPriority(ITurn highestTurn)
        {
            var turnLifeCycle = highestTurn as IPriorityLifecycle<ITurn>;
            turnLifeCycle?.OnAcquireHighestPriority();
            _turnLifecycle = turnLifeCycle;
        }

        /// <summary>
        /// 新しい角度を計算する．
        /// </summary>
        /// <param name="turn">回転コンポーネント．</param>
        /// <param name="rotation">現在の回転．</param>
        /// <param name="deltaTime">デルタタイム．</param>
        /// <returns>計算された新しい角度．</returns>
        private float CalculateNewAngle(in ITurn turn, in Quaternion rotation, float deltaTime)
        {
            return turn.TurnSpeed < 0
                ? turn.YawAngle
                : Mathf.LerpAngle(rotation.eulerAngles.y, turn.YawAngle, turn.TurnSpeed * deltaTime);
        }
    }
}