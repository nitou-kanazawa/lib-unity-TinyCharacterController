using System.Collections.Generic;
using UnityEngine;
using Nitou.TCC.Utils;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Interfaces.Components;

namespace Nitou.TCC.Controller.Core
{
    internal class MoveManager
    {
        private IPriorityLifecycle<IMove> _moveLifeCycle;

        // List of components for controlling character movement
        private readonly List<IMove> _moves = new();

        public bool HasHighestPriority { get; private set; } = false;

        public float CurrentSpeed { get; private set; }

        public IMove CurrentMove { get; private set; }

        public Vector3 Velocity { get; private set; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 初期化処理．
        /// </summary>
        public void Initialize(GameObject obj)
        {
            obj.GetComponentsInChildren(_moves);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateHighestMoveControl(float deltaTime)
        {
            using var _ = new ProfilerScope("Update Highest MoveControl");

            HasHighestPriority = _moves.TryGetHighestPriority(out var highestMove);
            var isChangeHighestPriorityComponent = CurrentMove != highestMove;

            // Action when a component with the highest priority changes
            if (isChangeHighestPriorityComponent)
            {
                HandleLoseHighestPriority();
                HandleAcquireHighestPriority(highestMove);
            }

            CurrentMove = highestMove;

            // Perform Update for the component with the highest priority
            if (HasHighestPriority)
                _moveLifeCycle?.OnUpdateWithHighestPriority(deltaTime);
        }

        /// <summary>
        /// 速度計算．
        /// </summary>
        public void CalculateVelocity()
        {
            // Update movement vector with the highest priority component.
            using var _ = new ProfilerScope("Control Calculation");

            // Update Control information
            Velocity = HasHighestPriority ? CurrentMove.MoveVelocity : Vector3.zero;
            CurrentSpeed = HasHighestPriority ? Velocity.magnitude : 0;
        }


        // ----------------------------------------------------------------------------
        // Private Method
        private void HandleLoseHighestPriority()
        {
            _moveLifeCycle?.OnLoseHighestPriority();
        }

        private void HandleAcquireHighestPriority(IMove highestMove)
        {
            var moveLifeCycle = highestMove as IPriorityLifecycle<IMove>;
            moveLifeCycle?.OnAcquireHighestPriority();
            _moveLifeCycle = moveLifeCycle;
        }
    }
}