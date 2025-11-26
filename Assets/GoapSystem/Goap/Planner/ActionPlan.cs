using System.Collections.Generic;

namespace Nitou.Goap
{
    public class ActionPlan
    {
        /// <summary>
        /// 解決したい目標．
        /// </summary>
        public AgentGoal AgentGoal { get; }
        
        /// <summary>
        /// 実行できるアクション．
        /// </summary>
        public Stack<AgentAction> Actions { get; }
        
        /// <summary>
        /// 総コスト．
        /// </summary>
        public float TotalCost { get; set; }

        public ActionPlan(AgentGoal agentGoal, Stack<AgentAction> actions, float totalcos)
        {
            AgentGoal = agentGoal;
            Actions = actions;
            TotalCost = totalcos;
        }
    }
}