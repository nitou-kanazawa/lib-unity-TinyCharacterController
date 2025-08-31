using UnityEngine;

namespace Nitou.TCC.Utils.Humanoid
{
    /// <summary>
    /// Humanoidの参照を表すインタフェース．
    /// </summary>
    public interface IHumanoidBodyReference
    {
        Transform transform { get; }
    }
}