using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Nitou.TCC.Foundation;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using R3;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.CharacterControl.Check
{
    /// <summary>
    /// 視界検出を行うコンポーネント．
    /// 視点から指定された範囲内にいるターゲットを、障害物を考慮して検出する．
    /// 視界内にターゲットがいる場合、またはすべてのオブジェクトが視界から出たときに <see cref="InsightTargets"/> を呼び出す．
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(SightCheck))]
    [DisallowMultipleComponent]
    public sealed class SightCheck : ComponentBase,
                                     IEarlyUpdateComponent
    {
        /// <summary>
        /// 検出に使用する頭の位置．
        /// </summary>
        [Title("Sight Settings")] 
        [SerializeField, Indent] public Transform _headTransform;

        /// <summary>
        /// 視界の範囲．
        /// </summary>
        [SerializeField, Indent] public int _range = 10;

        /// <summary>
        /// 視界の角度．
        /// </summary>
        [SerializeField, Indent] public int _angle = 30;

        /// <summary>
        /// 検出に使用するレイヤー．このレイヤーのオブジェクトが視認可能になる．
        /// </summary>
        public LayerMask VisibleLayerMask;

        /// <summary>
        /// 検出に使用するタグ．これらのタグを持つオブジェクトが視認可能になる．
        /// </summary>
        [SerializeField, Indent] public string[] _targetTagList;

        /// <summary>
        /// true の場合、障害物の存在を確認する．
        /// 障害物検出には <see cref="CharacterSettings._environmentLayer"/> が使用される．
        /// </summary>
        [Title("Options")] 
        public bool RaycastCheck = true;

        private Subject<bool> _onChangeInsightAnyTargetStateObject = new();
        
        /// <summary>
        /// 一度に検出できるオブジェクトの最大数．
        /// </summary>
        private const int CAPACITY = 100;
        
        private static readonly Collider[] Results = new Collider[CAPACITY];

        
        // ----------------------------------------------------------------------------
        // Property

        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        /// 視界内にあるオブジェクトのリストを取得する．
        /// </summary>
        public List<GameObject> InsightTargets { get; private set; } = new();

        /// <summary>
        /// 視界内で見つかった最初のオブジェクトを取得する．
        /// </summary>
        public GameObject InsightTarget => InsightTargets.Count > 0 ? InsightTargets[0] : null;

        /// <summary>
        /// true の場合、視界内にオブジェクトが存在する．
        /// </summary>
        public bool IsInsightAnyTarget => InsightTargets.Count > 0;

        /// <summary>
        /// オブジェクトが視界に入った、または出たときにイベントを呼び出す．
        /// </summary>
        public Observable<bool> OnChangeInsightAnyTargetStateObject => _onChangeInsightAnyTargetStateObject;
        

        // ----------------------------------------------------------------------------
        // Lifecycle Events

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            // 視界の変化を検出するために前の情報をキャッシュする
            var isAnyInsightTargetPreviousFrame = IsInsightAnyTarget;

            // センサーの位置の座標と方向を取得する
            var headPosition = _headTransform.position;
            var forward = _headTransform.forward;

            // キャラクター周辺のすべてのコライダーを収集する
            var count = Physics.OverlapSphereNonAlloc(headPosition, _range, Results,
                VisibleLayerMask, QueryTriggerInteraction.Ignore);

            // コライダーのリストからターゲットを抽出する
            InsightTargets.Clear();
            for (var i = 0; i < count; i++)
            {
                var col = Results[i];

                // 検出されたオブジェクトが自身のコライダーまたはタグリストにない場合は処理をスキップする
                if (CharacterSettings.IsOwnCollider(col) ||
                    col.gameObject.ContainTag(_targetTagList) == false)
                    continue;

                // 視界内の最も近い端の位置を検出する
                var closestPoint = col.ClosestPointOnBounds(headPosition);
                var deltaPosition = closestPoint - headPosition;

                // ターゲットが視界外にある場合は処理をスキップする
                if (Vector3.Angle(forward, deltaPosition) > _angle * 0.5f)
                    continue;

                // RaycastCheck が有効で、ターゲットが障害物に遮られている場合は処理をスキップする
                if (RaycastCheck &&
                    IsCollideTarget(headPosition, closestPoint, col))
                    continue;

                // 視界内のオブジェクトのリストにオブジェクトを追加する
                if (InsightTargets.Contains(col.gameObject) == false)
                    InsightTargets.Add(col.gameObject);
            }

            // 視界の状態が変化した場合は通知する
            if (IsInsightAnyTarget != isAnyInsightTargetPreviousFrame)
                _onChangeInsightAnyTargetStateObject.OnNext(IsInsightAnyTarget);
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// ターゲットオブジェクト間の障害物をチェックする．
        /// センサーのコライダーとターゲットコライダーは除外する．
        /// </summary>
        /// <param name="position">センサーの位置</param>
        /// <param name="targetPosition">ターゲットの最も近い位置</param>
        /// <param name="targetCollider">ターゲットオブジェクトのコライダー</param>
        /// <returns>障害物に遮られている場合は true</returns>
        private bool IsCollideTarget(in Vector3 position, in Vector3 targetPosition, in Collider targetCollider)
        {
            var deltaPosition = (targetPosition - position);
            var direction = deltaPosition.normalized;
            var distance = deltaPosition.magnitude;

            // バッファを割り当てる
            var hits = new RaycastHit[CAPACITY];

            // センサーからターゲットへの視界がクリアかどうかを確認する
            var count = Physics.RaycastNonAlloc(position, direction, hits, distance, CharacterSettings.EnvironmentLayer,
                QueryTriggerInteraction.Ignore);

            // 検出されたコライダーがターゲットのコライダーまたは自身に属する場合は処理をスキップする
            var isCollide = false;
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                // コライダーがターゲットまたは自身に属する場合は処理をスキップする
                if (targetCollider == hit.collider || CharacterSettings.IsOwnCollider(hit.collider))
                    continue;

                // 障害物に遮られているため検索処理を中断する
                isCollide = true;
                break;
            }

            // 遮られている場合は false を返す
            return isCollide;
        }


        // ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 最大値と最小値が有効な範囲内にあることを確認する
            _range = Mathf.Max(0, _range);
            _angle = Mathf.Clamp(_angle, 0, 360);
        }

        private void Reset()
        {
            // 視界に含めるレイヤーのデフォルト値を設定する
            VisibleLayerMask = LayerMask.GetMask("Default");
        }

#if TCC_USE_NGIZMOS
        // TODO: Gizmosの修正
        private void OnDrawGizmosSelected()
        {
            // ゲームがプレイ中でない場合は何もしない
            if (Application.isPlaying == false)
                return;

            // 視界内のオブジェクトを Gizmos を使用して表現する
            foreach (var obj in InsightTargets)
            {
                var position = obj.transform.position;
                NGizmo.DrawSphere(position, 1f, Colors.Yellow);
            }
        }
#endif
#endif
    }
}