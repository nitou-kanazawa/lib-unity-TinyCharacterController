using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// 移動方向と向きを直接設定するコンポーネント．
    /// 設定された値を使用してキャラクターを移動・回転させる．
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(ManualControl))]
    [DisallowMultipleComponent]
    public sealed class ManualControl : MonoBehaviour,
                                        ITurn,
                                        IMove
    {
        /// <summary>
        /// 移動の優先度．
        /// 他のコンポーネントより優先度が高い場合、<see cref="MoveVelocity"/> の値でキャラクターが移動する．
        /// </summary>
        [Title("Move Settings")]
        [SerializeField, Indent] public int MovePriority;

        /// <summary>
        /// キャラクターの移動ベクトル．
        /// </summary>
        [SerializeField, Indent] public Vector3 MoveVelocity;

        /// <summary>
        /// 回転の優先度．
        /// 他のコンポーネントより優先度が高い場合、<see cref="TurnAngle"/> の方向にキャラクターが向く．
        /// </summary>
        [Title("Turn Settings")]
        [SerializeField, Indent] public int TurnPriority;

        /// <summary>
        /// 向きを変える速度．0〜30 が基本で、負の値は補間なしで即座に向きを変える．
        /// </summary>
        [Range(-1, 50)]
        [SerializeField, Indent] public int TurnSpeed = 35;

        /// <summary>
        /// キャラクターが向く方向（ワールド座標）．
        /// </summary>
        [SerializeField, Indent] private float _turnAngle;

        private Quaternion _yawRotation;

        #region Property

        /// <summary>
        /// キャラクターが向く方向（ワールド座標）．
        /// </summary>
        public float TurnAngle
        {
            get => _turnAngle;
            set
            {
                _turnAngle = value;
                _yawRotation = Quaternion.AngleAxis(_turnAngle, Vector3.up);
            }
        }

        /// <summary>
        /// キャラクターが向く方向（ワールド座標・Y軸無視）．
        /// </summary>
        public Vector3 TurnDirection
        {
            get => _yawRotation * Vector3.forward;
            set
            {
                _yawRotation = Quaternion.LookRotation(value, Vector3.up);
                _turnAngle = Vector3.SignedAngle(Vector3.forward, value, Vector3.up);
            }
        }

        /// <summary>
        /// キャラクターの回転（Y軸無視）．
        /// </summary>
        public Quaternion TurnRotation
        {
            get => _yawRotation;
            set
            {
                _yawRotation = value;
                _turnAngle = Vector3.SignedAngle(Vector3.forward, value * Vector3.forward, Vector3.up);
            }
        }

        int IPriority<ITurn>.Priority => TurnPriority;

        int ITurn.TurnSpeed => TurnSpeed;

        float ITurn.YawAngle => _turnAngle;

        int IPriority<IMove>.Priority => MovePriority;

        Vector3 IMove.MoveVelocity => MoveVelocity;

        #endregion


        private void OnValidate()
        {
            MovePriority = Mathf.Max(0, MovePriority);
            TurnPriority = Mathf.Max(0, TurnPriority);
            TurnSpeed = Mathf.Max(-1, TurnSpeed);

            TurnAngle = _turnAngle;
        }
    }
}