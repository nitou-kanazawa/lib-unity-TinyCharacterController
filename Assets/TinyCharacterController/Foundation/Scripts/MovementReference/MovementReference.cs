using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public enum MovementReferenceMode
    {
        /// <summary>ワールド座標系．</summary>
        World,

        /// <summary>外部座標系．</summary>
        External,

        /// <summary>キャラクター座標系．</summary>
        Actor,
    }


    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [DisallowMultipleComponent]
    public sealed class MovementReference : MonoBehaviour, IMovementInputModifier
    {
        [TitleGroup("Reference Mode")] [EnumToggleButtons, HideLabel] [SerializeField, Indent]
        private MovementReferenceMode _mode = MovementReferenceMode.World;

        [Space] [TitleGroup("Reference Mode")] [ShowIf("_mode", MovementReferenceMode.External)] [SerializeField, Indent]
        private Transform _externalReference = null;


        // ----------------------------------------------------------------------------

        #region Properity

        /// <summary>
        /// �ړ����͂̊�ƂȂ���W�n�D
        /// </summary>
        public MovementReferenceMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        /// <summary>
        /// "External"���[�h�̎��ɎQ�Ƃ�����W�n�D
        /// </summary>
        public Transform ExternalReference
        {
            get => _externalReference;
            set => _externalReference = value;
        }

        /// <summary>
        /// ����W�n�̐��ʃx�N�g��.
        /// </summary>
        public Vector3 MovementReferenceForward { get; private set; }

        /// <summary>
        /// ����W�n�̉E�x�N�g���D
        /// </summary>
        public Vector3 MovementReferenceRight { get; private set; }

        /// <summary>
        /// ����W�n�i�J�������j�ɕϊ����ꂽ���̓x�N�g��.
        /// </summary>
        public Vector3 ModifieredInputVector { get; private set; }

        #endregion


        // ----------------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// �Q�ƃ��[�h��ݒ肷��D
        /// </summary>
        public void SetMode(MovementReferenceMode mode)
        {
            _mode = mode;

            ResetInputData();
            UpdateMovementReferenceData();
        }

        /// <summary>
        /// ���͒l�̍X�V�D
        /// </summary>
        public void UpdateInputData(Vector2 movementInput)
        {
            // ���W�n�̍X�V
            UpdateMovementReferenceData();

            // ���͒l�̍X�V
            Vector3 inputMovementReference =
                (MovementReferenceRight * movementInput.x) +
                (MovementReferenceForward * movementInput.y);
            ModifieredInputVector = Vector3.ClampMagnitude(inputMovementReference, 1f);
        }

        /// <summary>
        /// ���͒l�̃��Z�b�g�D
        /// </summary>
        public void ResetInputData()
        {
            ModifieredInputVector = Vector3.zero;
        }

        #endregion


        // Private Method 

        /// <summary>
        /// ����W�n�̍X�V�D
        /// </summary>
        private void UpdateMovementReferenceData()
        {
            // Forward
            switch (Mode)
            {
                case MovementReferenceMode.World: // ----- �O���[�o�����W�n�

                    MovementReferenceForward = Vector3.forward;
                    MovementReferenceRight = Vector3.right;
                    break;

                case MovementReferenceMode.Actor: // ----- �L�������ʊ

                    MovementReferenceForward = transform.forward;
                    MovementReferenceRight = transform.right;
                    break;

                case MovementReferenceMode.External: // ---- �C�ӂ̍��W�n�

                    if (ExternalReference != null)
                    {
                        MovementReferenceForward = Vector3.Normalize(Vector3.ProjectOnPlane(ExternalReference.forward, transform.up));
                        MovementReferenceRight = Vector3.Normalize(Vector3.ProjectOnPlane(ExternalReference.right, transform.up));
                    }
                    else if (Application.isPlaying)
                    {
                        Debug.LogWarning("the external reference is null! assign a Transform.");
                    }

                    break;
            }
        }


#if UNITY_EDITOR && USE_NITOU_GIZMOS
        [Title("Debug")] [SerializeField, Indent]
        private Vector3 _offset = Vector3.up * 0.01f;

        [SerializeField, Indent] private float _radius = 1.5f;
        private void OnValidate() => UpdateMovementReferenceData();
        private void OnDrawGizmos()
        {
            var pos = transform.position + _offset;

            // Circle
            Gizmos_.DrawWireCircle(pos, _radius * 0.4f, Colors.Gray);
            Gizmos_.DrawWireCircle(pos, _radius * 0.8f, Colors.Gray);

            // Vectors
            Gizmos_.DrawRayArrow(pos, MovementReferenceForward * _radius, Colors.DeepSkyBlue);
            Gizmos_.DrawRayArrow(pos, MovementReferenceRight * _radius, Colors.Orangered);
            Gizmos_.DrawRay(pos, ModifieredInputVector * _radius, Colors.WhiteSmoke);
        }
#endif
    }


    /// <summary>
    /// MovementReference�ɑ΂���ėp���C�u����
    /// </summary>
    public static class MovementReferenceExtensions
    {
        /// <summary>
        /// ����W�n�Ƃ��ăJ������Transform��ݒ肷��D
        /// </summary>
        public static void SetCameraTransform(this MovementReference self, Camera camera)
        {
            if (camera == null)
            {
                Debug.LogWarning("active camera not found.");
                return;
            }

            self.Mode = MovementReferenceMode.External;
            self.ExternalReference = camera.transform;
        }

        /// <summary>
        /// ����W�n�Ƃ��ăJ������Transform��ݒ肷��D
        /// </summary>
        public static void SetCameraTransform(this MovementReference self)
        {
            self.SetCameraTransform(Camera.main);
        }
    }
}