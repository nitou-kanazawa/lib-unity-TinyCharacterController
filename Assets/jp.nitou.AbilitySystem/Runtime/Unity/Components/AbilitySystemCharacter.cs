using System.Collections.Generic;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Unity.Components
{
    /// <summary>
    /// シーン上の 1 体のキャラクターを表すコンポーネントです。
    /// 内部に <see cref="AbilitySystemCore"/> と <see cref="GameplayEffectSystemCore"/> を保持し、
    /// Core への薄いラッパとして機能します。
    /// </summary>
    public class AbilitySystemCharacter : MonoBehaviour
    {
        [SerializeField] protected AttributeSystemComponent _attributeSystem;

        private AbilitySystemCore _abilitySystem;
        private GameplayEffectSystemCore _effectSystem;

        [SerializeField] private float _level = 1f;

        /// <summary>
        /// Core の IAbilitySystem を取得します。Awake() 後に利用可能です。
        /// </summary>
        public IAbilitySystem AbilitySystem => _abilitySystem;

        /// <summary>
        /// このアビリティシステムのレベルを取得または設定します。
        /// </summary>
        public float Level
        {
            get => _abilitySystem?.Level ?? _level;
            set
            {
                _level = value;
                if (_abilitySystem != null)
                {
                    _abilitySystem.Level = value;
                }
            }
        }

        /// <summary>
        /// 属性システムコンポーネントを取得または設定します。
        /// </summary>
        public AttributeSystemComponent AttributeSystem
        {
            get => _attributeSystem;
            set => _attributeSystem = value;
        }

        private void Awake()
        {
            if (_attributeSystem != null)
            {
                _effectSystem = new GameplayEffectSystemCore(_attributeSystem.Core);
                _abilitySystem = new AbilitySystemCore(_effectSystem, _attributeSystem.Core)
                {
                    Level = _level
                };
            }
        }

        /// <summary>
        /// 指定したアビリティ定義からアビリティスペックを生成し、このシステムに付与します。
        /// </summary>
        /// <param name="abilityDefinition">付与するアビリティ定義。</param>
        public void GrantAbility(IAbilityDefinition abilityDefinition)
        {
            if (_abilitySystem == null || abilityDefinition == null) return;
            var spec = abilityDefinition.CreateSpec(_abilitySystem);
            _abilitySystem.GrantAbility(spec);
        }

        /// <summary>
        /// 指定したアビリティスペックをこのシステムに付与します。
        /// </summary>
        /// <param name="spec">付与するアビリティスペック。</param>
        public void GrantAbility(IAbilitySpec spec)
        {
            if (_abilitySystem == null || spec == null) return;
            _abilitySystem.GrantAbility(spec);
        }

        /// <summary>
        /// 指定したアビリティスペックをこのシステムから取り除きます。
        /// </summary>
        /// <param name="spec">取り除くアビリティスペック。</param>
        public void RevokeAbility(IAbilitySpec spec)
        {
            if (_abilitySystem == null || spec == null) return;
            _abilitySystem.RevokeAbility(spec);
        }

        /// <summary>
        /// 指定したタグを持つ全てのアビリティスペックをこのシステムから取り除きます。
        /// </summary>
        /// <param name="tag">削除条件となるタグ。</param>
        public void RevokeAbilitiesWithTag(IGameplayTag tag)
        {
            if (_abilitySystem == null || tag == null) return;
            _abilitySystem.RevokeAbilitiesWithTag(tag);
        }

        /// <summary>
        /// 指定したアビリティスペックの起動を試みます。
        /// </summary>
        /// <param name="spec">起動を試みるアビリティスペック。</param>
        /// <returns>起動に成功した場合は true、それ以外は false。</returns>
        public bool TryActivateAbility(IAbilitySpec spec)
        {
            if (_abilitySystem == null || spec == null) return false;
            if (!_abilitySystem.TryActivateAbility(spec)) return false;

            // 実際のコルーチン起動は Unity 側で行う
            StartCoroutine(spec.Activate());
            return true;
        }

        /// <summary>
        /// 指定したゲームプレイ効果スペックをこのシステムに適用します。
        /// </summary>
        /// <param name="spec">適用するゲームプレイ効果スペック。</param>
        public void ApplyGameplayEffect(IGameplayEffectSpec spec)
        {
            if (_effectSystem == null || spec == null) return;
            _effectSystem.ApplyEffect(spec);
        }

        /// <summary>
        /// 指定したゲームプレイ効果アセットからスペックを生成し、このシステムに適用します。
        /// </summary>
        /// <param name="gameplayEffect">適用するゲームプレイ効果アセット。</param>
        /// <param name="level">効果のレベル（省略時はシステムのレベルを使用）。</param>
        public void ApplyGameplayEffect(GameplayEffectAsset gameplayEffect, float? level = null)
        {
            if (_abilitySystem == null || gameplayEffect == null) return;
            var spec = MakeOutgoingSpec(gameplayEffect, level);
            ApplyGameplayEffect(spec);
        }

        /// <summary>
        /// 指定したゲームプレイ効果アセットからスペックを生成します。
        /// </summary>
        /// <param name="gameplayEffect">ゲームプレイ効果アセット。</param>
        /// <param name="level">効果のレベル（省略時はシステムのレベルを使用）。</param>
        /// <returns>生成されたスペック。</returns>
        public GameplayEffectSpec MakeOutgoingSpec(GameplayEffectAsset gameplayEffect, float? level = null)
        {
            if (_abilitySystem == null || gameplayEffect == null) return null;
            return GameplayEffectSpec.CreateNew(
                gameplayEffect: gameplayEffect,
                source: _abilitySystem,
                level: level ?? _abilitySystem.Level);
        }


        /// <summary>
        /// 指定したゲームプレイ効果インスタンスを削除します。
        /// </summary>
        /// <param name="instance">削除するインスタンス。</param>
        public void RemoveGameplayEffect(IGameplayEffectInstance instance)
        {
            if (_effectSystem == null || instance == null) return;
            _effectSystem.RemoveEffect(instance);
        }

        /// <summary>
        /// 現在アクティブな全てのゲームプレイ効果インスタンスを取得します。
        /// </summary>
        public IReadOnlyList<IGameplayEffectInstance> ActiveGameplayEffects
        {
            get
            {
                if (_effectSystem == null) return System.Array.Empty<IGameplayEffectInstance>();
                return _effectSystem.ActiveEffects;
            }
        }

        /// <summary>
        /// 現在付与されている全てのアビリティスペックを取得します。
        /// </summary>
        public IReadOnlyList<IAbilitySpec> GrantedAbilities
        {
            get
            {
                if (_abilitySystem == null) return System.Array.Empty<IAbilitySpec>();
                return _abilitySystem.GrantedAbilities;
            }
        }

        /// <summary>
        /// このシステムが現在保持している全てのタグを取得します。
        /// </summary>
        public IEnumerable<IGameplayTag> OwnedTags
        {
            get
            {
                if (_abilitySystem == null) return System.Array.Empty<IGameplayTag>();
                return _abilitySystem.GetOwnedTags();
            }
        }

        void Update()
        {
            if (_attributeSystem == null || _effectSystem == null) return;

            // 属性修飾をリセット
            _attributeSystem.Core.ResetModifiers();

            // ゲームプレイ効果を更新（Tick）
            _effectSystem.Tick(Time.deltaTime);

            // 属性の現在値を再計算
            _attributeSystem.Core.UpdateCurrentValues();
        }
    }
}
