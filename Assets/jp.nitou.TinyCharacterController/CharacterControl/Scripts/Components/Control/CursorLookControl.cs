using UnityEngine;
using Sirenix.OdinInspector;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Shared;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// カーソル位置に基づいてキャラクターの向きを更新するコンポーネント。 <br/>
    /// 
    /// このコンポーネントが高い優先度を持つ場合、キャラクターはカーソルの方向を向きます。
    /// キャラクターが注視する座標は<see cref="LookTargetPoint"/>に基づいて計算されます。
    /// トップダウンではなくサイドビューを使用したい場合は、<see cref="_planeAxis"/>を変更してください。
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(CursorLookControl))]
    [DisallowMultipleComponent]
    public sealed class CursorLookControl : MonoBehaviour,
                                            ITurn, IUpdateComponent
    {
        [Required]
        [SerializeField, Indent] private Camera _camera;

        /// <summary>
        /// カーソルの最大距離．
        /// カメラの移動範囲を制限するために使用されます。
        /// 例えば、カメラをカーソル位置に追従させたい場合など。
        /// </summary>
        [Title("Cursor behavior settings")]
        [Tooltip("Maximum distance of cursor")]
        [SerializeField, Indent] private float _maxDistance = 3;

        /// <summary>
        /// カーソルへの向きを補正するためのオフセット．
        /// (0,0,0)の場合、モデルのルートからの向き．
        /// 例えば，銃の高さに基づいて向きを計算したい場合は，銃の高さに調整する．
        /// </summary>
        [SerializeField, Indent] private Vector3 _originOffset = Vector3.zero;

        [Title("Plane settings")]
        [SerializeField, Indent] private Vector3 _planeAxis = Vector3.up;

        [SerializeField, Indent] private float _planeOffset;

        [Title("Character orientation control")] 
        [GUIColor("green")] 
        [SerializeField, Indent] private int _turnPriority = 1;

        [PropertyRange(-1, 100)] 
        [SerializeField, Indent] private int _turnSpeed = 30;


        private Vector2 _mousePosition;
        
        private ITransform _transform;
        private CharacterSettings _characterSettings;


        // ----------------------------------------------------------------------------

        #region Property

        int IUpdateComponent.Order => Order.Control;

        /// <summary>
        /// カーソルに向く速度。値が-1の場合、向きは固定されます。
        /// </summary>
        public int TurnSpeed
        {
            get => _turnSpeed;
            set => _turnSpeed = value;
        }

        /// <summary>
        /// 回転の優先度。
        /// 他のコンポーネントの優先度よりも高い場合、カーソル方向を向きます。
        /// </summary>
        public int TurnPriority
        {
            get => _turnPriority;
            set => _turnPriority = value;
        }

        /// <summary>
        /// キャラクターを中心としてMaxDistance距離に制限されたカーソル位置。
        /// カメラがカーソルを追跡している場合に、カーソルが無限に離れていく問題を回避します。
        /// </summary>
        public Vector3 LimitedPosition { get; private set; }

        /// <summary>
        /// カーソルのワールド座標
        /// </summary>
        public Vector3 CursorPosition { get; private set; }

        /// <summary>
        /// カーソルへの回転（水平回転）
        /// </summary>
        public Quaternion YawRotation { get; private set; }

        /// <summary>
        /// カーソルへの回転（垂直回転）
        /// </summary>
        public Quaternion PitchRotation { get; private set; }

        /// <summary>
        /// 判定平面の方向。
        /// トップビューの場合は(0, 1, 0)、サイドビューの場合は(0, 0, 1)を設定します。
        /// </summary>
        public Vector3 PlaneAxis
        {
            get => _planeAxis;
            set => _planeAxis = value;
        }

        #endregion


        // ----------------------------------------------------------------------------

        float ITurn.YawAngle => YawRotation.eulerAngles.y;
        int IPriority<ITurn>.Priority => _turnPriority;


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            // ActorSettings
            _characterSettings = gameObject.GetComponentInParent<CharacterSettings>() ?? throw new System.NullReferenceException(nameof(_characterSettings));

            // Components
            _characterSettings.TryGetComponent<ITransform>(out _transform);

            LimitedPosition = transform.position;
            YawRotation = Quaternion.AngleAxis(0, Vector3.up);
        }

        void IUpdateComponent.OnUpdate(float deltaTime)
        {
            var plane = new Plane(_planeAxis, _transform.Position + _planeAxis * _planeOffset);
            var ray = _camera.ScreenPointToRay(_mousePosition);

            // If the cursor position does not make contact with the plane, the process is aborted.
            if (!plane.Raycast(ray, out var distance))
                return;

            // Get cursor position
            var contactPosition = ray.GetPoint(distance) + _originOffset;
            var position = _transform.Position;
            CursorPosition = contactPosition;
            LimitedPosition = (Vector3.Distance(position, contactPosition) > _maxDistance) ? Vector3.MoveTowards(position, contactPosition, _maxDistance) : contactPosition;

            // Pre-calculate character orientation
            var deltaPosition = LimitedPosition - position;
            YawRotation = Quaternion.LookRotation(Vector3.Scale(deltaPosition, new Vector3(1, 0, 1)), Vector3.up);
            PitchRotation = Quaternion.LookRotation(Vector3.Scale(deltaPosition, new Vector3(1, 1, 0)), Vector3.up);
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 基準カメラを設定する．
        /// </summary>
        /// <param name="camera"></param>
        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>
        /// スクリーン座標の方向を向きます。
        /// 注意：これは即座に反映されません。値はUpdateの時点で反映されます。
        /// </summary>
        /// <param name="screenPosition">注視位置</param>
        public void LookTargetPoint(Vector2 screenPosition)
        {
            _mousePosition = screenPosition;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            var position = transform.position;
            var size = new Vector3(1, 0, 1);

            var color = Colors.Green;
            color.a = 0.2f;
            NGizmo.DrawCube(position, size, color);

            if (Application.isPlaying == false)
                return;

            var boxSize = Vector3.one * 0.2f;
            Gizmos.color = Color.yellow;
            NGizmo.DrawCube(CursorPosition, boxSize, Color.yellow);

            NGizmo.DrawCube(LimitedPosition, boxSize, Color.white);
            Gizmos.DrawLine(position, LimitedPosition);

            NGizmo.DrawWireCube(LimitedPosition, Vector3.one * 0.2f);
            Gizmos.color = Color.white;
            Gizmos.DrawCube(LimitedPosition, boxSize);
        }
#endif
    }
}