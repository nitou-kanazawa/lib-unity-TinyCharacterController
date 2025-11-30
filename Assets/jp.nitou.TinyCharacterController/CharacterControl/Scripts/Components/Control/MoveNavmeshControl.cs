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
    /// Navmeshベースのキャラクター移動コンポーネント。
    ///
    /// <see cref="_agent"/>で指定されたコンポーネントを使用して、
    /// <see cref="SetTargetPosition(Vector3)"/>で指定された座標への経路探索を実行し、
    /// <see cref="IMove"/>のコンテキストでキャラクターを移動させます。
    /// 
    /// MovePriorityが高い場合、キャラクターはNavmeshAgentによって設定された最短経路で移動します。
    /// TurnPriorityが高い場合、キャラクターは目的地の方向に向きを変えます。
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
        /// キャラクターの移動を制御するために使用されるエージェント。
        /// 非同期で経路を計算するために使用されます。
        /// また、この設定で使用されるコンポーネントは複数のコンポーネントで共有できません。
        /// ここで設定するAgentは、キャラクターの子オブジェクトとして登録する必要があります。
        /// </summary>
        [SerializeField, Indent] private NavMeshAgent _agent;

        /// <summary>
        /// キャラクターの最大移動速度
        /// </summary>
        [Title("Settings")]
        [SerializeField, Indent] private float _speed = 4;

        /// <summary>
        /// キャラクターの回転速度
        /// </summary>
        [Range(-1, 50)]
        [SerializeField, Indent] public int TurnSpeed = 8;

        /// <summary>
        /// キャラクターの移動優先度
        /// </summary>
        [Title("movement and orientation")]
        [GUIColor("green")]
        [SerializeField, Indent] public int MovePriority = 1;

        /// <summary>
        /// キャラクターの回転優先度
        /// </summary>
        [GUIColor("green")]
        [SerializeField, Indent] public int TurnPriority = 1;

        /// <summary>
        /// 目的地に到達したときのコールバック
        /// </summary>
        public UnityEvent OnArrivedAtDestination;

        private float _yawAngle;
        private Vector3 _moveVelocity;


        #region Peoperty

        /// <summary>
        /// キャラクターが目標地点に到達した場合にtrue
        /// </summary>
        public bool IsArrived { get; private set; } = true;

        /// <summary>
        /// キャラクターの移動速度
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
        /// 現在の速度
        /// </summary>
        public float CurrentSpeed => IsArrived ? 0 : _speed;

        /// <summary>
        /// 移動ベクトル
        /// </summary>
        public Vector3 MoveVelocity => _moveVelocity;

        int IPriority<IMove>.Priority => MovePriority;
        int IPriority<ITurn>.Priority => TurnPriority;

        int ITurn.TurnSpeed => TurnSpeed;
        float ITurn.YawAngle => _yawAngle;

        int IUpdateComponent.Order => Order.Control;

        #endregion


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
                    // 速度が0または目標地点に到達したため、処理を停止
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

        /// <summary>
        /// 移動先の目標地点を設定します。
        /// </summary>
        /// <param name="position">目標位置</param>
        public void SetTargetPosition(Vector3 position)
        {
            _agent.isStopped = false;
            _agent.SetDestination(position);
            IsArrived = false;
        }

        /// <summary>
        /// 移動先の目標地点を設定します。
        /// また、キャラクターは<see cref="distance"/>の距離を維持します。
        /// 目標と同じ座標に移動する必要がない場合に使用します（例：近接戦闘など）。
        /// </summary>
        /// <param name="position">目標の位置</param>
        /// <param name="distance">目標からの距離</param>
        public void SetTargetPosition(Vector3 position, float distance)
        {
            var deltaPosition = Transform.Position - position;
            deltaPosition.y = 0;
            var direction = deltaPosition.normalized * distance;
            SetTargetPosition(position + direction);
        }

        void IComponentCondition.OnConditionCheck(List<string> messageList)
        {
            if (_agent != null && _agent.transform.parent != transform)
            {
                messageList.Add("エージェントをキャラクターの子オブジェクトとして配置してください。");
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