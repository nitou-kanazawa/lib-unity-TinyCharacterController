using UnityEngine;
using UnityEngine.Serialization;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.BatchProcessor;
using Sirenix.OdinInspector;
using Nitou.Gizmo;

namespace Nitou.TCC.UI.UI
{
    /// <summary>
    /// UIを3D空間の座標と同期させるコンポーネント。
    /// <see cref="WorldPosition"/>で指定した座標にUIの位置を調整します。
    /// <see cref="CanvasGroup"/>を使用して、画面外に出た場合にUIを非表示にします。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu(MenuList.Ui + nameof(IndicatorPin))]
    public sealed class IndicatorPin : ComponentBase
    {
        /// <summary>
        /// コンポーネントの更新タイミング。
        /// カメラがUpdateフレームで移動する場合はUpdate、物理フレームで移動する場合はFixedUpdateに設定します。
        /// </summary>
        [EnumToggleButtons]
        [SerializeField] private UpdateTiming _cameraUpdateTiming;

        /// <summary>
        /// UIのワールド座標。
        /// </summary>
        [SerializeField] private Vector3 _worldPosition;

        /// <summary>
        /// UIのワールド座標に対するオフセット。
        /// UIは<see cref="_worldPosition"/>に<see cref="_positionOffset"/>を加算した位置に表示されます。
        /// </summary>
        [SerializeField] private Vector3 _positionOffset;

        private RectTransform _transform;
        private CanvasGroup _group;
        private bool _isVisible = true;


        /// <summary>
        /// UIが表示される位置。
        /// </summary>
        public Vector3 WorldPosition
        {
            get => _worldPosition;
            set
            {
                // 座標が更新されていない場合は処理を終了する。
                if (_worldPosition == value)
                    return;

                _worldPosition = value;
                IsChangePosition = true;

                // システムの座標を更新する
                if (IsRegistered)
                    IndicatorPinSystem.GetInstance(_cameraUpdateTiming).SetPosition(Index, CorrectedPosition);
            }
        }

        /// <summary>
        /// オフセットを適用したUIのワールド座標。
        /// </summary>
        public Vector3 CorrectedPosition => _worldPosition + _positionOffset;

        /// <summary>
        /// UIのサイズ。
        /// </summary>
        public Vector2 UiSize => _transform.sizeDelta;


        /// <summary>
        /// UI座標が変更されている場合はTrue。更新後は自動的にFalseに設定されます。
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
            // コンポーネントが登録されていない場合は更新をキャンセルする。
            if (IsRegistered == false)
                return;

            // UI座標を反映する。
            IndicatorPinSystem.GetInstance(_cameraUpdateTiming).SetPosition(Index, CorrectedPosition);
        }

        #endregion


        // ----------------------------------------------------------------------------

        /// <summary>
        /// UIに変更を適用する。
        /// </summary>
        /// <param name="isVisible">UIが表示されているかどうか</param>
        public void ApplyUi(bool isVisible)
        {
            // Transformの更新はシステム側で処理される。

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
            // UIが表示されるワールド座標を表示する。
            var position = _worldPosition + _positionOffset;
            NGizmo.DrawSphere(position, 0.2f, Color.cyan);
        }
#endif
    }
}