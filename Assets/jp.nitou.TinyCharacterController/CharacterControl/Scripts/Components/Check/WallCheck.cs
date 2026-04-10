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
    /// 壁との衝突判定を行うコンポーネント．
    /// キャラクターの移動方向に対して壁を検出する．
    /// 壁との衝突が発生したとき、接触中、壁から離れたときにコールバックを呼び出す．
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(WallCheck))]
    [DisallowMultipleComponent]
    public sealed class WallCheck : ComponentBase,
                                    IEarlyUpdateComponent, IWallCheck
    {
        [Title("Settings")]
        [MinMaxSlider(15, 165)]
        [SerializeField, Indent] private Vector2 _wallAngleRange = new(75, 115);

        [Range(0.01f, 1f)]
        [SerializeField, Indent] private float _wallDetectionDistance = 0.1f;

        // References
        private IBrain _brain;
        private ITransform _transform;

        private Vector3 _normal;
        private Vector3 _hitPoint;
        private Collider _contactObject;
        private static readonly RaycastHit[] Hits = new RaycastHit[5];

        /// <summary>
        /// 接触がある場合は True を返す．
        /// </summary>
        public bool IsContact { get; private set; }

        /// <summary>
        /// 接触面の法線ベクトルを返す．接触がない場合は Vector3.Zero を返す．
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
        /// 壁に接触したときに呼び出される．
        /// </summary>
        public Observable<Unit> OnWallContacted => _onWallContactedSubject;

        /// <summary>
        /// 壁から離れたときに呼び出される．
        /// </summary>
        public Observable<Unit> OnWallLeft => _onWallLeftSubject;

        /// <summary>
        /// 壁に接触し続けているときに呼び出される．
        /// </summary>
        public Observable<Unit> OnWallStuck => _onWallStuckSubject;


        #region Lifecycle Events

        protected override void OnComponentInitialized()
        {
            CharacterSettings.TryGetComponent(out _transform);
            CharacterSettings.TryGetComponent(out _brain);
        }

        private void OnDestroy()
        {
            _onWallContactedSubject.Dispose();
            _onWallLeftSubject.Dispose();
            _onWallStuckSubject.Dispose();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            // コンポーネントが無効な場合は更新処理を行わない．
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
        /// 壁判定を即座に実行する．
        /// この計算結果はコンポーネントの計算とは独立して処理され、計算結果は保存されない．
        /// 同じコンポーネントのコライダーは無視される．
        /// </summary>
        /// <param name="direction">判定する方向</param>
        /// <param name="normal">接触面の法線（結果）</param>
        /// <param name="point">ヒット地点（結果）</param>
        /// <param name="contactCollider">接触したオブジェクト．接触なしの場合は null を返す．</param>
        /// <returns>いずれかのコライダーに接触しているか</returns>
        public bool HitCheck(Vector3 direction, out Vector3 normal, out Vector3 point, out Collider contactCollider)
        {
            var distance = CharacterSettings.Radius + _wallDetectionDistance;
            var halfHeight = CharacterSettings.Height * 0.5f;
            var centerPosition = _transform.Position + Vector3.up * halfHeight;
            var ray = new Ray(centerPosition, direction);
            var count = Physics.SphereCastNonAlloc(ray, CharacterSettings.Radius, Hits, distance, CharacterSettings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // 最も近いヒットを取得する．
            var hasClosestHit = CharacterSettings.ClosestHit(Hits, count, distance, out var hit);
            if (hasClosestHit)
            {
                // 角度制限を適用する．
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false)
                return;

            if (IsContact)
                NGizmo.DrawCollider(in _contactObject, Color.yellow);

            var distance = CharacterSettings.Radius + 0.1f;
            var halfHeight = CharacterSettings.Height * 0.5f;
            var centerPosition = _transform.Position + Vector3.up * halfHeight;
            var direction = _brain.ControlVelocity.normalized;
            var position = centerPosition + direction * distance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, CharacterSettings.Radius);

            Gizmos.color = IsContact ? Color.red : Color.white;
            Gizmos.DrawWireSphere(position, CharacterSettings.Radius);
        }
#endif
    }
}