using UnityEngine;

namespace Nitou.TCC.Utils.Humanoid
{
    /// <summary>
    /// Humanoidの身体への参照を示すコンポーネント．
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class BodyReferenceBase : MonoBehaviour, IHumanoidBodyReference
    {
        [SerializeField] private Vector3 _offset;
    }
}