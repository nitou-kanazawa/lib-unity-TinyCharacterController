using System;
using System.Buffers;
using UniRx;
using UnityEngine;
using Nitou.BatchProcessor;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using Sirenix.OdinInspector;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Effect
{
    /// <summary>
    /// アクターに外部からの衝撃を適用するコンポーネント．
    /// 空気抵抗や地面との摩擦により減速する．
    /// </summary>
    [AddComponentMenu(MenuList.MenuEffect + nameof(ExtraForce))]
    [DisallowMultipleComponent]
    public sealed class ExtraForce : ComponentBase,
                                     IEffect,
                                     IEarlyUpdateComponent
    {
        /// <summary>
        /// 摩擦．
        /// </summary>
        [SerializeField, Indent][MinValue(0)] private float _friction = 1f;

        /// <summary>
        /// 空気抵抗．
        /// </summary>
        [SerializeField, Indent][MinValue(0)] private float _drag = 0.1f;

        /// <summary>
        /// 加速度を停止するための閾値．
        /// </summary>
        [SerializeField, Indent][MinValue(0.1f)] private float _threshold = 0.5f;

        /// <summary>
        /// 反発係数．1で完全反射、0で衝突時に停止．
        /// </summary>
        [PropertyRange(0, 1)] [SerializeField, Indent]
        private float _bounce = 0f;

        // References
        private CharacterSettings _settings;
        private IGroundContact _groundCheck;
        private ITransform _transform;

        // State
        private Vector3 _velocity;

        // Event stream
        private readonly Subject<Collider> _onHitOtherCollider = new();

        // Constants
        private const int HIT_CAPACITY = 15;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 更新タイミング．
        /// </summary>
        int IEarlyUpdateComponent.Order => Order.Effect;

        /// <summary>
        /// 反発係数．
        /// </summary>
        public float Bounce
        {
            get => _bounce;
            set => _bounce = value;
        }

        /// <summary>
        /// 速度．
        /// </summary>
        public Vector3 Velocity => _velocity;

        /// <summary>
        /// 他コライダーと接触したときに通知するObservable．
        /// </summary>
        public IObservable<Collider> OnHitOtherCollider => _onHitOtherCollider;


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            GatherComponents();
        }

        private void OnDestroy()
        {
            _onHitOtherCollider.Dispose();
        }

        void IEarlyUpdateComponent.OnUpdate(float dt)
        {
            // If there is a vector affecting the character, perform deceleration and bounce processing.
            if (_velocity.magnitude > _threshold)
            {
                // If there is a collider at the destination, reflect the vector.
                if (HasColliderOnDestination(dt, out var closestHit))
                {
                    // Event to be called when colliding with other objects.
                    // Process before correcting Velocity.
                    _onHitOtherCollider.OnNext(closestHit.collider);

                    if (_bounce > 0)
                    {
                        // When colliding with other ExtraForce, propagate the impact.
                        if (closestHit.collider.TryGetComponent(out ExtraForce other) &&
                            closestHit.collider.TryGetComponent(out CharacterSettings otherSettings))
                        {
                            // Force = Mass * Acceleration
                            var ownForce = _settings.Mass * _velocity;
                            var otherForce = otherSettings.Mass * other._velocity;
                            var velocity = (ownForce + otherForce) / (_settings.Mass + otherSettings.Mass);

                            // Add acceleration to self and the target of the collision.
                            other.AddForce(velocity * other._bounce);
                            _velocity = Vector3.Reflect(velocity, closestHit.normal) * _bounce;
                        }
                        else
                        {
                            _velocity = Vector3.Reflect(_velocity, closestHit.normal) * _bounce;
                        }
                    }
                }

                // Decelerate the character's vector. The deceleration rate switches between ground and air.
                var value = _groundCheck.IsOnGround ? _friction : _drag;
                _velocity -= _velocity * (dt * value);
            }
            else
            {
                // If the acceleration falls below the threshold, disable the vector.
                _velocity = Vector3.zero;
            }
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// Add impact to the character.
        /// </summary>
        /// <param name="value">Power</param>
        public void AddForce(Vector3 value)
        {
            _velocity += value / _settings.Mass;
        }

        /// <summary>
        /// Override velocity.
        /// </summary>
        /// <param name="value">new value.</param>
        public void SetVelocity(Vector3 value)
        {
            _velocity = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetVelocity()
        {
            _velocity = Vector3.zero;
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        ///     Gather all components attached own object.
        /// </summary>
        private void GatherComponents()
        {
            _settings = GetComponentInParent<CharacterSettings>();

            _settings.TryGetComponent(out _transform);
            _settings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
        }

        /// <summary>
        ///     Method to retrieve the shape of a capsule.
        /// </summary>
        /// <param name="headPoint">Coordinates of the top of the capsule.</param>
        /// <param name="bottomPoint">Coordinates of the bottom of the capsule.</param>
        private void GetBottomHeadPosition(out Vector3 headPoint, out Vector3 bottomPoint)
        {
            // Get the current position of the capsule.
            var point = _transform.Position;

            // Get the height of the capsule.
            var height = _settings.Height;

            // Get the radius of the capsule.
            var radius = _settings.Radius;

            // Calculate the coordinates of the bottom of the capsule.
            bottomPoint = point + new Vector3(0, radius, 0);

            // Calculate the coordinates of the top of the capsule.
            headPoint = point + new Vector3(0, height - radius, 0);
        }

        /// <summary>
        ///     Determines if there is an object at the destination of movement.
        /// </summary>
        /// <param name="dt">Delta time since the previous frame.</param>
        /// <param name="closestHit">Information about the closest collider. It will be set to default if no objects are present.</param>
        /// <returns>True if an object is present.</returns>
        private bool HasColliderOnDestination(float dt, out RaycastHit closestHit)
        {
            // Get the positions of the character's head and bottom.
            GetBottomHeadPosition(out var bottom, out var top);

            // Calculate the distance: Character's radius + 1 frame of movement.
            var distance = _settings.Radius * 0.5f + _velocity.magnitude * dt;

            // Create an array for collision detection.
            var hits = ArrayPool<RaycastHit>.Shared.Rent(HIT_CAPACITY);

            // Perform collision detection with the character's shape.
            var hitCount = Physics.CapsuleCastNonAlloc(top, bottom,
                _settings.Radius, _velocity.normalized, hits, distance,
                _settings.EnvironmentLayer, QueryTriggerInteraction.Ignore);

            // Find the closest collider among the colliders within the range, excluding the collider to which self belongs.
            // isCapsuleHit indicates whether the hit was successful.
            var isCapsuleHit = _settings.ClosestHit(hits, hitCount, distance, out var hit);

            ArrayPool<RaycastHit>.Shared.Return(hits);


            if (isCapsuleHit)
            {
                // Get the normal of the contacting surface.
                // Without this step, there is a possibility of unexpected flipping during contact with the ground.
                var normal = _velocity.normalized;
                var ray = new Ray(hit.point - normal * 0.1f, normal);

                // Perform a raycast to find the closest hit point on the collider.
                var result = hit.collider.Raycast(ray, out closestHit, 1);

                return result;
            }

            // If there is no collision, set closestHit to the default value and return False.
            closestHit = default;
            return false;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // Calculate the center position of the character.
            var centerPosition = _transform.Position + new Vector3(0, _settings.Height * 0.5f, 0);

            // Calculate the target position for movement.
            var maxDistance = _velocity.magnitude * 0.28f;
            var targetPosition = centerPosition + _velocity.normalized * maxDistance;

            // Represent the movement vector.
            var sphereColor = Color.blue;
            sphereColor.a = 0.4f;
            NGizmo.DrawSphere(targetPosition, _settings.Radius, sphereColor);

            // Represent the line to the target position and the wireframe of the movement vector.
            var color = Color.white;
            NGizmo.DrawLine(targetPosition, centerPosition, color);
            NGizmo.DrawWireSphere(targetPosition, _settings.Radius, color);
        }
#endif
    }
}