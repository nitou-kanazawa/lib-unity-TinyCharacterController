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
        [SerializeField, Indent] [MinValue(0)] private float _friction = 1f;

        /// <summary>
        /// 空気抵抗．
        /// </summary>
        [SerializeField, Indent] [MinValue(0)] private float _drag = 0.1f;

        /// <summary>
        /// 加速度を停止するための閾値．
        /// </summary>
        [SerializeField, Indent] [MinValue(0.1f)]
        private float _threshold = 0.5f;

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
            // キャラクターに影響するベクトルがある場合、減速と反発処理を実行する
            if (_velocity.magnitude > _threshold)
            {
                // 目的地にコライダーがある場合、ベクトルを反射する
                if (HasColliderOnDestination(dt, out var closestHit))
                {
                    // 他オブジェクトと衝突したときに呼び出されるイベント
                    // Velocity を補正する前に処理する
                    _onHitOtherCollider.OnNext(closestHit.collider);

                    if (_bounce > 0)
                    {
                        // 他の ExtraForce と衝突した場合、衝撃を伝播する
                        if (closestHit.collider.TryGetComponent(out ExtraForce other) &&
                            closestHit.collider.TryGetComponent(out CharacterSettings otherSettings))
                        {
                            // 力 = 質量 × 加速度
                            var ownForce = _settings.Mass * _velocity;
                            var otherForce = otherSettings.Mass * other._velocity;
                            var velocity = (ownForce + otherForce) / (_settings.Mass + otherSettings.Mass);

                            // 自身と衝突対象に加速度を追加する
                            other.AddForce(velocity * other._bounce);
                            _velocity = Vector3.Reflect(velocity, closestHit.normal) * _bounce;
                        }
                        else
                        {
                            _velocity = Vector3.Reflect(_velocity, closestHit.normal) * _bounce;
                        }
                    }
                }

                // キャラクターのベクトルを減速する．減速率は地面と空中で切り替わる
                var value = _groundCheck.IsOnGround ? _friction : _drag;
                _velocity -= _velocity * (dt * value);
            }
            else
            {
                // 加速度が閾値を下回った場合、ベクトルを無効化する
                _velocity = Vector3.zero;
            }
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// キャラクターに衝撃を追加する．
        /// </summary>
        /// <param name="value">力</param>
        public void AddForce(Vector3 value)
        {
            _velocity += value / _settings.Mass;
        }

        /// <summary>
        /// 速度を上書きする．
        /// </summary>
        /// <param name="value">新しい値</param>
        public void SetVelocity(Vector3 value)
        {
            _velocity = value;
        }

        /// <summary>
        /// 速度をリセットする．
        /// </summary>
        public void ResetVelocity()
        {
            _velocity = Vector3.zero;
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// 自身のオブジェクトにアタッチされているコンポーネントを収集する．
        /// </summary>
        private void GatherComponents()
        {
            _settings = GetComponentInParent<CharacterSettings>();

            _settings.TryGetComponent(out _transform);
            _settings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
        }

        /// <summary>
        /// カプセルの形状を取得するメソッド．
        /// </summary>
        /// <param name="headPoint">カプセルの上部の座標</param>
        /// <param name="bottomPoint">カプセルの下部の座標</param>
        private void GetBottomHeadPosition(out Vector3 headPoint, out Vector3 bottomPoint)
        {
            // カプセルの現在位置を取得する
            var point = _transform.Position;

            // カプセルの高さを取得する
            var height = _settings.Height;

            // カプセルの半径を取得する
            var radius = _settings.Radius;

            // カプセルの下部の座標を計算する
            bottomPoint = point + new Vector3(0, radius, 0);

            // カプセルの上部の座標を計算する
            headPoint = point + new Vector3(0, height - radius, 0);
        }

        /// <summary>
        /// 移動先にオブジェクトがあるかどうかを判定する．
        /// </summary>
        /// <param name="dt">前フレームからのデルタ時間</param>
        /// <param name="closestHit">最も近いコライダーの情報．オブジェクトがない場合はデフォルト値が設定される</param>
        /// <returns>オブジェクトが存在する場合は true</returns>
        private bool HasColliderOnDestination(float dt, out RaycastHit closestHit)
        {
            // キャラクターの頭部と下部の位置を取得する
            GetBottomHeadPosition(out var bottom, out var top);

            // 距離を計算：キャラクターの半径 + 1フレーム分の移動
            var distance = _settings.Radius * 0.5f + _velocity.magnitude * dt;

            // 衝突検出用の配列を作成する
            var hits = ArrayPool<RaycastHit>.Shared.Rent(HIT_CAPACITY);

            // キャラクターの形状で衝突検出を実行する
            var hitCount = Physics.CapsuleCastNonAlloc(top, bottom,
                _settings.Radius, _velocity.normalized, hits, distance,
                _settings.EnvironmentLayer, QueryTriggerInteraction.Ignore);

            // 範囲内のコライダーの中で、自身が所属するコライダーを除いた最も近いコライダーを見つける
            // isCapsuleHit はヒットが成功したかどうかを示す
            var isCapsuleHit = _settings.ClosestHit(hits, hitCount, distance, out var hit);

            ArrayPool<RaycastHit>.Shared.Return(hits);


            if (isCapsuleHit)
            {
                // 接触面の法線を取得する
                // このステップがないと、地面との接触時に予期しない反転が発生する可能性がある
                var normal = _velocity.normalized;
                var ray = new Ray(hit.point - normal * 0.1f, normal);

                // レイキャストを実行してコライダー上の最も近いヒット点を見つける
                var result = hit.collider.Raycast(ray, out closestHit, 1);

                return result;
            }

            // 衝突がない場合、closestHit をデフォルト値に設定して False を返す
            closestHit = default;
            return false;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            // キャラクターの中心位置を計算する
            var centerPosition = _transform.Position + new Vector3(0, _settings.Height * 0.5f, 0);

            // 移動のターゲット位置を計算する
            var maxDistance = _velocity.magnitude * 0.28f;
            var targetPosition = centerPosition + _velocity.normalized * maxDistance;

            // 移動ベクトルを表現する
            var sphereColor = Color.blue;
            sphereColor.a = 0.4f;
            NGizmo.DrawSphere(targetPosition, _settings.Radius, sphereColor);

            // ターゲット位置への線と移動ベクトルのワイヤーフレームを表現する
            var color = Color.white;
            NGizmo.DrawLine(targetPosition, centerPosition, color);
            NGizmo.DrawWireSphere(targetPosition, _settings.Radius, color);
        }
#endif
    }
}