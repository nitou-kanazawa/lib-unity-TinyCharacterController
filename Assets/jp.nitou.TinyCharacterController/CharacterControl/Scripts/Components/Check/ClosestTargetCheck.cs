using System;
using Nitou.Gizmo;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.TCC.Foundation;
using R3;
using UnityEngine;
using UnityEngine.Events;
using Sirenix;
using Sirenix.OdinInspector;

namespace Nitou.TCC.CharacterControl.Check
{
    /// <summary>
    /// 一定範囲内で最も近いターゲットを取得するコンポーネント．
    /// </summary>
    [AddComponentMenu(MenuList.MenuCheck + nameof(ClosestTargetCheck))]
    [DisallowMultipleComponent]
    public sealed class ClosestTargetCheck : ComponentBase,
                                             IEarlyUpdateComponent
    {
        /// <summary>
        /// 検索対象のタグ．
        /// </summary>
        [TagSelector]
        [SerializeField, Indent] private string _tag;

        /// <summary>
        /// 検索範囲の半径．
        /// </summary>
        [SerializeField, Indent] private float _radius;

        /// <summary>
        /// 検索対象のレイヤー．
        /// </summary>
        [SerializeField, Indent] private LayerMask _layer;

        // references
        private ITransform _transform;
        
        // 内部処理用
        private int _preInstanceId;
        private Subject<Collider> _onClosestTargetChangedSubject = new ();
        
        private static readonly Collider[] Results = new Collider[100];



        #region Property

        int IEarlyUpdateComponent.Order => Order.Check;

        /// <summary>
        /// 検索結果として見つかったコライダー．
        /// </summary>
        public Collider Target { get; private set; }

        /// <summary>
        /// 前フレームの <see cref="Target"/>．
        /// </summary>
        public Collider PreTarget { get; private set; }

        /// <summary>
        /// ターゲットが変化したときに呼び出されるイベント．
        /// </summary>
        public Observable<Collider> OnClosestTagetChanged => _onClosestTargetChangedSubject;

        #endregion
        

        #region Lifecycle Events

        protected override void OnComponentInitialized()
        {
            CharacterSettings.TryGetComponent(out _transform);
        }

        void IEarlyUpdateComponent.OnUpdate(float deltaTime)
        {
            var position = _transform.Position;

            // 前フレームのコライダー情報をキャッシュする．
            PreTarget = Target;

            // 範囲内のコライダーを収集し、最も近いものを選択する．
            var count = Physics.OverlapSphereNonAlloc(position, _radius, Results, _layer, QueryTriggerInteraction.Ignore);
            Target = ClosestCollider(position, Results, count);

            // InstanceID を比較してターゲットが変化した場合は UnityEvent を発火する．
            // コライダーが削除された場合を考慮し、InstanceID で判定する．
            var instanceId = Target != null ? Target.GetInstanceID() : 0;
            if (_preInstanceId != instanceId)
            {
                _onClosestTargetChangedSubject.OnNext(Target);
            }

            _preInstanceId = instanceId;
        }

        private void OnDestroy()
        {
            _onClosestTargetChangedSubject.Dispose();
            _onClosestTargetChangedSubject = null;
        }

        #endregion

        private Collider ClosestCollider(Vector3 position, Collider[] colliders, int count)
        {
            Collider closest = null;
            float minDistance = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var col = colliders[i];

                if (col.CompareTag(_tag) == false)
                    continue;

                var distance = Vector3.Distance(col.transform.position, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = col;
                }
            }

            return closest;
        }

#if UNITY_EDITOR
        private static readonly Color _gizmoColor = Color.yellow;

#if TCC_USE_NGIZMOS
        private void OnDrawGizmosSelected()
        {
            var center = transform.position;

            // 検索範囲の描画
            if (_radius > 0)
            {
                NGizmo.DrawWireSphere(center, _radius, _gizmoColor);
                Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.2f);
                Gizmos.DrawSphere(center, _radius);
            }

            if (Application.isPlaying == false || Target == null)
                return;

            // 検出中ターゲットの描画
            DrawOutline(new[] { Target.gameObject }, _gizmoColor);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Target.bounds.center, Target.bounds.size);
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.4f);
            Gizmos.DrawCube(Target.bounds.center, Target.bounds.size);
        }

        private void DrawOutline(in GameObject[] validGameObjects, Color color)
        {
            var sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            bool isSceneViewReady = sceneView != null
                                    && sceneView.camera != null
                                    && sceneView.camera.activeTexture != null;

            if (validGameObjects.Length > 0
                && Event.current.type == EventType.Repaint
                && isSceneViewReady)
            {
                UnityEditor.Handles.DrawOutline(validGameObjects, color, 0.4f);
            }
        }
#endif
#endif
    }
}