using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Nitou.TCC.Utils;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Check
{
    /// <summary>
    /// This is a component that performs sight detection.
    /// It detects targets within a specified range from the viewpoint, considering obstacles.
    /// It calls <see cref="InsightTargets"/> when some targets are within the sight or when all objects have exited the sight.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SightCheck : MonoBehaviour, IEarlyUpdateComponent
    {
        /// <summary>
        /// The position of the head to be used for detection.
        /// </summary>
        [Title("Sight Settings")] [SerializeField, Indent]
        public Transform _headTransform;

        /// <summary>
        /// The range of the sight.
        /// </summary>
        [SerializeField, Indent] public int _range = 10;

        /// <summary>
        /// The angle of the sight.
        /// </summary>
        [SerializeField, Indent] public int _angle = 30;

        /// <summary>
        /// The layer to use for detection. Objects in this layer will be visible.
        /// </summary>
        public LayerMask VisibleLayerMask;

        /// <summary>
        /// The tags to use for detection. Objects with these tags will be visible.
        /// </summary>
        [SerializeField, Indent] public string[] _targetTagList;

        /// <summary>
        /// If true, check for the presence of obstacles.
        /// Obstacle detection uses <see cref="CharacterSettings._environmentLayer"/>.
        /// </summary>
        [Title("Options")] public bool RaycastCheck = true;

        /// <summary>
        /// Calls an event when an object enters or exits the sight.
        /// </summary>
        public UnityEvent<bool> OnChangeInsightAnyTargetState;
        
        private CharacterSettings _settings;
        

        // ----------------------------------------------------------------------------
        // Property

        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        /// Gets a list of objects within the sight.
        /// </summary>
        public List<GameObject> InsightTargets { get; private set; } = new();

        /// <summary>
        /// Gets the first object found within the sight.
        /// </summary>
        public GameObject InsightTarget => InsightTargets.Count > 0 ? InsightTargets[0] : null;

        /// <summary>
        /// If true, there are objects within the sight.
        /// </summary>
        public bool IsInsightAnyTarget => InsightTargets.Count > 0;
   
        /// <summary>
        /// The maximum number of objects that can be detected at once.
        /// </summary>
        private const int CAPACITY = 100;

        private static readonly Collider[] Results = new Collider[CAPACITY];


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            // Collect a list of related components.
            GatherComponents();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            // Cache the previous information to detect changes in the sight.
            var isAnyInsightTargetPreviousFrame = IsInsightAnyTarget;

            // Get the coordinates and direction of the sensor's position.
            var headPosition = _headTransform.position;
            var forward = _headTransform.forward;

            // Collect all colliders around the character.
            var count = Physics.OverlapSphereNonAlloc(headPosition, _range, Results,
                VisibleLayerMask, QueryTriggerInteraction.Ignore);

            // Extract targets from the list of colliders.
            InsightTargets.Clear();
            for (var i = 0; i < count; i++)
            {
                var col = Results[i];

                // Skip processing if the detected object is one's own collider or not in the tag list.
                if (_settings.IsOwnCollider(col) ||
                    col.gameObject.ContainTag(_targetTagList) == false)
                    continue;

                // Detect the position of the closest edge within the sight.
                var closestPoint = col.ClosestPointOnBounds(headPosition);
                var deltaPosition = closestPoint - headPosition;

                // Skip processing if the target is outside the sight.
                if (Vector3.Angle(forward, deltaPosition) > _angle * 0.5f)
                    continue;

                // Skip processing if RaycastCheck is enabled and the target is obstructed by an obstacle.
                if (RaycastCheck &&
                    IsCollideTarget(headPosition, closestPoint, col))
                    continue;

                // Add the object to the list of objects within the sight.
                if (InsightTargets.Contains(col.gameObject) == false)
                    InsightTargets.Add(col.gameObject);
            }

            // Notify if the sight state has changed.
            if (IsInsightAnyTarget != isAnyInsightTargetPreviousFrame)
                OnChangeInsightAnyTargetState.Invoke(IsInsightAnyTarget);
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// Checks for obstacles between the target objects.
        /// Excludes the collider of the sensor and the target collider.
        /// </summary>
        /// <param name="position">The position of the sensor.</param>
        /// <param name="targetPosition">The closest position of the target.</param>
        /// <param name="targetCollider">The target object's collider.</param>
        /// <returns>True if obstructed by an obstacle.</returns>
        private bool IsCollideTarget(in Vector3 position, in Vector3 targetPosition, in Collider targetCollider)
        {
            var deltaPosition = (targetPosition - position);
            var direction = deltaPosition.normalized;
            var distance = deltaPosition.magnitude;

            // Allocate a buffer.
            var hits = new RaycastHit[CAPACITY];

            // Check if the sight is clear towards the target from the sensor.
            var count = Physics.RaycastNonAlloc(position, direction, hits, distance, _settings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // Skip processing if the detected collider is the target's collider or belongs to oneself.
            var isCollide = false;
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                // Skip processing if the collider belongs to the target or oneself.
                if (targetCollider == hit.collider || _settings.IsOwnCollider(hit.collider))
                    continue;

                // Interrupt the search process because it is obstructed by an obstacle.
                isCollide = true;
                break;
            }

            // Return false if obstructed.
            return isCollide;
        }

        /// <summary>
        /// Collect a list of components.
        /// </summary>
        private void GatherComponents()
        {
            _settings = gameObject.GetComponentInParent<CharacterSettings>();
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure that the maximum and minimum values are within valid ranges.
            _range = Mathf.Max(0, _range);
            _angle = Mathf.Clamp(_angle, 0, 360);
        }

        private void Reset()
        {
            // Set the default value of the layer to include in the sight.
            VisibleLayerMask = LayerMask.GetMask("Default");
        }

        // TODO: Gizmosの修正
        /*
        private void OnDrawGizmosSelected() {
            // Do nothing if the game is not playing.
            if (Application.isPlaying == false)
                return;

            // Represent objects within the sight using Gizmos.
            foreach (var obj in InsightTargets) {
                var position = obj.transform.position;
                Gizmos_.DrawSphere(position, 1f, Colors.Yellow);
            }
        }
        */
#endif
    }
}