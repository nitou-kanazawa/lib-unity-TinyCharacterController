using System;
using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// キャラクターの向きを手動で設定するコンポーネント．
    /// ※<see cref="ManualControl"/>の使用を推奨．
    /// </summary>
    [Obsolete("Recommends Manual Control")]
    [AddComponentMenu(MenuList.MenuControl + nameof(ManualTurn))]
    [DisallowMultipleComponent]
    public sealed class ManualTurn : ComponentBase,
                                     ITurn
    {
        [SerializeField]
        private Vector3 _direction = Vector3.forward;

        [Space]
        [Header("キャラクターの向きの設定")]
        [SerializeField] public int TurnPriority;

        [Range(-1, 50)]
        public int TurnSpeed = 30;

        int IPriority<ITurn>.Priority => (_direction == Vector3.zero) ? 0 : TurnPriority;
        int ITurn.TurnSpeed => TurnSpeed;

        float ITurn.YawAngle => Vector3.SignedAngle(Vector3.forward, _direction, Vector3.up);

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
    }
}