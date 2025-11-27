using UnityEngine;
using Nitou.BatchProcessor;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;

namespace Nitou.TCC.CharacterControl.Core
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

        protected CharacterSettings Settings;
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
        /// ワールド空間におけるキャラクターの向き（Yaw角度）．
        /// </summary>
        public float YawAngle => _turnManager.TargetYawAngle;

        /// <summary>
        /// キャラクターが向いている方向のローカルベクトル．
        /// </summary>
        public Vector3 LocalVelocity => Quaternion.Inverse(CachedTransform.rotation) * ControlVelocity;

        /// <summary>
        /// ワールド空間におけるキャラクターの移動ベクトル．
        /// </summary>
        public Vector3 ControlVelocity => _moveManager.Velocity;

        /// <summary>
        /// 追加される移動ベクトル．
        /// 例えば、重力や衝撃など．
        /// </summary>
        public Vector3 EffectVelocity => _effectManager.Velocity;

        /// <summary>
        /// キャラクターの移動ベクトルと追加移動ベクトルの合計．
        /// </summary>
        public Vector3 TotalVelocity { get; private set; }

        /// <summary>
        /// 現在の向きと目標の向きとの差分角度．
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
        /// Brain の情報を更新する．
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
        /// <param name="position">新しい位置．</param>
        /// <param name="direction">新しい向き．Vector3.zero の場合、現在の向きを維持する．</param>
        public void Warp(Vector3 position, Vector3 direction)
        {
            // direction が vector3.zero の場合、現在の向きを維持する
            // 浮動小数点精度問題を回避するため、sqrMagnitudeで判定
            var rotation = direction.sqrMagnitude > float.Epsilon
                ? Quaternion.LookRotation(direction)
                : CachedTransform.rotation;
            _warpManager.SetPositionAndRotation(position, rotation);
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 位置と回転のワープ移動．
        /// </summary>
        /// <param name="position">新しい位置．</param>
        /// <param name="rotation">新しい回転．</param>
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
        /// <param name="position">新しい位置．</param>
        public void Warp(Vector3 position)
        {
            _warpManager.SetPosition(position);
            Position = position;
        }

        /// <summary>
        /// 回転のみのワープ移動．
        /// ワープによる移動が<see cref="IMove"/>より優先される．
        /// </summary>
        /// <param name="rotation">新しい回転．</param>
        public void Warp(Quaternion rotation)
        {
            _warpManager.SetRotation(rotation);
            Rotation = rotation;
        }

        /// <summary>
        /// 中間点の移動を考慮したワープ移動．
        /// ワープの挙動はBrainに依存する．
        /// </summary>
        /// <param name="position">新しい座標．</param>
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
        /// <param name="totalVelocity">現在の加速度．</param>
        /// <param name="deltaTime">デルタタイム．</param>
        protected abstract void ApplyPosition(in Vector3 totalVelocity, float deltaTime);

        /// <summary>
        /// 最終的な回転を適用する．
        /// </summary>
        /// <param name="rotation">適用する回転．</param>
        protected abstract void ApplyRotation(in Quaternion rotation);

        /// <summary>
        /// キャラクターの位置を <paramref name="newPosition"/> に移動する．
        /// <see cref="Warp(UnityEngine.Vector3)"/> とは異なり、他のベクトルの影響を受ける．
        /// </summary>
        /// <param name="newPosition">新しい位置．</param>
        protected abstract void SetPositionDirectly(in Vector3 newPosition);

        /// <summary>
        /// キャラクターを <paramref name="newRotation"/> に回転させる．
        /// <see cref="Warp(UnityEngine.Quaternion)"/> とは異なり、他の回転の影響を受ける．
        /// </summary>
        /// <param name="newRotation">新しい回転．</param>
        protected abstract void SetRotationDirectly(in Quaternion newRotation);

        /// <summary>
        /// キャラクターを <paramref name="newPosition"/> に移動する．
        /// <see cref="Warp(UnityEngine.Vector3)"/> とは異なり、他の移動の影響を受ける．
        /// </summary>
        /// <param name="newPosition">新しい位置．</param>
        protected abstract void MovePosition(in Vector3 newPosition);

        /// <summary>
        /// 継承した各 Brain の Position と Rotation をキャッシュする．
        /// </summary>
        /// <param name="position">キャッシュする位置．</param>
        /// <param name="rotation">キャッシュする回転．</param>
        protected abstract void GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
    }
}