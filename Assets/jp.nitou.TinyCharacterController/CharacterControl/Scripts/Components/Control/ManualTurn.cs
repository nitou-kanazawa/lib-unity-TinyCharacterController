using System;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Nitou.TCC.Foundation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nitou.TCC.CharacterControl.Components
{
    /// <summary>
    /// キャラクターの向きを手動で設定するコンポーネント．
    /// ※<see cref="ManualControl"/>の使用を推奨．
    /// </summary>
    [Obsolete("Recommends Manual Control")]
    [AddComponentMenu(MenuList.MenuControl + nameof(ManualTurn))]
    // [RequireComponent(typeof(CharacterSettings))]
    [DisallowMultipleComponent]
    public class ManualTurn : MonoBehaviour, ITurn
    {
        [SerializeField]
        private Vector3 _direction = Vector3.forward;

        [FormerlySerializedAs("_turnPriority")]
        [Space]
        [Header("キャラクターの向きの設定")]
        [SerializeField]
        public int TurnPriority;

        [ Range(-1, 50)]
        [FormerlySerializedAs("_turnSpeed")]
        public int TurnSpeed = 30;

        int IPriority<ITurn>.Priority => (_direction == Vector3.zero) ? 0 : TurnPriority;
        int ITurn.TurnSpeed => TurnSpeed;

        public void SetRotation(Quaternion rotation)
        {
            _direction = rotation * Vector3.forward;
            _direction.y = 0;
        }

        public void SetDirection(Vector3 dir)
        {
            _direction = dir;
            _direction.y = 0;
        }

        float ITurn.YawAngle => Vector3.SignedAngle(Vector3.forward, _direction, Vector3.up);
    }
}
