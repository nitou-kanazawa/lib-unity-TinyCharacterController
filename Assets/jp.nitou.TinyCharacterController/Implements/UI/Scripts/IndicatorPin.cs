using UnityEngine;
using UnityEngine.Serialization;
using Nitou.TCC.Controller.Shared;
using Nitou.BatchProcessor;
using Sirenix.OdinInspector;
using Nitou.Gizmo;

namespace Nitou.TCC.Implements.UI
{
    /// <summary>
    /// Component for synchronizing UI with 3D space coordinates.
    /// Adjusts the position of the UI to align with the coordinates specified in <see cref="WorldPosition"/>.
    /// Uses <see cref="CanvasGroup"/> to hide the UI when it goes off-screen.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu(MenuList.Ui + nameof(IndicatorPin))]
    public sealed class IndicatorPin : ComponentBase
    {
        /// <summary>
        /// Timing at which the component is updated.
        /// Set to Update if the camera moves during the Update frame, or FixedUpdate if it moves during the Physics frame.
        /// </summary>
        [EnumToggleButtons]
        [SerializeField] private UpdateTiming _cameraUpdateTiming;

        /// <summary>
        /// World coordinates of the UI.
        /// </summary>
        [SerializeField] private Vector3 _worldPosition;

        /// <summary>
        /// Offset for the world coordinates of the UI.
        /// The UI is displayed at the position defined by <see cref="_worldPosition"/> with <see cref="_positionOffset"/> added to it.
        /// </summary>
        [SerializeField] private Vector3 _positionOffset;

        private RectTransform _transform;
        private CanvasGroup _group;
        private bool _isVisible = true;


        /// <summary>
        /// Position where the UI is displayed.
        /// </summary>
        public Vector3 WorldPosition
        {
            get => _worldPosition;
            set
            {
                // If the coordinates haven't been updated, exit the processing.
                if (_worldPosition == value)
                    return;

                _worldPosition = value;
                IsChangePosition = true;

                // Update the system's coordinates
                if (IsRegistered)
                    IndicatorPinSystem.GetInstance(_cameraUpdateTiming).SetPosition(Index, CorrectedPosition);
            }
        }

        /// <summary>
        /// UI's world coordinates adjusted with the offset.
        /// </summary>
        public Vector3 CorrectedPosition => _worldPosition + _positionOffset;

        /// <summary>
        /// UI size.
        /// </summary>
        public Vector2 UiSize => _transform.sizeDelta;


        /// <summary>
        /// True if UI coordinates have changed, automatically set to False after updating.
        /// </summary>
        public bool IsChangePosition { get; private set; }


        // ----------------------------------------------------------------------------

        #region Unity Lifecycle

        private void Awake()
        {
            TryGetComponent(out _transform);
            TryGetComponent(out _group);
        }

        private void OnEnable()
        {
            IndicatorPinSystem.Register(this, _cameraUpdateTiming);

            IsChangePosition = true;
            _group.alpha = 0;
            _isVisible = false;
        }

        private void OnDisable()
        {
            IndicatorPinSystem.Unregister(this, _cameraUpdateTiming);
        }

        private void OnValidate()
        {
            // Cancel update if the component is not registered.
            if (IsRegistered == false)
                return;

            // Reflect UI coordinates.
            IndicatorPinSystem.GetInstance(_cameraUpdateTiming).SetPosition(Index, CorrectedPosition);
        }

        #endregion


        // ----------------------------------------------------------------------------

        /// <summary>
        /// Apply changes to the UI.
        /// </summary>
        /// <param name="isVisible"></param>
        public void ApplyUi(bool isVisible)
        {
            // Transform updates are handled on the system side.

            if (isVisible)
            {
                if (_isVisible == false)
                    _group.alpha = 1;
            }
            else
            {
                if (_isVisible)
                    _group.alpha = 0;
            }

            _isVisible = isVisible;
            IsChangePosition = false;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            // Show the world coordinates where the UI is displayed.
            var position = _worldPosition + _positionOffset;
            NGizmo.DrawSphere(position, 0.2f, Color.cyan);
        }
#endif
    }
}