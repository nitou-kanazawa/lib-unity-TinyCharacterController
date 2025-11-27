using System.Collections.Generic;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;
using Nitou.TCC.Foundation;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Check
{
    /// <summary>
    /// 上方向のオブジェクト検出を行うコンポーネント．
    /// CharacterSettings で設定された高さを考慮して上方向の検出を行う．
    /// 完全な接触だけでなく、わずかに曖昧な検出も考慮し、衝突時に UnityEvent を呼び出す．
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(MenuList.MenuCheck + nameof(HeadContactCheck))]
    public class HeadContactCheck : ComponentBase,
                                    IOverheadDetection,
                                    IEarlyUpdateComponent,
                                    IComponentCondition
    {
        /// <summary>
        /// 頭の位置からのオフセット．
        /// この値は上方のオブジェクトが接触しているかを判定するために使用される．
        /// 天井に接触しているときにオブジェクトが検出できない場合、この値を大きく設定する．
        /// </summary>
        [Title("Settings")]
        [Tooltip("Offset from the head position")]
        [Range(0, 0.5f)]
        [SerializeField, Indent] private float _headPositionOffset = 0.1f;

        /// <summary>
        /// 上方向のオブジェクトを検出できる最大距離．
        /// 天井の低いエリアに入ったときなど、直接接触していなくても頭上にオブジェクトがあるかを確認する際に使用される．
        /// この値は常に <see cref="CharacterSettings.Height" /> の高さよりも大きくなければならない．
        /// </summary>
        [FormerlySerializedAs("MaxRange")]
        [Tooltip("Maximum distance at which upward objects can be detected")]
        [Range(0, 10f)]
        [SerializeField, Indent] private float MaxHeight = 2.5f;


        /// <summary>
        /// このフレームで頭が接触したときにコールバックを呼び出す UnityEvent．
        /// </summary>
        [Title("Callbacks")]
        [SerializeField, Indent] private UnityEvent _onContact;

        /// <summary>
        /// InRange の値が変化したときに実行される．
        /// </summary>
        [SerializeField, Indent] private UnityEvent _onChangeInRange;

        // references
        private ITransform _transform;

        // 定数

        /// <summary>
        /// 検出可能なコライダーの数．
        /// </summary>
        private const int MAX_CONTACT_SIZE = 5;

        private static readonly RaycastHit[] Result = new RaycastHit[MAX_CONTACT_SIZE];


        // ----------------------------------------------------------------------------

        #region Properites

        /// <inheritdoc/>
        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        /// このフレームで頭が他のオブジェクトと接触している場合は true を返す．
        /// </summary>
        public bool IsHitCollisionInThisFrame { get; private set; }

        /// <summary>
        /// <see cref="IsHeadContact" /> が true の場合、<see cref="ContactPoint" /> から RootPosition までの距離を返す．
        /// </summary>
        public float DistanceFromRootPosition { get; private set; }

        /// <summary>
        /// 頭までの高さ．
        /// </summary>
        private float Height => CharacterSettings.Height + _headPositionOffset;

        /// <summary>
        /// 頭が他のオブジェクトと接触している場合は true を返す．
        /// </summary>
        public bool IsHeadContact { get; private set; }

        /// <summary>
        /// キャラクターの頭から Max Range の範囲内にコライダーがある場合は true を返す．
        /// </summary>
        public bool IsObjectOverhead { get; private set; }

        /// <summary>
        /// <see cref="IsHeadContact" /> が true の場合は接触点を返し、false の場合は頭の位置を返す．
        /// </summary>
        /// <seealso cref="MaxHeight" />
        public Vector3 ContactPoint { get; private set; }

        /// <summary>
        /// 衝突した GameObject を返す．<see cref="IsHeadContact" /> が false の場合は null を返す．
        /// このプロパティを使用する前に <see cref="IsHeadContact" /> の存在を確認することを推奨する．
        /// </summary>
        public GameObject ContactedObject { get; private set; }

        #endregion


        // ----------------------------------------------------------------------------

        #region Lifecycle Events

        protected override void OnComponentInitialized()
        {
            CharacterSettings.TryGetComponent(out _transform);
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            using var profile = new ProfilerScope(nameof(HeadContactCheck));

            // 前フレームからの変化を検出するために現在の値をキャッシュする
            var preInRange = IsObjectOverhead;

            // Raycast に必要なパラメータを準備する
            // 地面との接触を避けるため、体の中心からレイを投げる
            var offset = CharacterSettings.Height * 0.5f;
            IsObjectOverhead = DetectCollidersAboveHead(offset, out var closestHit);

            if (IsObjectOverhead)
            {
                // コライダーが検出されたときの動作

                SetPropertiesWhenInRange(closestHit, offset);

                // オブジェクトがヒットしたのでイベントを実行する
                if (IsHitCollisionInThisFrame)
                    _onContact?.Invoke();
            }
            else
            {
                // コライダーが検出されなかった

                SetPropertiesWhenOutOfRange();
            }

            // 範囲内の存在が変化した場合はイベントを呼び出す
            if (preInRange != IsObjectOverhead)
                _onChangeInRange?.Invoke();
        }

        void IComponentCondition.OnConditionCheck(List<string> messageList)
        {
            if (CharacterSettings == null)
                TryGetComponent(out CharacterSettings);

            if (CharacterSettings.Height > MaxHeight)
                messageList.Add("Max Range は _settings.Height 以上の値に設定する必要があります．");
        }

        #endregion

        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// 範囲外のときにプロパティを設定する．
        /// 頭上にコライダーが検出されなかったときに呼び出される．
        /// </summary>
        private void SetPropertiesWhenOutOfRange()
        {
            var distance = MaxHeight + _headPositionOffset;

            // ContactPoint は頭上の位置
            ContactPoint = new Vector3(0, distance, 0);

            // Distance は頭上の位置
            DistanceFromRootPosition = distance;
            IsHeadContact = false;
            ContactedObject = null;
            IsHitCollisionInThisFrame = false;
        }

        /// <summary>
        /// 範囲内のときにプロパティを設定する．
        /// 頭上にコライダーが検出されたときに呼び出される．
        /// </summary>
        /// <param name="closestHit">最も近い RaycastHit</param>
        /// <param name="offset">Raycast 開始時のオフセット</param>
        private void SetPropertiesWhenInRange(in RaycastHit closestHit, float offset)
        {
            // 前フレームとの差異を検出するために現在の値をキャッシュする
            var preContactHead = IsHeadContact;

            // Distance は RaycastHit の距離、Cast 開始時のオフセット、SphereCast のオフセットの合計
            DistanceFromRootPosition = closestHit.distance + offset + CharacterSettings.Radius + _headPositionOffset;
            ContactPoint = closestHit.point;
            ContactedObject = closestHit.collider.gameObject;

            // 地面からの距離が高さ設定よりも低い場合、衝突と見なす
            // また、衝突検出が前フレームと異なる場合、現在フレームでのヒットと見なす
            IsHeadContact = DistanceFromRootPosition < CharacterSettings.Height + _headPositionOffset;
            IsHitCollisionInThisFrame = !preContactHead && IsHeadContact;
        }

        /// <summary>
        /// 頭上のコライダーを検出し、最も近いコライダーを特定する．
        /// 自身が所有するコライダーは除外する．
        /// </summary>
        /// <param name="offset">検出のオフセット</param>
        /// <param name="closestHit">最も近いコライダーの RayCast</param>
        /// <returns>検出可能なコライダーが範囲内にある場合は true</returns>
        private bool DetectCollidersAboveHead(float offset, out RaycastHit closestHit)
        {
            var ray = new Ray(_transform.Position + new Vector3(0, offset + _headPositionOffset, 0), Vector3.up);
            var rayDistance = MaxHeight + _headPositionOffset - offset - CharacterSettings.Radius;

            // 上方向に SphereCast を実行して頭上のコライダーの存在を確認する
            var count = Physics.SphereCastNonAlloc(ray, CharacterSettings.Radius, Result,
                rayDistance, CharacterSettings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // 自身が所有するコライダーを除外して最も近い Raycast を取得する
            var isHit = CharacterSettings.ClosestHit(Result, count, rayDistance, out closestHit);
            return isHit;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR && TCC_USE_NGIZMOS
        // TODO: Gizmosの修正
        private void OnDrawGizmosSelected()
        {
            if (CharacterSettings == null)
            {
                GatherComponents();
            }

            // コライダーが接触していると見なされる場合、基本色を赤に設定する
            if (IsHeadContact)
                Gizmos.color = Color.red;

            var position = transform.position;
            var headPosition = position + new Vector3(0, Height - CharacterSettings.Radius, 0);

            if (Application.isPlaying)
            {
                // DistanceFromRootPosition に基づいて衝突が検出された位置を表現する
                var offset = CharacterSettings.Radius;
                var contactPosition = position + new Vector3(0, DistanceFromRootPosition - offset, 0);
                DrawHitRangeGizmos(headPosition, contactPosition);

                if (IsHeadContact)
                    NGizmo.DrawCollider(ContactedObject.GetComponent<Collider>(), Color.yellow);
            }
            else
            {
                // MaxHeight に基づいて頭の位置を表現する
                var maxHeightPosition = position + new Vector3(0, MaxHeight, 0);
                DrawHitRangeGizmos(headPosition, maxHeightPosition);
            }

            return;

            // カプセル形状の Gizmos を表現する
            void DrawHitRangeGizmos(in Vector3 startPosition, in Vector3 endPosition)
            {
                var leftOffset = new Vector3(CharacterSettings.Radius, 0, 0);
                var rightOffset = new Vector3(-CharacterSettings.Radius, 0, 0);
                var forwardOffset = new Vector3(0, 0, CharacterSettings.Radius);
                var backOffset = new Vector3(0, 0, -CharacterSettings.Radius);

                // カプセルの前後の垂直線を表現する
                Gizmos.DrawLine(startPosition + leftOffset, endPosition + leftOffset);
                Gizmos.DrawLine(startPosition + rightOffset, endPosition + rightOffset);
                Gizmos.DrawLine(startPosition + forwardOffset, endPosition + forwardOffset);
                Gizmos.DrawLine(startPosition + backOffset, endPosition + backOffset);

                // カプセルの上下の円形を表現する
                Gizmos.DrawWireSphere(startPosition, CharacterSettings.Radius);
                Gizmos.DrawWireSphere(endPosition, CharacterSettings.Radius);

                // カプセルの上下の円形を色で塗りつぶす
                var color = Colors.Yellow;
                color.a = 0.4f;
                NGizmo.DrawSphere(startPosition, CharacterSettings.Radius, color);
                NGizmo.DrawSphere(endPosition, CharacterSettings.Radius, color);
            }
        }
#endif
    }
}