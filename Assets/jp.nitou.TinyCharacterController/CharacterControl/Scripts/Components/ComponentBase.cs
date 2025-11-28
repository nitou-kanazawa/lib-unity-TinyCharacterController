using UnityEngine;
using Nitou.TCC.CharacterControl.Core;
using Nitou.TCC.CharacterControl.Interfaces.Core;

namespace Nitou.TCC.CharacterControl
{
    /// <summary>
    /// <see cref="CharacterSettings"/> へのアクセスを必要とするすべての TCC コンポーネントの基底クラス．
    /// 初期化時に自動的なコンポーネント収集と検証を提供する．
    /// </summary>
    /// <remarks>
    /// このクラスは、親階層から CharacterSettings を取得する共通パターンを集約することで、
    /// Check、Control、Effect コンポーネント間のコード重複を排除する．
    /// 派生クラスは <see cref="OnComponentInitialized"/> をオーバーライドして追加のコンポーネントを取得できる．
    /// </remarks>
    public abstract class ComponentBase : MonoBehaviour
    {
        /// <summary>
        /// 親階層の CharacterSettings への参照．
        /// Awake 時に自動的に設定される．初期化後は非 null が保証される．
        /// </summary>
        protected CharacterSettings CharacterSettings { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected ITransform Transform { get; private set; }

        
        // ----------------------------------------------------------------------------
        // Lifecycle Events

        /// <summary>
        /// 必要な参照を収集してコンポーネントを初期化する．
        /// 派生クラスでこのメソッドをオーバーライドする場合は、必ず base.Awake() を呼び出すこと．
        /// </summary>
        protected virtual void Awake()
        {
            InitializeComponent();
        }

        

        // ----------------------------------------------------------------------------
        // Private Methods

        /// <summary>
        /// 親階層から CharacterSettings を収集し、その存在を検証する．
        /// 初期化が成功した後に <see cref="OnComponentInitialized"/> を呼び出す．
        /// </summary>
        /// <exception cref="System.NullReferenceException">
        /// 親階層に CharacterSettings が見つからない場合にスローされる．
        /// </exception>
        private void InitializeComponent()
        {
            GatherCharacterSettings();

            if (CharacterSettings == null)
            {
                throw new System.NullReferenceException(
                    $"{GetType().Name} は親階層に {nameof(CharacterSettings)} コンポーネントを必要とします． " +
                    $"このコンポーネントが {nameof(CharacterSettings)} を持つ GameObject の子である GameObject にアタッチされていることを確認してください．"
                );
            }

            Transform = CharacterSettings.GetComponent<ITransform>();
            OnComponentInitialized();
        }

        protected void GatherCharacterSettings()
        {
            CharacterSettings = GetComponentInParent<CharacterSettings>();
        }
        
        /// <summary>
        /// CharacterSettings の初期化が成功した後に呼び出される．
        /// 派生クラス固有の追加のコンポーネント参照を収集するために、このメソッドをオーバーライドする．
        /// </summary>
        /// <example>
        /// <code>
        /// protected override void OnComponentInitialized()
        /// {
        ///     CharacterSettings.TryGetComponent(out _transform);
        ///     CharacterSettings.TryGetActorComponent(CharacterComponent.Check, out _groundCheck);
        /// }
        /// </code>
        /// </example>
        protected virtual void OnComponentInitialized()
        {
            // 派生クラスで追加のコンポーネントを収集するためにオーバーライドする
        }
    }
}
