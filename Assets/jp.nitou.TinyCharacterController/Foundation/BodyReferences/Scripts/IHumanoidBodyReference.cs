using UnityEngine;

namespace Nitou.TCC.Foundation.Humanoid
{
    /// <summary>
    /// Humanoidの参照を表すインタフェース．
    /// </summary>
    public interface IHumanoidBodyReference
    {
        Transform transform { get; }
    }
}