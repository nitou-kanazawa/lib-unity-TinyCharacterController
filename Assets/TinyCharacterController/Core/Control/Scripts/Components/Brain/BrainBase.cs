using UnityEngine;
using Nitou.BachProcessor;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Shared;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// アクターの移動処理を統括するBrain基底クラス．
    /// </summary>
    public abstract class BrainBase : MonoBehaviour,
                                      IWarp, IBrain,
                                      ITransform,
                                      IEarlyUpdateComponent
    {
        // Manager
        private readonly MoveManager _moveManager = new();
        private readonly TurnManager _turnManager = new();
        private readonly WarpManager _warpManager = new();
        private readonly EffectManager _effectManager = new();
        private readonly UpdateComponentManager _updateComponentManager = new();
        private readonly CameraManager _cameraManager = new();
        private readonly CollisionManager _collisionManager = new();

        // 
        protected Quaternion Rotation;
        protected Vector3 Position;


        protected ActorSettings Settings;
        protected Transform CachedTransform;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 現在の移動速度．
        /// <see cref="IMove"/> が存在しない場合、値は0となる．
        /// </summary>
        public float CurrentSpeed => _moveManager.CurrentSpeed;

        /// <summary>
        /// 現在の回転速度．
        /// <see cref="ITurn"/> が存在しない場合、値は0となる．
        /// </summary>
        public int TurnSpeed => _turnManager.CurrentTurn?.TurnSpeed ?? 0;

        /// <summary>
        /// Character orientation in world space.
        /// </summary>
        public float YawAngle => _turnManager.TargetYawAngle;

        /// <summary>
        /// Local vector for the direction the character is facing
        /// </summary>
        public Vector3 LocalVelocity => Quaternion.Inverse(CachedTransform.rotation) * ControlVelocity;

        /// <summary>
        /// Movement vector of the character in world space.
        /// </summary>
        public Vector3 ControlVelocity => _moveManager.Velocity;

        /// <summary>
        /// Additional movement vectors that are added.
        /// For example, gravity or impact.
        /// </summary>
        public Vector3 EffectVelocity => _effectManager.Velocity;

        /// <summary>
        /// The vector that is the sum of the character's movement vector and the additive movement vector.
        /// </summary>
        public Vector3 TotalVelocity { get; private set; }

        /// <summary>
        /// Difference between the current direction and the target direction
        /// </summary>
        public float DeltaTurnAngle => _turnManager.DeltaTurnAngle;

        /// <summary>
        /// 更新タイミング．
        /// </summary>
        public abstract UpdateTiming Timing { get; }

        IMove IBrain.CurrentMove => _moveManager.CurrentMove;
        ITurn IBrain.CurrentTurn => _turnManager.CurrentTurn;

        Vector3 ITransform.Position
        {
            get => Position;
            set
            {
                SetPositionDirectly(value);
                Position = value;
            }
        }

        Quaternion ITransform.Rotation
        {
            get => Rotation;
            set
            {
                SetRotationDirectly(value);
                Rotation = value;
            }
        }

        int IEarlyUpdateComponent.Order => Order.PrepareEarlyUpdate;


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected void Initialize()
        {
            var go = gameObject;
            _cameraManager.Initialize(go);
            _updateComponentManager.Initialize(go);
            _moveManager.Initialize(go);
            _turnManager.Initialize(go, this);
            _effectManager.Initialize(go);
            _collisionManager.Initialize(go);

            // gather all components.
            TryGetComponent(out CachedTransform);
            TryGetComponent(out Settings);
        }

        /// <summary>
        /// Update information in Brain.<br/>
        /// </summary>
        protected void UpdateBrain()
        {
            //if (!Settings.HasCamera) {
            //    Debug.LogWarning("Camera not found", gameObject);
            //    return;
            //}

            // If executed at the timing of FixedUpdate, deltaTime returns the value of FixedUpdate.
            var deltaTime = Time.deltaTime;

            // Update coordinates
            _updateComponentManager.Process(deltaTime);

            // update highestPriority
            _moveManager.UpdateHighestMoveControl(deltaTime);
            _turnManager.UpdateHighestTurnControl(deltaTime);

            // update velocity and angle.
            _effectManager.CalculateVelocity();
            _moveManager.CalculateVelocity();
            _turnManager.CalculateAngle(deltaTime);

            TotalVelocity = _moveManager.Velocity + _effectManager.Velocity;

            // Update the position.
            if (_warpManager.WarpedPosition)
            {
                if (_warpManager.IsMove)
                    MovePosition(_warpManager.Position);
                else
                    SetPositionDirectly(_warpManager.Position);

                _effectManager.ResetVelocity();
            }
            else
            {
                ApplyPosition(TotalVelocity, deltaTime);
            }

            // Update the direction.
            if (_warpManager.WarpedRotation)
            {
                SetRotationDirectly(_warpManager.Rotation);
            }
            else if (_turnManager.HasHighestPriority)
            {
                ApplyRotation(Quaternion.AngleAxis(_turnManager.NextYawAngle, Vector3.up));
            }

            // Update the camera's position and direction.
            // To prevent jitter, process the camera after updating the character's position.
            _cameraManager.Process(deltaTime);

            _warpManager.ResetWarp();
        }

        /// <summary>
        /// 更新処理．
        /// </summary>
        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            GetPositionAndRotation(out Position, out Rotation);
        }


        // ----------------------------------------------------------------------------
        // 

        /// <summary>
        /// 位置と回転のワープ移動．
        /// </summary>
        /// <param name="position">New position.</param>
        /// <param name="direction">New rotation. If Vector3.zero, maintain current orientation.</param>
        public void Warp(Vector3 position, Vector3 direction)
        {
            // If Direction is vector3.zero, maintain current orientation
            var rotation = direction != Vector3.zero
                ? Quaternion.LookRotation(direction)
                : CachedTransform.rotation;
            _warpManager.SetPositionAndRotation(position, rotation);
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 位置と回転のワープ移動．
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="rotation">new rotation.</param>
        public void Warp(Vector3 position, Quaternion rotation)
        {
            _warpManager.SetPositionAndRotation(position, rotation);
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 位置のみのワープ移動．
        /// ワープによる移動が<see cref="IMove"/>より優先される．
        /// </summary>
        /// <param name="position">New position.</param>
        public void Warp(Vector3 position)
        {
            _warpManager.SetPosition(position);
            Position = position;
        }

        /// <summary>
        /// 回転のみのワープ移動．
        /// ワープによる移動が<see cref="IMove"/>より優先される．
        /// </summary>
        /// <param name="rotation">new rotation.</param>
        public void Warp(Quaternion rotation)
        {
            _warpManager.SetRotation(rotation);
            Rotation = rotation;
        }

        /// <summary>
        /// Warp to take into account the movement of the midpoint.
        /// Warp behavior depends on Brain.
        /// </summary>
        /// <param name="position">New coordinates</param>
        public void Move(Vector3 position)
        {
            _warpManager.Move(position);
            Position = position;
        }


        // ----------------------------------------------------------------------------
        // Abstract Method

        /// <summary>
        /// 最終的な位置を適用する．
        /// </summary>
        /// <param name="totalVelocity">Current acceleration</param>
        /// <param name="deltaTime">Delta time</param>
        protected abstract void ApplyPosition(in Vector3 totalVelocity, float deltaTime);

        /// <summary>
        /// 最終的な回転を適用する．
        /// </summary>
        protected abstract void ApplyRotation(in Quaternion rotation);

        /// <summary>
        /// Move the position of the character to the <see cref="newPosition"/>.
        /// Unlike <see cref="Warp(UnityEngine.Vector3)"/>, it is affected by other vectors.
        /// </summary>
        /// <param name="newPosition">new position</param>
        /// <returns>move success</returns>
        protected abstract void SetPositionDirectly(in Vector3 newPosition);

        /// <summary>
        /// Turn the character to the <see cref="newRotation"/>.
        /// Unlink <see cref="Warp(UnityEngine.Quaternion)"/>, it is affected by other turn.
        /// </summary>
        /// <param name="newRotation">new rotation.</param>
        protected abstract void SetRotationDirectly(in Quaternion newRotation);

        /// <summary>
        /// Move the character to the <see cref="newPosition"/>.
        /// Unlink <see cref="Warp(UnityEngine.Vector3)"/>, it is affected by other move.
        /// </summary>
        /// <param name="newPosition">new position</param>
        protected abstract void MovePosition(in Vector3 newPosition);

        /// <summary>
        /// Cache Position and Rotation for each inherited Brain.
        /// </summary>
        protected abstract void GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
    }
}