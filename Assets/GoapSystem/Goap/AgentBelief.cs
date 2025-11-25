using System;
using UnityEngine;


namespace Nitou.Goap
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AgentBelief
    {
        private Func<bool> _condition = () => false;
        private Func<Vector3> _observedLocation = () => Vector3.zero;

        public string Name { get; }


        private AgentBelief(string name)
        {
            Name = name;
        }

        public bool Evaluate() => _condition();

        public sealed class Builder
        {
            private readonly AgentBelief _agentBelief;

            public Builder(string name)
            {
                _agentBelief = new AgentBelief(name);
            }

            public Builder WithCondition(Func<bool> condition)
            {
                _agentBelief._condition = condition;
                return this;
            }

            public Builder WithLocation(Func<Vector3> observedLocation)
            {
                _agentBelief._observedLocation = observedLocation;
                return this;
            }

            public AgentBelief Build()
            {
                return _agentBelief;
            }
        }
    }
}