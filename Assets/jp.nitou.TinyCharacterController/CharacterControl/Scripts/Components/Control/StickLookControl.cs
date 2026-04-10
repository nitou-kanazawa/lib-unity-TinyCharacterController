using Nitou.TCC.CharacterControl.Core;
using UnityEngine;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Sirenix.OdinInspector;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// <see cref="Look"/> で指定された方向にキャラクターの向きを更新するコンポーネント．
    ///
    /// <see cref="TurnPriority"/> が高い場合、スティックの入力方向にキャラクターを回転させる．
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(StickLookControl))]
    [DisallowMultipleComponent]
    public sealed class StickLookControl : MonoBehaviour,
                                           ITurn
    {
        private CharacterSettings _settings;

        /// <summary>
        /// 回転の優先度．他のコンポーネントより優先度が高い場合、スティックで指定した方向を向く．
        /// </summary>
        [GUIColor("green")]
        [SerializeField, Indent] private int _turnPriority;

        /// <summary>
        /// 向きを変える速度．
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