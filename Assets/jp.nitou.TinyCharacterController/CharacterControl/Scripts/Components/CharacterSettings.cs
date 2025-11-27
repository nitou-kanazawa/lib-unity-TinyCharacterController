using System.Collections.Generic;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;
using Sirenix.OdinInspector;
using Nitou.TCC.Foundation;
using UnityEngine.Pool;

namespace Nitou.TCC.CharacterControl.Core
{
    /// <summary>
    /// 
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu(MenuList.MenuBrain + nameof(CharacterSettings))]
    public sealed class CharacterSettings : MonoBehaviour
    {
        [Title("Body Settings")]
        [SerializeField, Indent] private float _mass = 1;

        [SerializeField, Indent] private float _height = 1.4f;
        [SerializeField, Indent] private float _radius = 0.5f;
        [SerializeField, Indent] private MovementReference _movementReference;

        [Title("Hierarchy")]
        [SerializeField, Indent] private Transform _checkParent;

        [SerializeField, Indent] private Transform _effectParent;
        [SerializeField, Indent] private Transform _controlParent;

        [Title("Environment Settings")]
        [SerializeField, Indent] private LayerMask _environmentLayer;

        // Camera
        private Camera _camera;
        private Transform _cameraTransform;

        // GameObject配下のコライダーリスト
        private readonly List<Collider> _hierarchyColliders = new();

        // 定数
        private const float MIN_HEIGHT = 0.1f;
        private const float MIN_RADIUS = 0.1f;
        private const float MIN_MASS = 0.001f;


        // ----------------------------------------------------------------------------

        #region Properties

        /// <summary>
        /// キャラクターの半径．
        /// </summary>
        public float Radius
        {
            get => _radius;
            set
            {
                var newValue = Mathf.Max(value, MIN_RADIUS);
                if (Mathf.Approximately(_radius, newValue))
                    return;

                _radius = newValue;
            }
        }

        /// <summary>
        /// キャラクターの身長．
        /// </summary>
        public float Height
        {
            get => _height;
            set { _height = Mathf.Max(value, MIN_HEIGHT); }
        }

        /// <summary>
        /// キャラクターの体重．
        /// </summary>
        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        /// <summary>
        /// 移動の基準座標系設定．
        /// </summary>
        public MovementReference MovementReference => _movementReference;

        /// <summary>
        /// Layer for recognizing terrain colliders.
        /// </summary>
        public LayerMask EnvironmentLayer => _environmentLayer;

        public Transform CheckParent => _checkParent;
        public Transform EffectParent => _effectParent;
        public Transform ControlParent => _controlParent;

        /// <summary>
        /// カメラが設定されているかどうか．
        /// </summary>
        public bool HasCamera => _camera != null;

        /// <summary>
        ///     Gets character's camera information.
        ///     Uses Camera.Main if no camera is set.
        /// </summary>
        public Camera CameraMain
        {
            get
            {
                // Return the cached camera if it's already registered.
                if (_camera != null)
                    return _camera;

                ApplyMainCameraTransform();
                return _camera;
            }

            set
            {
                // Update the camera and _cameraTransform.
                _camera = value;
                _cameraTransform = _camera != null ? _camera.transform : null;
            }
        }

        /// <summary>
        ///     MainCamera's Transform.
        /// </summary>
        public Transform CameraTransform
        {
            get
            {
                // Get the camera's Transform if already registered.
                if (_cameraTransform != null)
                    return _cameraTransform;

                ApplyMainCameraTransform();

                return _cameraTransform;
            }
        }

        #endregion


        // ----------------------------------------------------------------------------
        #region Lifecycle Events

        private void Awake()
        {
            // Get a list of colliders.
            GatherOwnColliders();

            // Update the camera's Transform.
            ApplyMainCameraTransform();
        }

        /// <summary>
        ///     Callback when the component's values change.
        /// </summary>
        private void OnValidate()
        {
            // Ensure values don't go below the minimum.
            _height = Mathf.Max(MIN_HEIGHT, _height);
            _radius = Mathf.Max(MIN_RADIUS, _radius);
            _mass = Mathf.Max(MIN_MASS, _mass);

            UpdateSettings();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _environmentLayer = LayerMaskUtil.OnlyDefault();
        }
#endif
        #endregion

        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// 対象コライダーがボディ配下のものか確認する．
        /// </summary>
        public bool IsOwnCollider(Collider collider)
        {
            return _hierarchyColliders.Contains(collider);
        }

        /// <summary>
        /// Retrieves the closest RaycastHit excluding the character's own colliders.
        /// </summary>
        public bool ClosestHit(RaycastHit[] hits, int count, float maxDistance, out RaycastHit closestHit)
        {
            var min = maxDistance;
            closestHit = default;
            var isHit = false;

            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];

                // Skip if the current Raycast's distance is greater than the current minimum,
                // or if it belongs to the character's collider list, or if it's null.
                if (hit.distance > min || IsOwnCollider(hit.collider) || hit.collider == null)
                    continue;

                // Update the closest Raycast.
                min = hit.distance;
                closestHit = hit;

                // Set to true if at least one closest Raycast is found.
                isHit = true;
            }

            return isHit;
        }


        // ----------------------------------------------------------------------------
        // Private Method

        /// <summary>
        /// 自身のボディ配下コライダー情報を更新する．
        /// </summary>
        private void GatherOwnColliders()
        {
            _hierarchyColliders.Clear();
            _hierarchyColliders.AddRange(GetComponentsInChildren<Collider>());
        }

        /// <summary>
        ///     Updates <see cref="Camera.main" /> settings for <see cref="_camera" /> and <see cref="_cameraTransform" />.
        /// </summary>
        private void ApplyMainCameraTransform()
        {
            // Get objects with the MainCamera tag.
            _camera = Camera.main;

            // Update the CameraTransform if a camera is acquired.
            if (_camera != null && _cameraTransform == null)
            {
                _cameraTransform = _camera.transform;
            }
        }

        /// <summary>
        ///     Updates components with <see cref="IActorSettingUpdateReceiver" />.
        /// </summary>
        private void UpdateSettings()
        {
            var controls = ListPool<IActorSettingUpdateReceiver>.Get();
            {
                // �X�V
                GetComponents(controls);
                controls.ForEach(c => c.OnUpdateSettings(this));
            }
            ListPool<IActorSettingUpdateReceiver>.Release(controls);
        }
    }


    public enum CharacterComponent
    {
        Brain,
        Check,
        Effect,
        Control,
        Others,
    }

    public static class CharacterSettingsExtensions
    {
  
        
    }
    
    
    public static class CharacterComponentUtils
    {
        /// <summary>
        /// Actorコンポーネントを設定したGameObjectから取得する
        /// </summary>
        public static T GetActorComponent<T>(this CharacterSettings settings, CharacterComponent type)
        {
            // コンポーネント
            Transform holder = type switch
            {
                CharacterComponent.Check => settings.CheckParent,
                CharacterComponent.Effect => settings.EffectParent,
                CharacterComponent.Control => settings.ControlParent,
                _ => settings.transform
            };
            if (holder == null) holder = settings.transform;

            return holder.GetComponent<T>();
        }

        /// <summary>
        /// Actorコンポーネントを設定したGameObjectから取得する
        /// </summary>
        public static bool TryGetActorComponent<T>(this CharacterSettings settings, CharacterComponent type, out T component)
        {
            // コンポーネント
            Transform holder = type switch
            {
                CharacterComponent.Check => settings.CheckParent,
                CharacterComponent.Effect => settings.EffectParent,
                CharacterComponent.Control => settings.ControlParent,
                _ => settings.transform
            };
            if (holder == null) holder = settings.transform;

            return holder.TryGetComponent(out component);
        }
    }
}