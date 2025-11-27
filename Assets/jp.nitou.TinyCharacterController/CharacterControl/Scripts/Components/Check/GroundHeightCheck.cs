using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Components;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Check
{
    [AddComponentMenu(MenuList.MenuCheck + nameof(GroundHeightCheck))]
    [DisallowMultipleComponent]
    public sealed class GroundHeightCheck : MonoBehaviour,
                                            IGroundContact
    {
        /// <summary>
        /// 地面の高さ
        /// </summary>
        public float GroundHeight;

        /// <summary>
        /// 接地したと判断される遊びの高さ。
        /// </summary>
        [SerializeField]
        private float _toleranceHeight = 0.2f;

        // references
        private CharacterSettings _characterSettings;
        private ITransform _transform;


        public bool IsOnGround => _transform.Position.y <= GroundHeight + _toleranceHeight;

        public bool IsFirmlyOnGround => _transform.Position.y <= GroundHeight;

        public float DistanceFromGround => _transform.Position.y - GroundHeight;

        Vector3 IGroundContact.GroundSurfaceNormal => Vector3.up;

        public Vector3 GroundContactPoint => new(_transform.Position.x, GroundHeight, _transform.Position.z);


        #region Lifecycle Events

        private void Awake()
        {
            GatherComponents();
        }

        #endregion

        private void GatherComponents()
                {
                    _characterSettings = GetComponentInParent<CharacterSettings>() ?? throw new System.NullReferenceException(nameof(_characterSettings));
        
                    // Components
                    _characterSettings.TryGetComponent(out _transform);
                }
        
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GetGizmosColor();

            // 地面を描画
            var position = transform.position;
            var groundPos = new Vector3(position.x, GroundHeight, position.z);
            Gizmos.DrawCube(groundPos, new Vector3(5, 0, 5));
            return;

            Color GetGizmosColor()
            {
                if (Application.isPlaying == false)
                    return Color.yellow;

                if (((IGroundContact)this).IsFirmlyOnGround)
                    return Color.red;
                if (((IGroundContact)this).IsOnGround)
                    return Color.magenta;
                return Color.yellow;
            }
        }
    }
}