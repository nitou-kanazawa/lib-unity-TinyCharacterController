using System;
using System.Linq;
using System.Collections.Generic;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
using Nitou.TCC.Utils;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Check
{
    /// <summary>
    /// 地面との衝突検出コンポーネント.地面が存在するかどうかや接触している面の向きなど,
    /// 接触しているオブジェクトに関する情報を判断し、地面オブジェクトの変化を通知する.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(MenuList.MenuCheck + nameof(GroundCheck))]
    public sealed class GroundCheck : MonoBehaviour,
                                      IGroundContact, IGroundObject,
                                      IEarlyUpdateComponent
    {
        /// <summary>
        /// 地面にいると認識される最大距離．(大まかな地面検出用）
        /// </summary>
        [Title("Parameters")] [PropertyRange(0, 2f)] [SerializeField, Indent]
        private float _ambiguousDistance = 0.2f;

        /// <summary>
        /// 地面にいると認識される距離．(厳密な地面検出用）
        /// </summary>
        [PropertyRange(0, 0.5f)] [SerializeField, Indent]
        private float _preciseDistance = 0.02f;

        /// <summary>
        /// 地面と認識される最大傾斜角度．
        /// 最も近い地面の傾斜がこの角度を超える場合、IsGroundはfalseになる
        /// </summary>
        [PropertyRange(0, 90)] [SerializeField, Indent]
        private int _maxSlope = 60;

        // references
        private CharacterSettings _characterSettings;
        private ITransform _transform;

        // 内部処理用
        private readonly RaycastHit[] _hits = new RaycastHit[MAX_COLLISION_SIZE];
        private RaycastHit _groundCheckHit;
        private readonly Subject<GameObject> _onGroundObjectChandedSubject = new();

        // 定数
        private const int MAX_COLLISION_SIZE = 5;


        // ----------------------------------------------------------------------------
        // Property

        /// <summary>
        /// 処理オーダー．
        /// </summary>
        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        /// 接地状態の大まかな判定．
        /// 範囲内にコライダーがある場合はtrueを返す．
        /// この計算結果は、アニメーターの状態間などで地面検出の微小な変動を避けたいときに使用される．
        /// </summary>
        public bool IsOnGround { get; private set; }

        /// <summary>
        /// 接地状態の厳密な判定．
        /// アクターの位置決めに使用される．
        /// </summary>
        public bool IsFirmlyOnGround { get; private set; }


        /// <summary>
        /// 地面オブジェクト. 
        /// 接地していない場合はnullを返す．
        /// </summary>
        public GameObject GroundObject { get; private set; } = null;

        /// <summary>
        /// 現在の地面コライダー．
        /// 接地していない場合はnullを返す．
        /// </summary>
        public Collider GroundCollider { get; private set; } = null;

        /// <summary>
        /// 地面までの距離．
        /// 接地していない場合は最大距離を返す．
        /// </summary>
        public float DistanceFromGround { get; private set; }

        /// <summary>
        /// 地面の法線ベクトル．
        /// 接地していない場合はVector3.Upを返す．
        /// </summary>
        public Vector3 GroundSurfaceNormal { get; private set; } = Vector3.up;

        /// <summary>
        /// 地面との接地位置．
        /// 接地していない場合はVector3.Zeroを返す．
        /// </summary>
        public Vector3 GroundContactPoint { get; private set; }

        /// <summary>
        /// 現在フレームで地面オブジェクトが切り替わったかどうか
        /// </summary>
        public bool IsChangeGroundObject { get; private set; } = false;


        /// <summary>
        /// 地面オブジェクトが変化したときのイベント通知
        /// </summary>
        public IObservable<GameObject> OnGrounObjectChanged => _onGroundObjectChandedSubject;


        // ----------------------------------------------------------------------------
        // Lifecycle Events
        private void Awake()
        {
            GatherComponents();
        }

        private void OnDestroy()
        {
            _onGroundObjectChandedSubject.Dispose();
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            using var _ = new ProfilerScope(nameof(GroundCheck));

            var preGroundObject = GroundObject;
            var offset = _characterSettings.Radius * 2;
            var origin = new Vector3(0, offset, 0) + (_transform?.Position ?? transform.position);
            var groundCheckDistance = _ambiguousDistance + offset;

            // Perform ground detection while ignoring the character's own collider.
            var groundCheckCount = Physics.SphereCastNonAlloc(
                origin, _characterSettings.Radius, Vector3.down, _hits,
                groundCheckDistance,
                _characterSettings.EnvironmentLayer, QueryTriggerInteraction.Ignore);

            var isHit = ClosestHit(_hits, groundCheckCount, groundCheckDistance, out _groundCheckHit);

            // fill the properties of the component based on the information of the ground.
            if (isHit)
            {
                var inLimitAngle = Vector3.Angle(Vector3.up, _groundCheckHit.normal) < _maxSlope;

                DistanceFromGround = _groundCheckHit.distance - (offset - _characterSettings.Radius);
                IsOnGround = DistanceFromGround < _ambiguousDistance;
                IsFirmlyOnGround = DistanceFromGround <= _preciseDistance && inLimitAngle;
                GroundContactPoint = _groundCheckHit.point;
                GroundSurfaceNormal = _groundCheckHit.normal;
                GroundCollider = _groundCheckHit.collider;
                GroundObject = IsOnGround ? _groundCheckHit.collider.gameObject : null;
            }
            else
            {
                DistanceFromGround = _ambiguousDistance;
                IsOnGround = false;
                IsFirmlyOnGround = false;
                GroundContactPoint = Vector3.zero;
                GroundSurfaceNormal = Vector3.zero;
                GroundCollider = null;
                GroundObject = null;
            }

            // If the object has changed, invoke _onChangeGroundObject.
            IsChangeGroundObject = preGroundObject != GroundObject;
            if (IsChangeGroundObject)
            {
                using var invokeProfile = new ProfilerScope($"{nameof(GroundCheck)}.Invoke");
                _onGroundObjectChandedSubject.OnNext(GroundObject);
            }
        }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// アクター自身にアタッチされているコライダーを無視して Raycast を実行する．
        /// この API は例えばキャラクターの前方にある段差を検出するために使用される．
        /// </summary>
        /// <param name="position">開始位置．</param>
        /// <param name="distance">レイの範囲．</param>
        /// <param name="hit">ヒットした RaycastHit を返す．</param>
        /// <returns>コライダーにヒットした場合は true を返す．</returns>
        public bool Raycast(Vector3 position, float distance, out RaycastHit hit)
        {
            var groundCheckCount = Physics.RaycastNonAlloc(position, Vector3.down, _hits, distance,
                _characterSettings.EnvironmentLayer, QueryTriggerInteraction.Ignore);

            // 最も近いオブジェクト
            return _characterSettings.ClosestHit(_hits, groundCheckCount, distance, out hit);
        }


        // ----------------------------------------------------------------------------
        // Private Method
        private void GatherComponents()
        {
            _characterSettings = GetComponentInParent<CharacterSettings>() ?? throw new System.NullReferenceException(nameof(_characterSettings));

            // Components
            _characterSettings.TryGetComponent(out _transform);
        }

        private bool ClosestHit(IReadOnlyList<RaycastHit> hits, int count, float maxDistance, out RaycastHit closestHit)
        {
            var min = maxDistance;
            closestHit = new RaycastHit();
            var isHit = false;

            foreach (var hit in hits.Take(count))
            {
                var isOverOriginHeight = (hit.distance == 0);
                if (isOverOriginHeight || hit.distance > min || _characterSettings.IsOwnCollider(hit.collider) || hit.collider == null)
                    continue;

                min = hit.distance;
                closestHit = hit;
                isHit = true;
            }

            return isHit;
        }

        
        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void Reset()
        {
            _ambiguousDistance = GetComponentInParent<CharacterSettings>().Height * 0.35f;
        }

#if TCC_USE_NGIZMOS
        // TODO: Gizmosの修正
        private void OnDrawGizmosSelected()
        {
            void DrawHitRangeGizmos(Vector3 startPosition, Vector3 endPosition)
            {
                var leftOffset = new Vector3(_characterSettings.Radius, 0, 0);
                var rightOffset = new Vector3(-_characterSettings.Radius, 0, 0);
                var forwardOffset = new Vector3(0, 0, _characterSettings.Radius);
                var backOffset = new Vector3(0, 0, -_characterSettings.Radius);

                Gizmos.DrawLine(startPosition + leftOffset, endPosition + leftOffset);
                Gizmos.DrawLine(startPosition + rightOffset, endPosition + rightOffset);
                Gizmos.DrawLine(startPosition + forwardOffset, endPosition + forwardOffset);
                Gizmos.DrawLine(startPosition + backOffset, endPosition + backOffset);
                NGizmo.DrawSphere(startPosition, _characterSettings.Radius, Color.yellow);
                NGizmo.DrawSphere(endPosition, _characterSettings.Radius, Color.yellow);
            }

            if (_characterSettings == null)
            {
                _characterSettings = gameObject.GetComponentInParent<CharacterSettings>();
            }

            var position = transform.position;
            var offset = _characterSettings.Height * 0.5f;


            if (Application.isPlaying)
            {
                Gizmos.color = IsOnGround ? Color.red : Gizmos.color;
                Gizmos.color = IsFirmlyOnGround ? Color.blue : Gizmos.color;

                var topPosition = new Vector3 { y = _characterSettings.Radius - _preciseDistance };
                var bottomPosition = IsOnGround
                    ? new Vector3 { y = _characterSettings.Radius - DistanceFromGround }
                    : new Vector3 { y = _characterSettings.Radius - _ambiguousDistance };

                DrawHitRangeGizmos(position + topPosition, position + bottomPosition);

                if (IsOnGround)
                {
                    NGizmo.DrawCollider(GroundCollider, Color.green);
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(_groundCheckHit.point, 0.1f);
                    Gizmos.DrawRay(_groundCheckHit.point, GroundSurfaceNormal * offset);
                }
            }
            else
            {
                var topPosition = new Vector3 { y = _characterSettings.Radius - _preciseDistance };
                var bottomPosition = new Vector3 { y = _characterSettings.Radius - _ambiguousDistance };
                DrawHitRangeGizmos(position + topPosition, position + bottomPosition);
            }
        }
#endif
#endif
    }
}