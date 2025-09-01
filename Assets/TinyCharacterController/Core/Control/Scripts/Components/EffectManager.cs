using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Core
{
    internal sealed class EffectManager
    {
        private readonly List<IEffect> _components = new(); // List of additional acceleration components

        public Vector3 Velocity { get; private set; }

        public bool IsInitialized { get; private set; } = false;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 初期化処理．
        /// </summary>
        /// <param name="obj">ルートオブジェクト．</param>
        public void Initialize(GameObject obj)
        {
            obj.GetComponentsInChildren(_components);
            IsInitialized = true;
        }

        /// <summary>
        /// 速度計算．
        /// </summary>
        public void CalculateVelocity()
        {
            using var _ = new ProfilerScope("Velocity Calculation");

            SumVelocities(_components, out var velocity);
            Velocity = velocity;
        }

        /// <summary>
        /// 速度のリセット．
        /// </summary>
        public void ResetVelocity()
        {
            foreach (var effect in _components)
                effect.ResetVelocity();
        }


        // ----------------------------------------------------------------------------
        // Private Method
        private static void SumVelocities(in List<IEffect> velocities, out Vector3 sum)
        {
            sum = Vector3.zero;
            foreach (var velocity in velocities)
                sum += velocity.Velocity;
        }
    }
}