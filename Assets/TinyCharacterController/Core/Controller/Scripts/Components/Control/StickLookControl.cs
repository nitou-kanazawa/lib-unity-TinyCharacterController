using Nitou.TCC.Controller.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Nitou.TCC.Controller.Interfaces.Core;
using Nitou.TCC.Controller.Shared;
using Sirenix.OdinInspector;

namespace Nitou.TCC.Controller.Components
{
    /// <summary>
    /// Update the character's orientation to the direction specified by <see cref="Look"/>.
    ///
    /// If <see cref="TurnPriority"/> is high, the character is turned in the direction of the stick movement.
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(StickLookControl))]
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
        [SerializeField, Indent] [Range(-1, 50)]
        private int _turnSpeed;

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
        /// Turns in the direction specified by the stick. The direction is compensated by the camera orientation.
        /// </summary>
        /// <param name="rightStick">X faces left and right on the screen space, Y faces up and down on the screen space</param>.
        public void Look(Vector2 rightStick)
        {
            var rotation = Quaternion.AngleAxis(_settings.CameraTransform.rotation.eulerAngles.y, Vector3.up);
            var target = Quaternion.LookRotation(new Vector3(rightStick.x, 0, rightStick.y), Vector3.up);
            _yawAngle = (rotation * target).eulerAngles.y;
        }
    }
}