using System.Collections.Generic;
using Nitou.TCC.Controller.Interfaces.Core;
using UnityEngine;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// <see cref="ITurn"/>を統括するマネージャークラス．
    /// </summary>
    internal class TurnManager
    {
        private ITransform _transform;
        private IPriorityLifecycle<ITurn> _turnLifecycle;
        private readonly List<ITurn> _turns = new();

        /// <summary>
        /// 現在アクティブなTurnコンポーネント.
        /// </summary>
        public ITurn CurrentTurn { get; private set; }

        /// <summary>
        /// Indicates whether the highest priority turn is held.
        /// </summary>
        public bool HasHighestPriority { get; private set; } = false;

        /// <summary>
        /// Indicates the target yaw angle.
        /// </summary>
        public float TargetYawAngle { get; private set; }

        /// <summary>
        /// Indicates the difference from the previous yaw angle.
        /// </summary>
        public float DeltaTurnAngle { get; private set; }

        /// <summary>
        /// Calculates the next yaw angle.
        /// </summary>
        public float NextYawAngle { get; private set; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 初期化処理．
        /// </summary>
        /// <param name="obj">The GameObject to retrieve turns from.</param>
        /// <param name="transform">The object's transform.</param>
        public void Initialize(GameObject obj, ITransform transform)
        {
            _transform = transform;
            obj.GetComponentsInChildren(_turns);
        }

        /// <summary>
        /// Updates the highest priority turn control.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last frame.</param>
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
        /// Handles actions when the highest priority turn changes.
        /// </summary>
        private void HandleLoseHighestPriority()
        {
            _turnLifecycle?.OnLoseHighestPriority();
        }

        /// <summary>
        /// Handles acquiring the new highest priority turn when it changes.
        /// </summary>
        /// <param name="highestTurn">The new highest priority turn.</param>
        private void HandleAcquireHighestPriority(ITurn highestTurn)
        {
            var turnLifeCycle = highestTurn as IPriorityLifecycle<ITurn>;
            turnLifeCycle?.OnAcquireHighestPriority();
            _turnLifecycle = turnLifeCycle;
        }

        /// <summary>
        /// Calculates the new angle.
        /// </summary>
        private float CalculateNewAngle(in ITurn turn, in Quaternion rotation, float deltaTime)
        {
            return turn.TurnSpeed < 0
                ? turn.YawAngle
                : Mathf.LerpAngle(rotation.eulerAngles.y, turn.YawAngle, turn.TurnSpeed * deltaTime);
        }
    }
}