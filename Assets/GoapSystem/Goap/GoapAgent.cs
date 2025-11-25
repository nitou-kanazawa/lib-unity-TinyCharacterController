using System;
using System.Collections.Generic;
using Nitou.Goap.Components;
using Nitou.Goap.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace Nitou.Goap
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AnimationController))]
    public sealed class GoapAgent : MonoBehaviour
    {
        [Serializable]
        public class Status
        {
            public float health = 100;
            public float stamina = 100;
        }


        [Header("Sensor")]
        [SerializeField] private Sensor _chaseSonsor;

        [SerializeField] private Sensor _attackSonsor;

        [Header("Known Locations")]
        [SerializeField] private Transform _restingPosition;

        [SerializeField] private Transform _foodShack;
        [SerializeField] private Transform _doorOnePosition;
        [SerializeField] private Transform _doorTwoPosition;

        [Header("Stats")]
        [SerializeField] private Status _state;

        // Components
        private NavMeshAgent _navMeshAgent;
        private AnimationController _animations;
        private Rigidbody _rigidbody;


        private CountdownTimer _statsTimer;

        private GameObject _target;
        private Vector3 _destination;

        // Goals
        private AgentGoal _lastGoal;
        public AgentGoal currentGoal;
        public ActionPlan actionPlan;
        public AgentAction currentAction;


        public Dictionary<string, AgentBelief> _beliefs;


        #region Lifecycle Events

        private void Awake()
        {
            GatherComponents();
            _rigidbody.freezeRotation = true;
        }

        private void Start()
        {
            SetupBeliefs();
            SetupActions();
            SetupGoals();
        }
        
        #endregion

        private void SetupBeliefs()
        {
        }

        private void SetupActions()
        {
        }

        private void SetupGoals()
        {
        }

        private void GatherComponents()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animations = GetComponent<AnimationController>();
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}