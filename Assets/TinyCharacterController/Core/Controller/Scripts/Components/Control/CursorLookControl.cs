using UnityEngine;
using Sirenix.OdinInspector;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Shared;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Control
{
    /// <summary>
    /// This component updates the character's orientation based on the cursor position. <br/>
    /// 
    /// If the component has high priority, the character will look in the direction of the cursor.
    /// The coordinates at which the character gazes are calculated based on <see cref="LookTargetPoint"/>.
    /// If you want to use side view instead of top-down, change <see cref="_planeAxis"/>.
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(CursorLookControl))]
    [DisallowMultipleComponent]
    public sealed class CursorLookControl : MonoBehaviour, ITurn, IUpdateComponent
    {
        [Required]
        [SerializeField, Indent]private  Camera _camera;

        [Title("Cursor behavior settings")]
        /// <summary>
        /// �J�[�\����F������ő勗���D
        /// Used to limit the range of movement of the camera.
        /// For example, if you want the camera to follow the cursor position.
        /// </summary>
        [Tooltip("Maximum distance of cursor")]
        [SerializeField, Indent]
        private float _maxDistance = 3;

        /// <summary>
        /// Offset to compensate for orientation to the cursor.
        /// For (0,0,0), orientation from the root of the model.
        /// Adjust to gun height if you want to calculate orientation based on gun height.
        /// </summary>
        [SerializeField, Indent]private  Vector3 _originOffset = Vector3.zero;

        [Title("Plane settings")] [SerializeField, Indent]
        private Vector3 _planeAxis = Vector3.up;

        [SerializeField, Indent] private float _planeOffset;

        [Title("Character orientation control")]
        /// <summary>
        /// Rotation Priority. Face the cursor direction when it is higher than the priority of other components.
        /// </summary>
        [SerializeField, Indent]
        private int _turnPriority = 1;

        /// <summary>
        /// Speed at which the cursor is oriented. If the value is -1, the orientation is fixed.
        /// </summary>
        [PropertyRange(-1, 100)] [SerializeField, Indent]
        private int _turnSpeed = 30;


        private Vector2 _mousePosition;
        private ITransform _transform;
        private ActorSettings _actorSettings;


        // ----------------------------------------------------------------------------

        #region Property

        /// <summary>
        /// ���������D
        /// </summary>
        int IUpdateComponent.Order => Order.Control;

        /// <summary>
        /// Turn Speed
        /// </summary>
        public int TurnSpeed
        {
            get => _turnSpeed;
            set => _turnSpeed = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public int TurnPriority
        {
            get => _turnPriority;
            set => _turnPriority = value;
        }

        /// <summary>
        /// CursorPosition centered on the character and limited to MaxDistance distance.
        /// Avoid the problem of the cursor leaving indefinitely in cases where the camera is tracking the cursor.
        /// </summary>
        public Vector3 LimitedPosition { get; private set; }

        /// <summary>
        /// �J�[�\���̃��[���h���W�D
        /// </summary>
        public Vector3 CursorPosition { get; private set; }

        /// <summary>
        /// �J�[�\���̉�]�p�x�D(��������)
        /// </summary>
        public Quaternion YawRotation { get; private set; }

        /// <summary>
        ///  �J�[�\���̉�]�p�x�D(��������)
        /// </summary>
        public Quaternion PitchRotation { get; private set; }

        /// <summary>
        /// Direction of the judgment plane.
        /// Set (0, 1, 0) for a top viewpoint or (0, 0, 1) for a side viewpoint.
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
            _actorSettings = gameObject.GetComponentInParent<ActorSettings>() ?? throw new System.NullReferenceException(nameof(_actorSettings));

            // Components
            _actorSettings.TryGetComponent<ITransform>(out _transform);

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
        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        /// <summary>
        /// Face the direction of the screen coordinates
        /// Note that this is not immediately reflected. The value is reflected at the time of Update.
        /// </summary>
        /// <param name="screenPosition">look position</param>
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