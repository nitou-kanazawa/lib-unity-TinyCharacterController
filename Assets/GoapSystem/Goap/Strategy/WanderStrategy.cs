using Nitou.Goap.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace Nitou.Goap
{
    public sealed class WanderStrategy : IActionStrategy
    {
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _wanderRadius;

        public bool CanPerform => !Complete;
        public bool Complete => _navMeshAgent.remainingDistance <= 2f && !_navMeshAgent.pathPending;

        public WanderStrategy(NavMeshAgent navMeshAgent, float wanderRadius)
        {
            _navMeshAgent = navMeshAgent;
            _wanderRadius = wanderRadius;
        }

        public void Start()
        {
            for (int i = 0; i < 5; i++)
            {
                var randomDirection = (Random.insideUnitSphere * _wanderRadius).With(y: 0);
                
                if (NavMesh.SamplePosition(_navMeshAgent.transform.position + randomDirection, out var hit, _wanderRadius, 1))
                {
                    _navMeshAgent.SetDestination(hit.position);
                    return;
                }
                
            }
        }
    }
}