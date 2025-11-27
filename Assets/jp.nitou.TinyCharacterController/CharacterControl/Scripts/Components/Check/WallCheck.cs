using System;
using Nitou.Gizmo;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Check
{
    /// <summary>
    /// A component that performs collision detection with walls is implemented.
    /// It detects walls in the direction of the character's movement.
    /// When a collision with a wall occurs, callbacks are triggered during the collision,
    /// while in contact with the wall, and when the character moves away from the wall.
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(WallCheck))]
    [DisallowMultipleComponent]
    public sealed class WallCheck : MonoBehaviour,
                                    IEarlyUpdateComponent, IWallCheck
    {
        [Title("Settings")]
        [MinMaxSlider(15, 165)]
        [SerializeField, Indent] private Vector2 _wallAngleRange = new(75, 115);

        [Range(0.01f, 1f)]
        [SerializeField, Indent] private float _wallDetectionDistance = 0.1f;

        private int _order;
        private IBrain _brain;
        private ITransform _transform;
        private CharacterSettings _settings;

        private Vector3 _normal;
        private Vector3 _hitPoint;
        private Collider _contactObject;
        private static readonly RaycastHit[] Hits = new RaycastHit[5];

        /// <summary>
        /// If there is contact, it returns True.
        /// </summary>
        public bool IsContact { get; private set; }

        /// <summary>
        /// Returns normal vector of the contact surface. If there is no contact, it returns Vector3.Zero.
        /// </summary>
        public Vector3 Normal => _normal;

        public GameObject ContactObject => _contactObject.gameObject;

        public Vector3 HitPoint => _hitPoint;

        int IEarlyUpdateComponent.Order => Order.Check;

        // callbacks

        private Subject<Unit> _onWallContactedSubject = new();
        private Subject<Unit> _onWallLeftSubject = new();
        private Subject<Unit> _onWallStuckSubject = new();

        /// <summary>
        /// Invoke when touch with a wall.
        /// </summary>
        public Observable<Unit> OnWallContacted => _onWallContactedSubject;

        /// <summary>
        /// Invoke when leave from a wall.
        /// </summary>
        public Observable<Unit> OnWallLeft => _onWallLeftSubject;

        /// <summary>
        /// Invoke when contact with a wall.
        /// </summary>
        public Observable<Unit> OnWallStuck => _onWallStuckSubject;


        #region Lifecycle Events

        private void Awake()
        {
            TryGetComponent(out _brain);
            TryGetComponent(out _transform);
            TryGetComponent(out _settings);
        }

        private void OnDestroy()
        {
            _onWallContactedSubject.Dispose();
            _onWallLeftSubject.Dispose();
            _onWallStuckSubject.Dispose();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            // If the component is invalid, do not update the process.
            if (enabled == false)
                return;

            var preContact = IsContact;
            var direction = _brain.ControlVelocity.normalized;

            IsContact = HitCheck(direction, out _normal, out _hitPoint, out _contactObject);

            if (IsContact && !preContact)
                _onWallContactedSubject.OnNext(Unit.Default);

            if (IsContact)
                _onWallStuckSubject.OnNext(Unit.Default);

            if (!IsContact && preContact)
                _onWallLeftSubject.OnNext(Unit.Default);
        }

        #endregion

        /// <summary>
        /// Immediately performs wall determination.
        /// The results of this calculation are processed independently of the component calculations. This means that the calculation results are not saved.
        /// The calculation ignores colliders in the same component.
        /// </summary>
        /// <param name="direction">direction</param>
        /// <param name="normal">result normal</param>
        /// <param name="point">result hit point</param>
        /// <param name="contactCollider">return contact object.  if no contact return null.</param>
        /// <returns>is contact any collider</returns>
        public bool HitCheck(Vector3 direction, out Vector3 normal, out Vector3 point, out Collider contactCollider)
        {
            var distance = _settings.Radius + _wallDetectionDistance;
            var halfHeight = _settings.Height * 0.5f;
            var centerPosition = _transform.Position + Vector3.up * halfHeight;
            var ray = new Ray(centerPosition, direction);
            var count = Physics.SphereCastNonAlloc(ray, _settings.Radius, Hits, distance, _settings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // find most closest target.
            var hasClosestHit = _settings.ClosestHit(Hits, count, distance, out var hit);
            if (hasClosestHit)
            {
                // apply limit angle.
                var angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle > _wallAngleRange.x && angle < _wallAngleRange.y &&
                    Vector3.Distance(hit.point, centerPosition) < distance)
                {
                    normal = hit.normal;
                    point = hit.point;
                    contactCollider = hit.collider;
                    return true;
                }
            }

            normal = Vector3.zero;
            point = Vector3.zero;
            contactCollider = null;
            return false;
        }


        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false)
                return;

            if (IsContact)
                NGizmo.DrawCollider(in _contactObject, Color.yellow);

            var distance = _settings.Radius + 0.1f;
            var halfHeight = _settings.Height * 0.5f;
            var centerPosition = _transform.Position + Vector3.up * halfHeight;
            var direction = _brain.ControlVelocity.normalized;
            var position = centerPosition + direction * distance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, _settings.Radius);

            Gizmos.color = IsContact ? Color.red : Color.white;
            Gizmos.DrawWireSphere(position, _settings.Radius);
        }
    }
}