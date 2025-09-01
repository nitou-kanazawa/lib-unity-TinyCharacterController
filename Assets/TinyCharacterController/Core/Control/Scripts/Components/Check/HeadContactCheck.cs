using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Shared;
using Nitou.TCC.Utils;

namespace Nitou.TCC.Controller.Check
{
    /// <summary>
    ///     This is a component that performs upward object detection.
    ///     It performs upward detection, considering the height set in CharacterSettings.
    ///     It also considers slightly ambiguous detection, not just complete contact with other objects, and calls UnityEvents
    ///     upon collision.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(MenuList.MenuCheck + nameof(HeadContactCheck))]
    public class HeadContactCheck : MonoBehaviour,
                                    IOverheadDetection,
                                    IEarlyUpdateComponent,
                                    IComponentCondition
    {
        /// <summary>
        ///     Offset from the head position.
        ///     This value is used to determine whether objects above are in contact.
        ///     If objects cannot be detected when in contact with the ceiling, set this value to a higher value.
        /// </summary>
        [Title("Settings")] [Tooltip("Offset from the head position")] [Range(0, 0.5f)] [SerializeField, Indent]
        private float _headPositionOffset = 0.1f;

        /// <summary>
        ///     Maximum distance at which upward objects can be detected.
        ///     This is used when checking if there is an object above the head even if there is no direct contact, such as when
        ///     entering a low-ceiling area.
        ///     This value must always be higher than the height of <see cref="CharacterSettings.Height" />.
        /// </summary>
        [FormerlySerializedAs("MaxRange")] [Tooltip("Maximum distance at which upward objects can be detected")] [Range(0, 10f)] [SerializeField, Indent]
        private float MaxHeight = 2.5f;


        /// <summary>
        ///     This UnityEvent calls the callback when the head makes contact during this frame.
        /// </summary>
        [Title("Callbacks")] [SerializeField, Indent]
        private UnityEvent _onContact;

        /// <summary>
        ///     Executes when the value of InRange changes.
        /// </summary>
        [SerializeField, Indent] private UnityEvent _onChangeInRange;

        // references
        private ActorSettings _actorSettings;
        private ITransform _transform;

        // 定数

        /// <summary>
        ///     Number of detectable colliders.
        /// </summary>
        private const int MAX_CONTACT_SIZE = 5;

        private static readonly RaycastHit[] Result = new RaycastHit[MAX_CONTACT_SIZE];


        // ----------------------------------------------------------------------------
        // Property

        /// <inheritdoc/>
        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        ///     Returns true if the head is in contact with other objects during this frame.
        /// </summary>
        public bool IsHitCollisionInThisFrame { get; private set; }

        /// <summary>
        ///     If <see cref="IsHeadContact" /> is true, returns the distance from <see cref="ContactPoint" /> to RootPosition.
        /// </summary>
        public float DistanceFromRootPosition { get; private set; }

        /// <summary>
        ///     Height to the head.
        /// </summary>
        private float Height => _actorSettings.Height + _headPositionOffset;

        /// <summary>
        ///     Returns true if the head is in contact with other objects.
        /// </summary>
        public bool IsHeadContact { get; private set; }

        /// <summary>
        ///     Returns true if there is a collider within the character's head to Max Range range.
        /// </summary>
        public bool IsObjectOverhead { get; private set; }

        /// <summary>
        ///     If true, returns the contact point if <see cref="IsHeadContact" /> is true, and returns the head position if
        ///     false.
        /// </summary>
        /// <seealso cref="MaxHeight" />
        public Vector3 ContactPoint { get; private set; }

        /// <summary>
        ///     Returns the collided gameObject. Returns null if <see cref="IsHeadContact" /> is false.
        ///     It is recommended to check the existence of <see cref="IsHeadContact" /> before using this property.
        /// </summary>
        public GameObject ContactedObject { get; private set; }


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            GatherComponents();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            using var profile = new ProfilerScope(nameof(HeadContactCheck));

            // Cache the current value to detect changes from the previous frame.
            var preInRange = IsObjectOverhead;

            // Prepare parameters needed for Raycast.
            // Cast the ray from the center of the body to avoid contact with the ground.
            var offset = _actorSettings.Height * 0.5f;
            IsObjectOverhead = DetectCollidersAboveHead(offset, out var closestHit);

            if (IsObjectOverhead)
            {
                // Behavior when a collider is detected.

                SetPropertiesWhenInRange(closestHit, offset);

                // Execute the event because an object has hit.
                if (IsHitCollisionInThisFrame)
                    _onContact?.Invoke();
            }
            else
            {
                // No collider detected.

                SetPropertiesWhenOutOfRange();
            }

            // Invoke the event if the presence in range changes.
            if (preInRange != IsObjectOverhead)
                _onChangeInRange?.Invoke();
        }
        
        void IComponentCondition.OnConditionCheck(List<string> messageList)
        {
            if (_actorSettings == null)
                TryGetComponent(out _actorSettings);

            if (_actorSettings.Height > MaxHeight)
                messageList.Add("Max Range should be set to a value greater than or equal to _settings.Height.");
        }


        // ----------------------------------------------------------------------------
        // Private Method
        private void GatherComponents()
        {
            _actorSettings = GetComponentInParent<ActorSettings>() ?? throw new System.NullReferenceException(nameof(_actorSettings));

            // Components
            _actorSettings.TryGetComponent(out _transform);
        }

        /// <summary>
        ///     Sets properties when out of range.
        ///     Called when no colliders are detected above the head.
        /// </summary>
        private void SetPropertiesWhenOutOfRange()
        {
            var distance = MaxHeight + _headPositionOffset;

            // ContactPoint is the position above the head.
            ContactPoint = new Vector3(0, distance, 0);

            // Distance is the position above the head.
            DistanceFromRootPosition = distance;
            IsHeadContact = false;
            ContactedObject = null;
            IsHitCollisionInThisFrame = false;
        }

        /// <summary>
        ///     Sets properties when in range.
        ///     Called when colliders are detected above the head.
        /// </summary>
        /// <param name="closestHit">The closest RaycastHit</param>
        /// <param name="offset">Offset at the start of Raycast</param>
        private void SetPropertiesWhenInRange(in RaycastHit closestHit, float offset)
        {
            // Cache the current value to detect differences from the previous frame.
            var preContactHead = IsHeadContact;

            // Distance is the sum of RaycastHit's distance, the offset at the start of the Cast, and the SphereCast's offset.
            DistanceFromRootPosition = closestHit.distance + offset + _actorSettings.Radius + _headPositionOffset;
            ContactPoint = closestHit.point;
            ContactedObject = closestHit.collider.gameObject;

            // If the distance from the ground is lower than the height setting, consider it a collision.
            // Also, if the collision detection is different from the previous frame, consider it a hit in the current frame.
            IsHeadContact = DistanceFromRootPosition < _actorSettings.Height + _headPositionOffset;
            IsHitCollisionInThisFrame = !preContactHead && IsHeadContact;
        }

        /// <summary>
        ///     Detects colliders above the head and identifies the closest collider.
        ///     Excludes the collider owned by itself.
        /// </summary>
        /// <param name="offset">Offset for detection</param>
        /// <param name="closestHit">RayCast of the closest Collider</param>
        /// <returns>True if a detectable Collider is within range</returns>
        private bool DetectCollidersAboveHead(float offset, out RaycastHit closestHit)
        {
            var ray = new Ray(_transform.Position + new Vector3(0, offset + _headPositionOffset, 0), Vector3.up);
            var rayDistance = MaxHeight + _headPositionOffset - offset - _actorSettings.Radius;

            // Perform a SphereCast upward to check for the presence of colliders above the head.
            var count = Physics.SphereCastNonAlloc(ray, _actorSettings.Radius, Result,
                rayDistance, _actorSettings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // Get the closest Raycast excluding the Collider owned by itself.
            var isHit = _actorSettings.ClosestHit(Result, count, rayDistance, out closestHit);
            return isHit;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        // TODO: Gizmosの修正
        private void OnDrawGizmosSelected()
        {
            if (_actorSettings == null)
            {
                GatherComponents();
            }

            // If a collider is considered in contact, set the base color to red.
            if (IsHeadContact)
                Gizmos.color = Color.red;

            var position = transform.position;
            var headPosition = position + new Vector3(0, Height - _actorSettings.Radius, 0);

            if (Application.isPlaying)
            {
                // Represent the position where collision is detected based on DistanceFromRootPosition.
                var offset = _actorSettings.Radius;
                var contactPosition = position + new Vector3(0, DistanceFromRootPosition - offset, 0);
                DrawHitRangeGizmos(headPosition, contactPosition);

                // if (IsHeadContact)
                //     Gizmos_.DrawCollider(ContactedObject.GetComponent<Collider>(), Color.yellow);
            }
            else
            {
                // Represent the position of the head based on MaxHeight.
                var maxHeightPosition = position + new Vector3(0, MaxHeight, 0);
                DrawHitRangeGizmos(headPosition, maxHeightPosition);
            }

            return;

            // Represent capsule-shaped Gizmos.
            void DrawHitRangeGizmos(in Vector3 startPosition, in Vector3 endPosition)
            {
                var leftOffset = new Vector3(_actorSettings.Radius, 0, 0);
                var rightOffset = new Vector3(-_actorSettings.Radius, 0, 0);
                var forwardOffset = new Vector3(0, 0, _actorSettings.Radius);
                var backOffset = new Vector3(0, 0, -_actorSettings.Radius);

                // Represent vertical lines before and after the capsule.
                Gizmos.DrawLine(startPosition + leftOffset, endPosition + leftOffset);
                Gizmos.DrawLine(startPosition + rightOffset, endPosition + rightOffset);
                Gizmos.DrawLine(startPosition + forwardOffset, endPosition + forwardOffset);
                Gizmos.DrawLine(startPosition + backOffset, endPosition + backOffset);

                // Represent circular shapes above and below the capsule.
                Gizmos.DrawWireSphere(startPosition, _actorSettings.Radius);
                Gizmos.DrawWireSphere(endPosition, _actorSettings.Radius);

                // Fill the color of the circular shapes above and below the capsule.
                // var color = Colors.Yellow.WithAlpha(.4f);
                // Gizmos_.DrawSphere(startPosition, _actorSettings.Radius, color);
                // Gizmos_.DrawSphere(endPosition, _actorSettings.Radius, color);
            }
        }
#endif
    }
}