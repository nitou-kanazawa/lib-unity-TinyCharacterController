using System.Collections.Generic;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// キャラクターが所持するアビリティおよびゲームプレイ効果を管理するシステムです。
    /// </summary>
    public interface IAbilitySystem : IGameplayTagProvider
    {
        /// <summary>
        /// このアビリティシステムに紐づく属性システムを取得します。
        /// アビリティや効果が属性を参照する際に使用されます。
        /// </summary>
        IAttributeSystem AttributeSystem { get; }

        /// <summary>
        /// このアビリティシステムに紐づくゲームプレイ効果システムを取得します。
        /// アビリティが効果を適用する際に使用されます。
        /// </summary>
        IGameplayEffectSystem GameplayEffectSystem { get; }

        /// <summary>
        /// このシステムに付与されている全アビリティの一覧を取得します。
        /// </summary>
        IReadOnlyList<IAbilitySpec> GrantedAbilities { get; }

        /// <summary>
        /// このアビリティシステムのレベルを取得または設定します。
        /// アビリティや効果のスケーリングに利用されます。
        /// </summary>
        float Level { get; set; }

        /// <summary>
        /// 指定したアビリティスペックをこのシステムに付与します。
        /// </summary>
        /// <param name="spec">付与するアビリティスペック。</param>
        void GrantAbility(IAbilitySpec spec);

        /// <summary>
        /// 指定したアビリティスペックをこのシステムから取り除きます。
        /// </summary>
        /// <param name="spec">取り除くアビリティスペック。</param>
        void RevokeAbility(IAbilitySpec spec);

        /// <summary>
        /// 指定したタグを持つ全てのアビリティスペックをこのシステムから取り除きます。
        /// </summary>
        /// <param name="tag">削除条件となるタグ。</param>
        void RevokeAbilitiesWithTag(IGameplayTag tag);

        /// <summary>
        /// 指定したアビリティスペックの起動を試みます。
        /// 起動条件を満たさない場合は何も行いません。
        /// </summary>
        /// <param name="spec">起動を試みるアビリティスペック。</param>
        /// <returns>起動に成功した場合は true、それ以外は false。</returns>
        bool TryActivateAbility(IAbilitySpec spec);
    }
}


