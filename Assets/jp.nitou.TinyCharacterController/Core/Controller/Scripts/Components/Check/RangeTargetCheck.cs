using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Nitou.TCC.Controller.Interfaces.Components;
using Nitou.TCC.Controller.Core;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using Nitou.TCC.Utils;
#if TCC_USE_NGIZMOS
using Nitou.Gizmo;
#endif

namespace Nitou.TCC.Controller.Check
{
    /// <summary>
    /// 視線と障害物を考慮して、範囲内にある特定のタグを持つオブジェクトを取得するコンポーネント．
    /// 毎フレーム実行され、内容に変化がある場合はコールバックを呼び出す．
    /// 主に一定範囲内のオブジェクトを取得するために使用される．
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(RangeTargetCheck))]
    [DisallowMultipleComponent]
    public sealed class RangeTargetCheck : MonoBehaviour,
                                           IEarlyUpdateComponent
    {
        /// <summary>
        /// センサーの中心点．
        /// </summary>
        [SerializeField] private Vector3 _sensorOffset = new(0, 0.5f, 0);

        /// <summary>
        /// センサーが検出できるレイヤー．
        /// </summary>
        [SerializeField] private LayerMask _hitLayer;

        /// <summary>
        /// Environment Layer で無視するレイヤー．
        /// 透明な窓など、移動可能だが検出不可にしたいオブジェクトを指定するために使用される．
        /// </summary>
        [SerializeField] private LayerMask _transparentLayer;

        /// <summary>
        /// センサーが検索するターゲットを指定するプロパティ．
        /// ターゲットのタグ、検索範囲、視認性を考慮するかどうかなどのプロパティを設定できる．
        ///
        /// この設定はコンポーネントのセンサーの範囲を決定する．
        /// </summary>
        [SerializeField] private SearchRangeSettings[] _searchData;

        // 
        private readonly List<string> _tags = new();
        private SearchedTarget[] _searchTargets;
        private float _maxDistance = -1;
        private bool _useScreenCheck = true;
        private LayerMask _raycastHitLayer;

        // 
        private CharacterSettings _settings;
        private ITransform _transform;

        // 
        private static readonly Collider[] OverlapSphereResult = new Collider[CAPACITY];
        private static readonly RaycastHit[] Hits = new RaycastHit[30];
        private static readonly Plane[] CameraPlanes = new Plane[6];

        // Constants
        private const int CAPACITY = 100;

        int IEarlyUpdateComponent.Order => Order.Check;


        public SearchRangeSettings GetSearchRangeSettings(int index) => _searchData[index];

        public SearchRangeSettings GetSearchRangeSettings(string tagName)
        {
            var index = GetTagIndex(tagName);
            if (index == -1)
                throw new Exception("SearchData not found!");

            return GetSearchRangeSettings(index);
        }

        /// <summary>
        /// 特定の範囲内にある特定のタグを持つオブジェクトのリストを取得する．
        /// このコレクションの収集は Check 時に行われる集計に基づいている．
        /// _searchData に見つからないタグは Null を返す．
        /// </summary>
        /// <param name="tagName">タグのインデックス</param>
        /// <returns>範囲内の Transform のリスト</returns>
        public List<Transform> GetTargets(string tagName)
        {
            var tagIndex = GetTagIndex(tagName);
            return GetTargets(tagIndex);
        }

        /// <summary>
        /// 特定の範囲内にある特定のタグを持つオブジェクトのリストを取得する．
        /// このコレクションの収集は Check 時に行われる集計に基づいている．
        /// _searchData に見つからないタグは Null を返す．
        /// </summary>
        /// <param name="index">タグのインデックス</param>
        /// <returns>範囲内の Transform のリスト</returns>
        public List<Transform> GetTargets(int index)
        {
            return index == -1 ? null : _searchTargets[index].Targets;
        }

        /// <summary>
        /// 指定されたタグを持つオブジェクトが存在しないかどうかを確認する．
        /// </summary>
        /// <param name="tagIndex">タグのインデックス</param>
        /// <returns>指定されたタグを持つオブジェクトが存在しない、またはタグが検索対象に含まれていない場合は true を返す</returns>
        public bool IsEmpty(int tagIndex)
        {
            if (tagIndex < _searchTargets.Length && tagIndex >= 0)
                return _searchTargets[tagIndex].Targets.Count == 0;
            return
                true;
        }

        /// <summary>
        /// 指定されたタグを持つオブジェクトが存在しないかどうかを確認する．
        /// </summary>
        /// <param name="tagName">タグの名前</param>
        /// <returns>指定されたタグを持つオブジェクトが存在しない、またはタグが検索対象に含まれていない場合は true を返す</returns>
        public bool IsEmpty(string tagName)
        {
            var tagIndex = GetTagIndex(tagName);
            return IsEmpty(tagIndex);
        }

        /// <summary>
        /// 事前計算された結果を使用して、指定されたタグを持つ最も近いターゲットを取得する．
        /// 要求されたタグが範囲に含まれていない場合、または範囲内にターゲットがない場合は False を返す．
        /// </summary>
        /// <param name="tagName">タグの名前</param>
        /// <param name="target">最も近いターゲット</param>
        /// <returns>ターゲットが見つかった場合は true</returns>
        public bool TryGetClosestTarget(string tagName, out Transform target)
        {
            var tagIndex = GetTagIndex(tagName);
            if (tagIndex == -1)
            {
                target = null;
                return false;
            }

            target = _searchTargets[tagIndex].ClosestTarget.CurrentTransform;
            return target != null;
        }

        /// <summary>
        /// タグが格納されているインデックスを取得する．
        /// </summary>
        /// <param name="tagName">タグの名前</param>
        /// <returns>タグのインデックス</returns>
        public int GetTagIndex(string tagName)
        {
            return _tags.IndexOf(tagName);
        }

        /// <summary>
        /// 事前計算された結果を使用して、指定されたタグを持つ最も近いターゲットを取得する．
        /// 要求されたタグが範囲に含まれていない場合、または範囲内にターゲットがない場合は False を返す．
        /// </summary>
        /// <param name="tagName">タグの名前</param>
        /// <param name="target">最も近いターゲット</param>
        /// <param name="preTarget">前回の最も近いターゲット</param>
        /// <returns>ターゲットが見つかった場合は true</returns>
        public bool TryGetClosestTarget(string tagName, out Transform target, out Transform preTarget)
        {
            var tagIndex = GetTagIndex(tagName);
            ;
            if (tagIndex == -1)
            {
                target = null;
                preTarget = null;
                return false;
            }

            var item = _searchTargets[tagIndex].ClosestTarget;
            target = item.CurrentTransform;
            preTarget = item.PreTransform;
            return target != null;
        }

        /// <summary>
        /// 指定されたタグ範囲内で新しく追加または削除されたターゲットのリストを取得する．
        /// リストに変更がない場合、または指定されたタグが検索対象に含まれていない場合は False を返す．
        /// 指定されたタグが検索対象に含まれていない場合、Added と Removed は Null になる．
        /// </summary>
        /// <param name="tagName">タグの名前</param>
        /// <param name="added">範囲内のオブジェクトのリスト</param>
        /// <param name="removed">範囲外のオブジェクトのリスト</param>
        /// <returns>オブジェクトが追加または削除された場合は true</returns>
        public bool ChangedValues(string tagName, out List<Transform> added, out List<Transform> removed)
        {
            var tagIndex = GetTagIndex(tagName);
            return ChangedValues(tagIndex, out added, out removed);
        }

        /// <summary>
        /// 指定されたタグ範囲内で新しく追加または削除されたターゲットのリストを取得する．
        /// リストに変更がない場合、または指定されたタグが検索対象に含まれていない場合は False を返す．
        /// 指定されたタグが検索対象に含まれていない場合、Added と Removed は Null になる．
        /// </summary>
        /// <param name="tagIndex">タグのインデックス</param>
        /// <param name="added">範囲内のオブジェクトのリスト</param>
        /// <param name="removed">範囲外のオブジェクトのリスト</param>
        /// <returns>オブジェクトが追加または削除された場合は true</returns>
        public bool ChangedValues(int tagIndex, out List<Transform> added, out List<Transform> removed)
        {
            if (tagIndex == -1)
            {
                added = null;
                removed = null;
                return false;
            }

            added = _searchTargets[tagIndex].Added;
            removed = _searchTargets[tagIndex].Removed;

            return added.Count + removed.Count > 0;
        }


        /// ----------------------------------------------------------------------------

        #region Lifecycle Events

        private void Awake()
        {
            GatherComponents();

            _raycastHitLayer = _settings.EnvironmentLayer & ~_transparentLayer;

            foreach (var data in _searchData)
            {
                _tags.Add(data.Tag);
                _useScreenCheck |= data.ExcludeHiddenFromCamera;
                if (data.Range > _maxDistance)
                    _maxDistance = data.Range;
            }

            _searchTargets = new SearchedTarget[_tags.Count];
            for (var i = 0; i < _tags.Count; i++)
            {
                _searchTargets[i] = new SearchedTarget();
            }
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            using var profiler = new ProfilerScope("Range Check");

            // センサー位置を中心に、指定されたレイヤーを持つコライダーのリストを取得する

            var sensorPosition = _transform.Position + _sensorOffset;
            var cameraPosition = _settings.CameraTransform.position;
            var count = Physics.OverlapSphereNonAlloc(
                sensorPosition,
                _maxDistance,
                OverlapSphereResult,
                _hitLayer,
                QueryTriggerInteraction.Collide);

            if (_useScreenCheck)
                GeometryUtility.CalculateFrustumPlanes(_settings.CameraMain, CameraPlanes);

            // コライダー、オブジェクト座標、コライダー境界までの距離を取得する

            using var hitStatePo = UnityEngine.Pool.ListPool<ValueTuple<Collider, Vector3, float>>
                                              .Get(out var hitState);

            for (var index = 0; index < count; index++)
            {
                var col = OverlapSphereResult[index];
                var closePoint = col.ClosestPointOnBounds(sensorPosition);
                var distance = Vector3.Distance(sensorPosition, closePoint);
                hitState.Add(new ValueTuple<Collider, Vector3, float>(col, closePoint, distance));
            }

            // リストから各タグを割り当て、新しく追加された要素と削除された要素を選択する

            for (var dataIndex = 0; dataIndex < _searchData.Length; dataIndex++)
            {
                var data = _searchData[dataIndex];
                var target = _searchTargets[dataIndex];

                using var preTransformPo = UnityEngine.Pool.ListPool<Transform>.Get(out var preTransforms);
                preTransforms.AddRange(target.Targets);
                target.Colliders.Clear();
                target.Targets.Clear();
                target.Added.Clear();
                target.Removed.Clear();

                foreach (var hit in hitState)
                {
                    if (data.Range < hit.Item3 ||
                        hit.Item1.CompareTag(data.Tag) == false)
                        continue;

                    if (target.Targets.Contains(hit.Item1.transform))
                        continue;

                    if (data.ExcludeOutOfView && GeometryUtility.TestPlanesAABB(CameraPlanes, hit.Item1.bounds) == false)
                        continue;

                    if (data.ExcludeHiddenFromPlayer && RaycastCheck(sensorPosition, hit.Item2, hit.Item1))
                        continue;

                    if (data.ExcludeHiddenFromCamera && RaycastCheck(cameraPosition, hit.Item2, hit.Item1))
                        continue;

                    target.Colliders.Add(hit.Item1);
                    target.Targets.Add(hit.Item1.transform);
                }

                foreach (var newTransform in target.Targets)
                {
                    if (newTransform != null && !preTransforms.Contains(newTransform))
                        target.Added.Add(newTransform);
                }

                foreach (var preTransform in preTransforms)
                {
                    if (preTransform != null && !target.Targets.Contains(preTransform))
                        target.Removed.Add(preTransform);

                    if (preTransform == null)
                        target.Removed.Add(null);
                }

                if (target.Added.Count != 0 || target.Removed.Count != 0)
                    data.OnChangeValue?.Invoke(new ValueTuple<List<Transform>, List<Transform>>(target.Added, target.Removed));
            }


            // 

            for (var index = 0; index < _searchData.Length; index++)
            {
                var data = _searchData[index];
                if (data.CalculateNearest == false)
                    continue;

                var tagIndex = _tags.FindIndex(c => c == data.Tag);
                var isEmpty = IsEmpty(tagIndex);
                var closest = isEmpty ? null : GetClosest(sensorPosition, _searchTargets[index].Targets);

                var item = _searchTargets[tagIndex].ClosestTarget;

                var id = isEmpty ? 0 : closest.GetInstanceID();

                if (id != item.PreTransformInstance)
                {
                    _searchTargets[tagIndex].ClosestTarget = new TargetData
                    {
                        CurrentTransform = closest,
                        PreTransform = item.CurrentTransform,
                        PreTransformInstance = id
                    };
                    data.OnChangeClosestTarget.Invoke();
                }
            }
        }

        #endregion


        /// ----------------------------------------------------------------------------
        // Public Method
        private bool RaycastCheck(Vector3 position, Vector3 closePoint, Collider targetCollider)
        {
            const float offset = 0.1f;

            for (var i = 0; i < Hits.Length; i++)
                Hits[i] = default;

            var distance = Vector3.Distance(position, closePoint) - offset;
            var ray = new Ray(position, closePoint - position);
            var count = Physics.RaycastNonAlloc(ray, Hits, distance, _raycastHitLayer, QueryTriggerInteraction.Ignore);
            count -= Array.Exists(Hits, h => h.collider == targetCollider) ? 1 : 0;
            return count > 0;
        }

        private void GatherComponents()
        {
            _settings = GetComponentInParent<CharacterSettings>();
            _transform = _settings.GetComponent<ITransform>();
        }

        private Transform GetClosest(Vector3 position, List<Transform> targets)
        {
            float minDistance = float.MaxValue;
            Transform nearest = null;
            foreach (var target in targets)
            {
                var distance = Vector3.Distance(position, target.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = target;
                }
            }

            return nearest;
        }


        #region Inner Definition

        private class SearchedTarget
        {
            public readonly List<Collider> Colliders = new();
            public readonly List<Transform> Targets = new();
            public readonly List<Transform> Added = new();
            public readonly List<Transform> Removed = new();

            public TargetData ClosestTarget;
        }

        private struct TargetData
        {
            public Transform PreTransform;
            public Transform CurrentTransform;
            public int PreTransformInstance;
        }

        [System.Serializable]
        public class SearchRangeSettings
        {
            [Header("Settings")]
            /// <summary>
            /// ターゲットオブジェクトのタグ．
            /// </summary>
            // [TagSelector]
            public string Tag;

            /// <summary>
            /// 検索範囲の半径．
            /// </summary>
            public float Range;

            [Header("Options")]
            /// <summary>
            /// カメラの視界外にあるオブジェクトを除外する．
            /// </summary>
            public bool ExcludeOutOfView;

            /// <summary>
            /// カメラとの間に障害物があるオブジェクトを除外する．
            /// </summary>
            public bool ExcludeHiddenFromCamera;

            /// <summary>
            /// プレイヤーとの間に障害物があるオブジェクトを除外する．
            /// </summary>
            public bool ExcludeHiddenFromPlayer;

            /// <summary>
            /// 最も近いオブジェクトを計算する．デフォルトで有効．
            /// </summary>
            public bool CalculateNearest = true;

            [Header("Event")]
            /// <summary>
            /// 最も近いターゲットが変化したときにトリガーされるイベント．
            /// </summary>
            public UnityEvent OnChangeClosestTarget;

            /// <summary>
            /// 検出されたターゲットのリストが変化したときにトリガーされるイベント．
            /// </summary>
            public UnityEvent<(List<Transform>, List<Transform>)> OnChangeValue;
        }

        #endregion


        /// ----------------------------------------------------------------------------
#if UNITY_EDITOR
        private readonly Color[] _colors = new[]
        {
            Color.yellow, Color.red, Color.blue, Color.green,
            Color.cyan, Color.white, Color.magenta,
        };


#if TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            var center = _sensorOffset + transform.position;


            for (var i = 0; i < _searchData.Length; i++)
            {
                var data = _searchData[i];
                var currentColor = _colors[i % _colors.Length];
                if (Mathf.Approximately(0, data.Range) || data.Range < 0)
                    continue;

                NGizmo.DrawWireSphere(center, data.Range, currentColor);
                Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.2f);
                Gizmos.DrawSphere(center, data.Range);
            }

            if (Application.isPlaying == false)
                return;

            for (var i = 0; i < _searchData.Length; i++)
            {
                var currentColor = _colors[i % _colors.Length];
                var targets = _searchTargets[i].Targets;
                UnityEditor.Handles.DrawOutline(targets.Select(c => c.gameObject).ToArray(), currentColor, 0.4f);

                var colliders = _searchTargets[i].Colliders;
                var closest = _searchTargets[i].ClosestTarget.CurrentTransform;
                for (var index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    Gizmos.color = target == closest ? Color.red : Color.white;
                    Gizmos.DrawWireCube(colliders[index].bounds.center, colliders[index].bounds.size);
                    Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.4f);
                    Gizmos.DrawCube(colliders[index].bounds.center, colliders[index].bounds.size);
                }
            }
        }
#endif
#endif
    }
}