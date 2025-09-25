using Nitou.BatchProcessor;
using Nitou.TCC.Controller.Shared;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Implements.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu(MenuList.Ui + nameof(Indicator))]
    public sealed class Indicator : ComponentBase
    {
        /// <summary>
        /// コンポーネントの更新タイミング．
        /// キャラクターがUpdateタイミングで動く場合はUpdate、物理タイミングで動く場合はFixedUpdateに設定します。
        /// </summary>
        [EnumToggleButtons]
        [SerializeField] private UpdateTiming _updateTiming;

        /// <summary>
        /// 追跡するTransform。
        /// 設定されていない場合、Indicatorは動作を停止します。
        /// </summary>
        [Header("Target to Track")]
        [SerializeField] private Transform _target;

        /// <summary>
        /// Indicatorが表示される位置のオフセット。
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// trueの場合、Indicatorは画面境界内に収まるように位置を調整します。
        /// </summary>
        [Header("Tracking Range")]
        [SerializeField] private bool _isLimitIconRange;

        /// <summary>
        /// <see cref="IsLimitIconRange"/>がtrueの場合、UIを調整する位置を設定します。
        /// 例：0.9に設定すると画面の90%の位置で調整されます。
        /// </summary>
        [SerializeField] private float _bounds = 0.9f;

        /// <summary>
        /// UIが画面内または画面外にある時に呼び出されるイベント。
        /// </summary>
        public UnityEvent<bool> OnValueChanged = new();

        /// <summary>
        /// ターゲットが見える時に使用するアイコン。
        /// </summary>
        [Header("Behavior When Off-Screen")]
        [SerializeField]
        private Transform _onScreenIcon;

        /// <summary>
        /// ターゲットが見えない時に使用するアイコン。
        /// </summary>
        [SerializeField] private Transform _offScreenIcon;

        /// <summary>
        /// <see cref="IsTargetVisible"/>がfalseの時、アイコンがターゲットの方向に回転します。
        /// </summary>
        [SerializeField] private bool _isTurnOffScreenIcon;

        private RectTransform _transform;
        private bool _isInitialized = false;

        /// <summary>
        /// 境界の設定。
        /// </summary>
        public float Bounds => _bounds;

        /// <summary>
        /// trueの場合、Indicatorは画面境界内に収まるように位置を調整します。
        /// </summary>
        public bool IsLimitIconRange => _isLimitIconRange;

        /// <summary>
        /// ターゲットが画面上に見えているかどうか。
        /// </summary>
        public bool IsTargetVisible { get; private set; }

        /// <summary>
        /// ターゲットのTransformを指定します。
        /// Nullに設定するとIndicatorの動作を停止します。
        /// </summary>
        public Transform Target
        {
            get => _target;
            set
            {
                var hasTarget = value != null;
                var isChangedTarget = _target != value;

                _target = value;

                // If the target being tracked by an already active Indicator changes, re-register the unit once.
                if (isChangedTarget && ((IComponentIndex)this).IsRegistered && hasTarget)
                {
                    IndicatorSystem.Unregister(this, _updateTiming);
                    IndicatorSystem.Register(this, _updateTiming);
                }

                enabled = hasTarget;
            }
        }

        /// <summary>
        /// ターゲットの更新タイミング。
        /// </summary>
        public UpdateTiming FollowTargetUpdateTiming
        {
            get => _updateTiming;
            set
            {
                // Exit the process if there is no change.
                if (_updateTiming == value)
                    return;

                // If the component is active, unregister it from the system and re-assign it to another timing.
                if (((IComponentIndex)this).IsRegistered)
                {
                    var preTiming = _updateTiming;
                    var newTiming = _updateTiming == UpdateTiming.Update
                        ? UpdateTiming.Update
                        : UpdateTiming.FixedUpdate;

                    IndicatorSystem.Unregister(this, preTiming);
                    IndicatorSystem.Register(this, newTiming);
                }

                _updateTiming = value;
            }
        }

        /// <summary>
        /// trueの場合、画面外の時にアイコンが回転する．
        /// </summary>
        public bool IsTurnOffscreenIcon => _isTurnOffScreenIcon;


        // ----------------------------------------------------------------------------

        #region Unity Lifecycle
        
        private void Awake()
        {
            TryGetComponent(out _transform);


        }

        private void OnEnable()
        {
            // If there is no target, immediately stop processing.
            if (InitializeCheck() && _target == null)
            {
                enabled = false;
                return;
            }

            // Register the component.
            IndicatorSystem.Register(this, _updateTiming);

            // Switch between on-screen and off-screen icons.
            UpdateIconDisplay();
        }
        
        private void OnDisable()
        {
            // Unregister the component.
            IndicatorSystem.Unregister(this, _updateTiming);
        }

        /// <summary>
        /// Indicatorが表示する設定を更新します。
        /// </summary>
        /// <param name="iconAngle">アイコンの角度。</param>
        /// <param name="isTargetVisible">UIが画面外に表示されているかどうか。</param>
        public void OnUpdate(float iconAngle, bool isTargetVisible)
        {
            // Update properties.
            var isChangeVisible = IsTargetVisible != isTargetVisible;
            IsTargetVisible = isTargetVisible;

            // Apply transform.
            if (_offScreenIcon != null)
                _offScreenIcon.localRotation = Quaternion.AngleAxis(iconAngle, Vector3.forward);
            ;

            // If the display status has changed, update the icon and trigger the event.
            if (isChangeVisible)
            {
                UpdateIconDisplay();
                OnValueChanged.Invoke(IsTargetVisible);
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        /// <summary>
        /// 動作するためのチェック
        /// </summary>
        /// <returns>動作可能ならばTrue</returns>
        private bool InitializeCheck()
        {
            if (_isInitialized == false)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("UI requires Canvas.", gameObject);
                    return false;
                }

                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogError("Canvas is not a ScreenSpaceOverlay.", gameObject);
                    return false;
                }
            }

            _isInitialized = true;
            return _isInitialized;
        }

 

        private void OnDrawGizmosSelected()
        {
            // Stop processing if there is no target.
            if (_target == null)
                return;

            // Represent a sphere at the target's position including the offset.
            var center = _target.position + Offset;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(center, 0.5f);

            // Represent a filled sphere.
            Gizmos.color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.4f);
            Gizmos.DrawSphere(center, 0.5f);
        }

        /// <summary>
        /// <see cref="IsTargetVisible"/>に基づいてアイコンの表示を切り替えます。
        /// <see cref="_isLimitIconRange"/>がfalseの場合は動作しません。
        /// </summary>
        private void UpdateIconDisplay()
        {
            if (!_isLimitIconRange)
                return;

            // Switch the icon.
            if (_onScreenIcon != null)
                _onScreenIcon.gameObject.SetActive(IsTargetVisible);
            if (_offScreenIcon != null)
                _offScreenIcon.gameObject.SetActive(!IsTargetVisible);
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (((IComponentIndex)this).IsRegistered && IndicatorSystem.IsCreated(_updateTiming))
                IndicatorSystem.GetInstance(_updateTiming)
                               .SetIndicatorData(this, Bounds, Offset, IsTurnOffscreenIcon, IsLimitIconRange);
        }
#endif
    }
}