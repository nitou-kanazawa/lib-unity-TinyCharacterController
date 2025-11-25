using System.Collections.Generic;

namespace Nitou.Goap
{
    /// <summary>
    /// Action は，<see cref="IActionStrategy"/>をラップする．
    /// </summary>
    public sealed class AgentAction
    {
        private IActionStrategy _strategy;

        public string Name { get; }
        public float Cost { get; private set; } = 1;

        /// <summary>
        /// アクションを実行できるかの前提条件．
        /// </summary>
        public HashSet<AgentBelief> Preconditions { get; } = new();
        
        /// <summary>
        /// アクションによって得られる効果．
        /// </summary>
        public HashSet<AgentBelief> Effects { get; } = new();

        public bool Complete => _strategy.Complete;

        private AgentAction(string name)
        {
            Name = name;
        }

        public void Start() => _strategy.Start();

        public void Update(float deltaTime)
        {
            // Check if the action can be performed and update the strategy
            if (_strategy.CanPerform)
            {
                _strategy.Update(deltaTime);
            }

            // Bail out if the strategy is still executing
            if (!_strategy.Complete) return;

            // Apply effects
            foreach (var effect in Effects)
            {
                effect.Evaluate();
            }
        }

        public void Stop() => _strategy.Stop();


        public sealed class Builder
        {
            private readonly AgentAction _agentAction;

            public Builder(string name)
            {
                _agentAction = new AgentAction(name);
            }

            public Builder WithCost(float cost)
            {
                _agentAction.Cost = cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy strategy)
            {
                _agentAction._strategy = strategy;
                return this;
            }

            public Builder AddPrecondition(AgentBelief precondition)
            {
                _agentAction.Preconditions.Add(precondition);
                return this;
            }

            public Builder AddEffect(AgentBelief effect)
            {
                _agentAction.Effects.Add(effect);
                return this;
            }

            public AgentAction Build()
            {
                return _agentAction;
            }
        }
    }
}