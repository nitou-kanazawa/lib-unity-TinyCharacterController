namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイタグの必須・無視要件を表す構造体です。
    /// Core レイヤの構造体として、抽象インターフェースを使用します。
    /// </summary>
    public struct GameplayTagRequireIgnoreContainer
    {
        /// <summary>
        /// 必須タグの配列。全てのタグが存在する必要があります。
        /// </summary>
        public IGameplayTag[] RequireTags;

        /// <summary>
        /// 無視タグの配列。いずれのタグも存在してはいけません。
        /// </summary>
        public IGameplayTag[] IgnoreTags;
    }
}

