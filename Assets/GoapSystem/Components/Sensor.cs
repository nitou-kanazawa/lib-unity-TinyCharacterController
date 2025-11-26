using System;
using UnityEngine;
using R3;

namespace Nitou.Goap.Components
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class Sensor : MonoBehaviour
    {
        [SerializeField] private float _detectionRadius = 5f;
        [SerializeField] private float _timeInterval = 1f;

        private SphereCollider _collider;
        private GameObject _target;
        private Vector3 _lastKnownPosition;

        private readonly Subject<Vector3> _onTargetChanged = new();

        public Observable<Vector3> OnTargetChanged => _onTargetChanged;
        public Vector3 TargetPosition => _target != null ? _target.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;


        #region Lifecycle Events

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            _collider.radius = _detectionRadius;
        }

        private void Start()
        {
            _onTargetChanged.AddTo(this);

            // R3のObservable.Intervalを使って定期的にターゲット位置を更新
            Observable.Interval(TimeSpan.FromSeconds(_timeInterval))
                      .Subscribe(_ => UpdateTargetPosition(_target))
                      .AddTo(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition();
        }

        private void OnDrawGizmos()
        {
            if (_collider == null) return;

            Gizmos.color = IsTargetInRange ? Color.red : Color.greenYellow;
            Gizmos.DrawWireSphere(_collider.center, _detectionRadius);
        }

        private void OnValidate()
        {
            if (_collider == null)
                _collider = GetComponent<SphereCollider>();
        }

        #endregion


        /// <summary>
        /// ターゲット位置を更新し、変更があればイベントを発行
        /// </summary>
        /// <param name="target">新しいターゲット（nullの場合はターゲットをクリア）</param>
        private void UpdateTargetPosition(GameObject target = null)
        {
            _target = target;
            if (IsTargetInRange && (_lastKnownPosition != TargetPosition || _lastKnownPosition != Vector3.zero))
            {
                _lastKnownPosition = TargetPosition;
                _onTargetChanged.OnNext(TargetPosition);
            }
        }
    }
}