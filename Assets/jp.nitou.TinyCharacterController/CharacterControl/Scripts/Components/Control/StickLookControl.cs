using Nitou.TCC.CharacterControl.Core;
using UnityEngine;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Sirenix.OdinInspector;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// Update the character's orientation to the direction specified by <see cref="Look"/>.
    ///
    /// If <see cref="TurnPriority"/> is high, the character is turned in the direction of the stick movement.
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(StickLookControl))]
    [DisallowMultipleComponent]
    public sealed class StickLookControl : MonoBehaviour,
                                           ITurn
    {
        private CharacterSettings _settings;

        /// <summary>
        /// Rotation priority. When the priority is higher than the priority of other components,
        /// it turns in the direction specified by the stick.
        /// </summary>
        [GUIColor("green")]
        [SerializeField, Indent] private int _turnPriority;

        /// <summary>
        /// Speed to change orientation
        /// </summary>
        [Range(-1, 50)]
        [SerializeField, Indent] private int _turnSpeed;

        private float _yawAngle;

        /// <summary>
        /// 回転移動の優先度．
        /// </summary>
        public int TurnPriority
        {
            get => _turnPriority;
            set => _turnPriority = value;
        }

        int IPriority<ITurn>.Priority => TurnPriority;
        int ITurn.TurnSpeed => _turnSpeed;
        float ITurn.YawAngle => _yawAngle;

        
        private void Awake()
        {
            _settings = GetComponentInParent<CharacterSettings>();
        }

        /// <summary>
        /// スティックで指定した方向を向く．方向はカメラの向きを基準に補正される．
        /// </summary>
        /// <param name="rightStick">スクリーン空間で X が左右、Y が上下に対応する入力値</param>
        public void Look(Vector2 rightStick)
        {
            var rotation = Quaternion.AngleAxis(_settings.CameraTransform.rotation.eulerAngles.y, Vector3.up);
            var target = Quaternion.LookRotation(new Vector3(rightStick.x, 0, rightStick.y), Vector3.up);
            _yawAngle = (rotation * target).eulerAngles.y;
        }
    }
}