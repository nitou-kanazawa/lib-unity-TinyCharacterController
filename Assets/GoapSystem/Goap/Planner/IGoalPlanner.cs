using System.Collections.Generic;

namespace Nitou.Goap
{
    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
    }
}