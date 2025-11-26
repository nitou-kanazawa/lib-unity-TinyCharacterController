using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitou.Goap
{
    public class GoapPlanner : IGoapPlanner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goals"></param>
        /// <param name="mostRecentGoal"></param>
        /// <returns></returns>
        public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
        {
            // Order goals by priority, desending
            var orderedGoals = goals
                               .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
                               // 直近で達成した目標は少し低く調整
                               .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)
                               .ToList();

            // Try to solve each goal in order
            foreach (var goal in orderedGoals)
            {
                var goalNode = new Node(null, null, goal.DesiredEffects, 0);

                // If we can fing a path to the goal, return the plan
                if (FindPath(goalNode, agent.actions))
                {
                    // If the goalNode has no leaves and no action to perform try a different goal
                    if(goalNode.IsLeafDead) continue;
                    
                    var actionStack = new Stack<AgentAction>();
                    while (goalNode.Leaves.Count > 0)
                    {
                        var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                        goalNode = cheapestLeaf;
                        actionStack.Push(cheapestLeaf.Action);
                    }
                    
                    return new ActionPlan(goal, actionStack, goalNode.Cost);
                }
            }
            
            Debug.LogWarning("No plan found.");
            return null;
        }

        private bool FindPath(Node parent, HashSet<AgentAction> actions)
        {
            foreach (var action in actions)
            {
                var requiredEffects = parent.RequiredEffects;

                // Remove any effects that evaluate to true, there is no action to take
                requiredEffects.RemoveWhere(b => b.Evaluate());

                // If there are no required effects to fulfill, we have a plan
                if (requiredEffects.Count == 0)
                {
                    return true;
                }

                if (action.Effects.Any(requiredEffects.Contains))
                {
                    var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                    newRequiredEffects.ExceptWith(action.Effects);
                    newRequiredEffects.UnionWith(action.Preconditions);
                    
                    var newAvailableActions = new HashSet<AgentAction>(actions);
                    newAvailableActions.Remove(action);
                    
                    var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);
                    
                    // Explore the new node recursively
                    if (FindPath(newNode, newAvailableActions))
                    {
                        parent.Leaves.Add(newNode);
                        newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                    }
                }
            }
            return false;
        }


        private class Node
        {
            public Node Parent { get; }
            public AgentAction Action { get; }
            public HashSet<AgentBelief> RequiredEffects { get; }
            public List<Node> Leaves { get; }
            public float Cost { get; }

            public bool IsLeafDead => Leaves.Count == 0 && Action == null;

            public Node(Node parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
            {
                Parent = parent;
                Action = action;
                RequiredEffects = new HashSet<AgentBelief>(effects);
                Leaves = new List<Node>();
                Cost = cost;
            }
        }
    }
}