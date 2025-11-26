using UnityEngine;
using UnityEngine.AI;
using Nitou.BatchProcessor;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Shared;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// <see cref="UnityEngine.AI.NavMeshAgent"/> を使用して動作する Brain．
    /// Agentの高さと幅は <see cref="CharacterSettings.Height"/> と <see cref="CharacterSettings.Radius"/> によって決定される．
    /// 正しく機能するには、<see cref="UnityEngine.AI.NavMeshAgent"/> が必要．
    /// </summary>
    [AddComponentMenu(MenuList.MenuBrain + "Navmesh Brain")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [DefaultExecutionOrder(Order.UpdateBrain)]
    [RequireComponent(typeof(CharacterSettings))]
    public sealed class NavmeshBrain : BrainBase, IActorSettingUpdateReceiver
    {
        private NavMeshAgent _agent;
        private EarlyUpdateBrainBase _earlyUpdate;

        private Quaternion _rotation;
        private Vector3 _position;

        /// <summary>
        /// コンポーネントの更新タイミング．
        /// </summary>
        public override UpdateTiming Timing => UpdateTiming.Update;


        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        private void Reset()
        {
            var agent = GetComponent<NavMeshAgent>();
            agent.avoidancePriority = 0;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            agent.autoTraverseOffMeshLink = false;
            agent.autoBraking = false;
            agent.autoRepath = false;
            agent.baseOffset = -0.05f;
            ((IActorSettingUpdateReceiver)this).OnUpdateSettings(GetComponent<CharacterSettings>());
        }

        private void Awake()
        {
            base.Initialize();
            TryGetComponent(out _agent);
            _earlyUpdate = gameObject.GetOrAddComponent<EarlyUpdateBrain>();
        }

        private void OnEnable()
        {
            _earlyUpdate.enabled = true;
        }

        private void OnDisable()
        {
            _earlyUpdate.enabled = false;
        }

        private void Update()
        {
            UpdateBrain();
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region Protected Method

        protected override void ApplyPosition(in Vector3 totalVelocity, float deltaTime)
        {
            if (_agent.enabled)
                _agent.Move(totalVelocity * deltaTime);
            else
                CachedTransform.position += totalVelocity * deltaTime;
        }


        protected override void ApplyRotation(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        protected override void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            position = CachedTransform.position;
            rotation = CachedTransform.rotation;
        }

        #endregion

        void IActorSettingUpdateReceiver.OnUpdateSettings(CharacterSettings settings)
        {
            TryGetComponent(out NavMeshAgent agent);

            agent.radius = settings.Radius;
            agent.height = settings.Height;
        }

        protected override void SetPositionDirectly(in Vector3 position)
        {
            if (_agent.enabled)
                _agent.Warp(position);
            else
                CachedTransform.position = position;
        }

        protected override void SetRotationDirectly(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        protected override void MovePosition(in Vector3 newPosition)
        {
            var deltaPosition = newPosition - Position;
            _agent.Move(deltaPosition);
        }
    }
}