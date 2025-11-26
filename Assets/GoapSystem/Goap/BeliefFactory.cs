using System;
using System.Collections.Generic;
using Nitou.Goap.Components;
using UnityEngine;

namespace Nitou.Goap
{
    public class BeliefFactory
    {
        private readonly IGoapAgent _agent;
        private readonly Dictionary<string, AgentBelief> _beliefs = new();

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
        {
            if (agent == null)
                throw new ArgumentNullException();

            _agent = agent;
            beliefs = beliefs ?? throw new ArgumentNullException(nameof(beliefs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="condition"></param>
        public void AddBelief(string key, Func<bool> condition)
        {
            var belief = new AgentBelief.Builder(key)
                         .WithCondition(condition)
                         .Build();
            _beliefs.Add(key, belief);
        }

        // TODO: Add Sensor Beliefs

        public void AddSensorBelief(string key, Sensor sensor)
        {
            var belief = new AgentBelief.Builder(key)
                         .WithCondition(() => sensor.IsTargetInRange)
                         .WithLocation(() => sensor.TargetPosition)
                         .Build();
            _beliefs.Add(key, belief);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="distance"></param>
        /// <param name="position"></param>
        public void AddLocationBelief(string key, float distance, Vector3 position)
        {
            var belief = new AgentBelief.Builder(key)
                         .WithCondition(() => IsRangeOf(position, distance))
                         .WithLocation(() => position)
                         .Build();
            _beliefs.Add(key, belief);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="distance"></param>
        /// <param name="transform"></param>
        public void AddLocationBelief(string key, float distance, Transform transform)
        {
            AddLocationBelief(key, distance, transform.position);
        }

        /// <summary>
        /// エージェントが範囲内にいるかを判定する．
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool IsRangeOf(Vector3 position, float distance) => Vector3.Distance(_agent.Transform.position, position) <= distance;
    }
}