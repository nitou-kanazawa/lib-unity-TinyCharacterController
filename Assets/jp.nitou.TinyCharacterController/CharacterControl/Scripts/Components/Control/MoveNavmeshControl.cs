using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.TCC.Foundation;
using Sirenix.OdinInspector;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// Component for Navmesh-based movement of a character.
    ///
    /// Uses the component specified by <see cref="_agent"/> to perform a path search to
    /// the coordinates specified by <see cref="SetTargetPosition(Vector3)"/> and move the character
    /// in the context of <see cref="IMove"/>.
    /// 
    /// If MovePriority is high, the character moves on the shortest path set by NavmeshAgent.
    /// /// If TurnPriority is high, the character will turn in the direction of the destination.
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(MoveNavmeshControl))]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Transform))]
    // [RequireInterface(typeof(IBrain))]
    public sealed class MoveNavmeshControl : ComponentBase,
                                      IMove,
                                      ITurn,
                                      IUpdateComponent,
                                      IComponentCondition
    {
        /// <summary>
        /// Agent used to control character movement.
        /// Used to calculate paths asynchronously.
        /// Also, the component used in this setting cannot be shared by multiple components.
        /// The Agent set here should be registered as a child object of the character.
        /// </summary>
        [SerializeField, Indent] private NavMeshAgent _agent;

        /// <summary>
        /// Maximum character movement speed
        /// </summary>
        [Title("Settings")]
        [SerializeField, Indent] private float _speed = 4;

        /// <summary>
        /// Character turn speed.
        /// </summary>
        [Range(-1, 50)]
        [SerializeField, Indent] public int TurnSpeed = 8;

        /// <summary>
        /// Character move priority.
        /// </summary>
        [Title("movement and orientation")]
        [GUIColor("green")]
        [SerializeField, Indent] public int MovePriority = 1;

        /// <summary>
        /// Character Turn Priority.
        /// </summary>
        [GUIColor("green")]
        [SerializeField, Indent] public int TurnPriority = 1;

        /// <summary>
        /// Callback when destination is reached
        /// </summary>
        public UnityEvent OnArrivedAtDestination;

        private float _yawAngle;
        private Vector3 _moveVelocity;


        /// <summary>
        /// True if the character has reached the target point.
        /// </summary>
        public bool IsArrived { get; private set; } = true;

        /// <summary>
        /// Character movement speed
        /// </summary>
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                _agent.speed = _speed;
            }
        }

        /// <summary>
        /// Set a target point to move to.
        /// </summary>
        /// <param name="position">Target position</param>
        public void SetTargetPosition(Vector3 position)
        {
            _agent.isStopped = false;
            _agent.SetDestination(position);
            IsArrived = false;
        }

        /// <summary>
        /// Set the target point to move to.
        /// and, the character maintains the <see cref="distance"/> distance.
        /// Use when you do not necessarily want to move to the same coordinates as the target, for example in melee combat.
        /// </summary>
        /// <param name="position">Position of target</param>
        /// <param name="distance">distance from the target</param>.
        public void SetTargetPosition(Vector3 position, float distance)
        {
            var deltaPosition = Transform.Position - position;
            deltaPosition.y = 0;
            var direction = deltaPosition.normalized * distance;
            SetTargetPosition(position + direction);
        }

        /// <summary>
        /// Current Speed.
        /// </summary>
        public float CurrentSpeed => IsArrived ? 0 : _speed;

        /// <summary>
        /// Movement vector
        /// </summary>
        public Vector3 MoveVelocity => _moveVelocity;

        int IPriority<IMove>.Priority => MovePriority;
        int IPriority<ITurn>.Priority => TurnPriority;

        int ITurn.TurnSpeed => TurnSpeed;
        float ITurn.YawAngle => _yawAngle;

        int IUpdateComponent.Order => Order.Control;


        #region Lifecycle Events

        protected override void OnComponentInitialized()
        {
            if (_agent == null)
            {
                var agent = new GameObject("agent", typeof(NavMeshAgent));
                agent.transform.SetParent(transform);
                agent.TryGetComponent(out _agent);
            }

            _agent.transform.localPosition = Vector3.zero;
            _agent.speed = _speed;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        void IUpdateComponent.OnUpdate(float deltaTime)
        {
            using var profiler = new ProfilerScope(nameof(MoveNavmeshControl));

            if (_agent.pathPending)
                return;
            var distance = _agent.remainingDistance;

            if (_agent.isOnOffMeshLink)
            {
                var deltaPosition = _agent.currentOffMeshLinkData.endPos - Transform.Position;
                deltaPosition.y = 0;
                var direction = deltaPosition.normalized;

                if (deltaPosition.sqrMagnitude < 1)
                    _agent.CompleteOffMeshLink();

                _moveVelocity = direction * _speed;
                _agent.nextPosition = Transform.Position + _moveVelocity * deltaTime;
            }
            else
            {
                var deltaPosition = _agent.steeringTarget - Transform.Position;
                deltaPosition.y = 0;

                var isMoving = distance - deltaTime * _speed > 0;
                if (isMoving && IsArrived == false)
                {
                    _yawAngle = Vector3.SignedAngle(Vector3.forward, deltaPosition, Vector3.up);
                }
                else
                {
                    // Process stopped because speed is 0 or target point has been reached.
                    if (IsArrived == false)
                    {
                        OnArrivedAtDestination?.Invoke();
                        IsArrived = true;
                        _agent.isStopped = true;
                    }
                }

                // var speed = _agent.desiredVelocity.magnitude;
                var currentSpeed = distance < Speed * deltaTime ? distance / deltaTime : Speed;
                // _moveVelocity = direction * currentSpeed;
                _moveVelocity = _agent.desiredVelocity.normalized * currentSpeed;
                _agent.nextPosition = Transform.Position + _moveVelocity * deltaTime;
            }
        }

        #endregion


        void IComponentCondition.OnConditionCheck(List<string> messageList)
        {
            if (_agent != null && _agent.transform.parent != transform)
            {
                messageList.Add("Please place the agent as a child object of the character.");
            }
        }


#if UNITY_EDITOR
        private void Reset()
        {
            if (_agent == null)
            {
                var agent = new GameObject("Agent (NavigationControl)", typeof(NavMeshAgent));
                agent.transform.SetParent(transform, false);
                agent.TryGetComponent(out _agent);
                _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                _agent.speed = 0;
                _agent.acceleration = 0;
                _agent.stoppingDistance = 0;
                _agent.autoBraking = false;
            }
        }

#if TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false)
                return;

            var position = _agent.destination;
            var center = position + new Vector3(0, 1, 0);
            var cubeSize = new Vector3(0.5f, 2f, 0.5f);

            NGizmo.DrawCube(center, cubeSize, Color.yellow);

            if (_agent.path.status == NavMeshPathStatus.PathComplete)
            {
                var corners = _agent.path.corners;
                if (corners.Length > 0)
                {
                    for (var i = 1; i < corners.Length; i++)
                    {
                        var start = corners[i - 1];
                        var next = corners[i];
                        Gizmos.DrawLine(start, next);
                    }
                }
            }
        }
#endif
#endif
    }
}