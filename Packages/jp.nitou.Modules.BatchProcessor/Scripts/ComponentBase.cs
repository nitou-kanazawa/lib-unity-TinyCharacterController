using UnityEngine;

namespace Nitou.BatchProcessor
{
    /// <summary>
    /// バッチ処理対象のコンポーネント基底クラス．
    /// Register this class with a class that inherits from <see cref="SystemBase{TComponent, TSystem}"/> for usage.
    /// </summary>
    public abstract class ComponentBase : MonoBehaviour, IComponentIndex
    {
        protected int Index { get; private set; } = -1;
        protected bool IsRegistered => Index != -1;


        /// <inheritdoc/>
        int IComponentIndex.Index
        {
            get => Index;
            set => Index = value;
        }

        /// <inheritdoc/>
        bool IComponentIndex.IsRegistered => IsRegistered;
    }
}
