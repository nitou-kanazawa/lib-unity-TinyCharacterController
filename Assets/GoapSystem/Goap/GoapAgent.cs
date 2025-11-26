using System;
using System.Collections.Generic;
using Nitou.Goap.Components;
using Nitou.Goap.Utilities;
using R3;
using UnityEngine;
using UnityEngine.AI;

namespace Nitou.Goap
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AnimationController))]
    public sealed class GoapAgent : MonoBehaviour, IGoapAgent
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

        // キャッシュ
        public Dictionary<string, AgentBelief> beliefs;
        public HashSet<AgentAction> actions;
        public HashSet<AgentGoal> goals;


        Transform IGoapAgent.Transform => transform;

        #region Lifecycle Events

        private void Awake()
        {
            GatherComponents();
            _rigidbody.freezeRotation = true;
        }

        private void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(2f))
                      .Subscribe(_ => UpdateStats())
                      .AddTo(this);

            // 
            SetupBeliefs();
            SetupActions();
            SetupGoals();

            _chaseSonsor.OnTargetChanged
                        .Where(_ => this.isActiveAndEnabled)
                        .Subscribe(_ => HandleTargetChanged())
                        .AddTo(this);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void SetupBeliefs()
        {
            beliefs = new Dictionary<string, AgentBelief>();
            var factory = new BeliefFactory(this, beliefs);

            // Register beliefs
            factory.AddBelief("Nothing", () => false);
            factory.AddBelief("AgentIdle", () => !_navMeshAgent.hasPath);
            factory.AddBelief("AgentMoving", () => _navMeshAgent.hasPath);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupActions()
        {
            actions = new HashSet<AgentAction>();

            new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy(5))
                .AddEffect(beliefs["Nothing"])
                .Build();

            new AgentAction.Builder("Wander Around")
                .WithStrategy(new WanderStrategy(_navMeshAgent, 10))
                .AddEffect(beliefs["AgentMoving"])
                .Build();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupGoals()
        {
            goals = new HashSet<AgentGoal>();

            goals.Add(new AgentGoal.Builder("Chill Out")
                      .WithPriority(1)
                      .WithDesiredEffect(beliefs["Nothing"])
                      .Build());

            goals.Add(new AgentGoal.Builder("Wander")
                      .WithPriority(1)
                      .WithDesiredEffect(beliefs["AgentMoving"])
                      .Build());
        }


        private void HandleTargetChanged()
        {
            Debug.Log("Target changed, clearing current action and goal.");

            // Force the planner to re-evaluate the plan
            currentAction = null;
            currentGoal = null;
        }

        // TODO: move to stats system

        private void UpdateStats()
        {
            // _state.stamina += 
            _state.stamina = Mathf.Clamp(_state.stamina, 0, 100);
            _state.health = Mathf.Clamp(_state.health, 0, 100);
        }

        private void GatherComponents()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animations = GetComponent<AnimationController>();
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}