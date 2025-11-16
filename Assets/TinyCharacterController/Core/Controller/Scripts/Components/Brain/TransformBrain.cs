using Nitou.BatchProcessor;
using UnityEngine;
using Nitou.TCC.Controller.Shared;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// <see cref="UnityEngine.Transform"/> を使用して動作する Brain．
    /// コライダーの接触による移動制限はない．
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(Order.UpdateBrain)]
    [AddComponentMenu(MenuList.MenuBrain + "Transform Brain")]
    [RequireComponent(typeof(CharacterSettings))]
    public sealed class TransformBrain : BrainBase
    {
        private EarlyUpdateBrainBase _earlyUpdate;

        public override UpdateTiming Timing => UpdateTiming.Update;


        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        private void Awake()
        {
            base.Initialize();
            GatherComponents();
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

        #region Override Methods

        protected override void ApplyPosition(in Vector3 totalVelocity, float deltaTime)
        {
            CachedTransform.position += totalVelocity * Time.deltaTime;
        }

        protected override void SetPositionDirectly(in Vector3 position)
        {
            CachedTransform.position = position;
        }

        protected override void SetRotationDirectly(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        protected override void MovePosition(in Vector3 newPosition)
        {
            SetPositionDirectly(newPosition);
        }

        protected override void ApplyRotation(in Quaternion rotation)
        {
            CachedTransform.rotation = rotation;
        }

        protected override void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            CachedTransform.GetPositionAndRotation(out position, out rotation);
        }

        #endregion


        private void GatherComponents()
        {
            _earlyUpdate = gameObject.GetOrAddComponent<EarlyUpdateBrain>();
        }
    }
}