using System;
using System.Collections.Generic;

namespace Nitou.Goap
{
    /// <summary>
    /// 達成すべきゴール．
    /// </summary>
    public sealed class AgentGoal
    {
        public string Name { get; }

        public float Priority { get; private set; }

        /// <summary>
        /// 目標を達成したときに実行する効果．
        /// </summary>
        public HashSet<AgentBelief> DesiredEffects { get; } = new();

        private AgentGoal(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }


        public sealed class Builder
        {
            private readonly AgentGoal _agentGoal;

            public Builder(string name)
            {
                _agentGoal = new AgentGoal(name);
            }

            public Builder WithPriority(float priority)
            {
                _agentGoal.Priority = priority;
                return this;
            }

            public Builder WithDesiredEffect(AgentBelief effect)
            {
                _agentGoal.DesiredEffects.Add(effect);
                return this;
            }

            public AgentGoal Build()
            {
                return _agentGoal;
            }
        }
    }
}