using Nitou.TCC.CharacterControl.Interfaces.Core;
using Nitou.TCC.CharacterControl.Shared;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Control
{
    /// <summary>
    /// <see cref="Target"/> の方向を向くコンポーネント．
    ///
    /// 他のコンポーネントより優先度が高い場合、キャラクターは Target の方向を向く．
    /// Target が設定されていない場合、このコンポーネントの <see cref="Priority"/> は 0 になる．
    /// </summary>
    [AddComponentMenu(MenuList.MenuControl + nameof(LookTargetControl))]
    [DisallowMultipleComponent]
    public sealed class LookTargetControl : MonoBehaviour,
                                            ITurn
    {
        /// <summary>
        /// 向く対象の Transform．
        /// null の場合、優先度は無効になる．
        /// </summary>
        // [AllowsNull]
        public Transform Target;

        /// <summary>
        /// 回転の優先度．
        /// </summary>
        [Header("Turn Priority and Speed")]
        public int Priority = 10;

        /// <summary>
        /// 向きを変える速度．
        /// -1 の場合は即座に対象の方向を向く．
        /// </summary>
        [SerializeField, Range(-1, 30)]
        public int TurnSpeed = 15;

        int IPriority<ITurn>.Priority => Target != null ? Priority : 0;

        int ITurn.TurnSpeed => TurnSpeed;

        float ITurn.YawAngle
        {
            get
            {
                if (Target == null)
                    return 0;

                var delta = Target.position - transform.position;

                return (delta.sqrMagnitude > 0) ? Vector3.SignedAngle(Vector3.forward, delta, Vector3.up) : transform.rotation.eulerAngles.y;
            }
        }
    }
}