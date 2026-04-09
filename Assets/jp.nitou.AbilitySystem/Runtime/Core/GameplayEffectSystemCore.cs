using System.Collections.Generic;
using UnityEngine;

namespace Nitou.AbilitySystem.Core
{
    /// <summary>
    /// ゲームプレイ効果の適用とライフサイクルを管理するコア実装です。
    /// </summary>
    public sealed class GameplayEffectSystemCore : IGameplayEffectSystem
    {
        /// <summary>
        /// 効果が適用した属性修飾値の情報を保持する構造体です。
        /// </summary>
        private struct AppliedModifierInfo
        {
            public IAttribute Attribute;
            public AttributeModifier Modifier;
        }

        private sealed class GameplayEffectInstance : IGameplayEffectInstance
        {
            public GameplayEffectInstance(IGameplayEffectSpec spec, AppliedModifierInfo[] appliedModifiers)
            {
                Spec = spec;
                AppliedModifiers = appliedModifiers;
            }

            /// <inheritdoc />
            public IGameplayEffectSpec Spec { get; }

            /// <summary>
            /// このインスタンスが適用した属性修飾値のリスト。
            /// 効果削除時に修飾値を解除するために使用されます。
            /// </summary>
            public AppliedModifierInfo[] AppliedModifiers { get; }
        }

        private readonly List<IGameplayEffectInstance> _activeEffects = new();
        private readonly IAttributeSystem _attributeSystem;

        /// <summary>
        /// 新しい <see cref="GameplayEffectSystemCore"/> を生成します。
        /// </summary>
        /// <param name="attributeSystem">属性システム。</param>
        public GameplayEffectSystemCore(IAttributeSystem attributeSystem)
        {
            _attributeSystem = attributeSystem ?? throw new System.ArgumentNullException(nameof(attributeSystem));
        }

        /// <inheritdoc />
        public IReadOnlyList<IGameplayEffectInstance> ActiveEffects => _activeEffects;

        /// <inheritdoc />
        public void ApplyEffect(IGameplayEffectSpec spec)
        {
            if (spec == null)
            {
                throw new System.ArgumentNullException(nameof(spec));
            }

            if (spec.Definition == null)
            {
                throw new System.ArgumentException("Spec.Definition cannot be null.", nameof(spec));
            }

            switch (spec.Definition.DurationPolicy)
            {
                case DurationPolicy.Instant:
                    ApplyInstant(spec);
                    break;
                case DurationPolicy.Infinite:
                case DurationPolicy.HasDuration:
                    ApplyDurational(spec);
                    break;
            }
        }

        /// <inheritdoc />
        public void RemoveEffect(IGameplayEffectInstance instance)
        {
            if (instance == null) return;

            if (instance is GameplayEffectInstance concreteInstance)
            {
                // 適用した修飾値を解除
                RemoveModifiers(concreteInstance.AppliedModifiers);
            }

            _activeEffects.Remove(instance);
        }

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0)
            {
                return; // 負の時間は無視
            }

            // まず継続系効果を更新
            for (var i = 0; i < _activeEffects.Count; i++)
            {
                var ge = _activeEffects[i].Spec;

                if (ge?.Definition == null)
                {
                    continue; // 無効な効果はスキップ
                }

                if (ge.Definition.DurationPolicy == DurationPolicy.Instant)
                {
                    continue;
                }

                ge.UpdateRemainingDuration(deltaTime);

                ge.TickPeriodic(deltaTime, out var executePeriodicTick);
                if (executePeriodicTick)
                {
                    // 周期処理では即時効果として適用（修飾値は一時的なもの）
                    ApplyInstant(ge);
                }
            }

            // 有効期限切れの効果を削除
            var expiredEffects = new List<IGameplayEffectInstance>();
            for (var i = 0; i < _activeEffects.Count; i++)
            {
                var effect = _activeEffects[i];
                if (effect?.Spec?.Definition != null &&
                    effect.Spec.Definition.DurationPolicy == DurationPolicy.HasDuration &&
                    effect.Spec.DurationRemaining <= 0)
                {
                    expiredEffects.Add(effect);
                }
            }

            // 期限切れの効果を削除（修飾値も解除される）
            foreach (var expired in expiredEffects)
            {
                RemoveEffect(expired);
            }
        }

        /// <summary>
        /// 即時効果を属性システムに適用します。
        /// </summary>
        /// <param name="spec">適用する効果スペック。</param>
        private void ApplyInstant(IGameplayEffectSpec spec)
        {
            if (spec?.Definition == null) return;

            var definition = spec.Definition;
            if (definition.Modifiers == null) return;

            for (var i = 0; i < definition.Modifiers.Length; i++)
            {
                var modifier = definition.Modifiers[i];
                if (modifier.ModifierMagnitude == null || modifier.Attribute == null)
                {
                    continue; // 無効な修飾子はスキップ
                }

                var magnitude = modifier.ModifierMagnitude.CalculateMagnitude(spec);
                if (!magnitude.HasValue)
                {
                    continue; // マグニチュードが計算できない場合はスキップ
                }

                var finalMagnitude = magnitude.Value * modifier.Multiplier;
                var attribute = modifier.Attribute;

                if (!_attributeSystem.TryGetValue(attribute, out _))
                {
                    continue; // 属性が存在しない場合はスキップ
                }

                var attrModifier = CreateAttributeModifier(modifier.ModifierType, finalMagnitude);
                _attributeSystem.ApplyModifier(attribute, attrModifier);
            }
        }

        /// <summary>
        /// 継続効果を登録し、その修飾値を属性システムに適用します。
        /// </summary>
        /// <param name="spec">登録する効果スペック。</param>
        private void ApplyDurational(IGameplayEffectSpec spec)
        {
            if (spec?.Definition == null) return;

            var definition = spec.Definition;
            if (definition.Modifiers == null) return;

            var appliedModifiers = new List<AppliedModifierInfo>();

            // 各修飾値を計算して適用
            for (var i = 0; i < definition.Modifiers.Length; i++)
            {
                var modifier = definition.Modifiers[i];
                if (modifier.ModifierMagnitude == null || modifier.Attribute == null)
                {
                    continue; // 無効な修飾子はスキップ
                }

                var magnitude = modifier.ModifierMagnitude.CalculateMagnitude(spec);
                if (!magnitude.HasValue)
                {
                    continue; // マグニチュードが計算できない場合はスキップ
                }

                var finalMagnitude = magnitude.Value * modifier.Multiplier;
                var attribute = modifier.Attribute;

                if (!_attributeSystem.TryGetValue(attribute, out _))
                {
                    continue; // 属性が存在しない場合はスキップ
                }

                // 修飾値を適用
                var attrModifier = CreateAttributeModifier(modifier.ModifierType, finalMagnitude);
                _attributeSystem.ApplyModifier(attribute, attrModifier);

                // 削除時に解除するために記録（逆方向の修飾値を作成）
                var removalModifier = CreateRemovalModifier(modifier.ModifierType, finalMagnitude);
                if (removalModifier.Add != 0 || removalModifier.Multiply != 0 || removalModifier.OverrideValue != 0)
                {
                    appliedModifiers.Add(new AppliedModifierInfo
                    {
                        Attribute = attribute,
                        Modifier = removalModifier
                    });
                }
            }

            // インスタンスを登録（適用した修飾値を記録）
            _activeEffects.Add(new GameplayEffectInstance(spec, appliedModifiers.ToArray()));
        }

        /// <summary>
        /// 指定した修飾タイプとマグニチュードから属性修飾値を生成します。
        /// </summary>
        private AttributeModifier CreateAttributeModifier(AttributeModifierType type, float magnitude)
        {
            var modifier = new AttributeModifier();

            switch (type)
            {
                case AttributeModifierType.Add:
                    modifier.Add = magnitude;
                    break;
                case AttributeModifierType.Multiply:
                    modifier.Multiply = magnitude;
                    break;
                case AttributeModifierType.Override:
                    modifier.OverrideValue = magnitude;
                    break;
            }

            return modifier;
        }

        /// <summary>
        /// 効果削除時に修飾値を解除するための逆方向の修飾値を生成します。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 計算式: CurrentValue = (BaseValue + Add) * (1 + Multiply)
        /// </para>
        /// <para>
        /// - <b>Add の解除</b>: Add = -magnitude を適用することで、加算分を相殺します。
        /// </para>
        /// <para>
        /// - <b>Multiply の解除</b>: Multiply = -magnitude / (1 + magnitude) を適用します。
        ///   これにより (1 + m) * (1 + (-m / (1 + m))) = 1 となり、乗算分が相殺されます。
        ///   ただし、magnitude == -1 の場合は (1 + (-1)) = 0 となり 0 除算が発生するため、
        ///   この場合は解除できません（警告を出力するか、別の方法を検討してください）。
        /// </para>
        /// <para>
        /// - <b>Override の解除</b>: OverrideValue = 0 を設定することで、上書きを解除します。
        ///   これにより、次の Override 修飾値が適用されるか、なければ元の値に戻ります。
        /// </para>
        /// <para>
        /// <b>注意</b>: 複数の効果が累積している場合、個別の解除は正確に動作しますが、
        /// 効果の適用順序によって最終的な値が異なる可能性があります。
        /// </para>
        /// </remarks>
        /// <param name="type">修飾タイプ。</param>
        /// <param name="magnitude">適用されたマグニチュード。</param>
        /// <returns>解除用の修飾値。解除できない場合は空の修飾値を返します。</returns>
        private AttributeModifier CreateRemovalModifier(AttributeModifierType type, float magnitude)
        {
            var modifier = new AttributeModifier();

            switch (type)
            {
                case AttributeModifierType.Add:
                    // 加算の場合は負の値を加算して解除
                    modifier.Add = -magnitude;
                    break;
                case AttributeModifierType.Multiply:
                    // 乗算の場合は逆数を計算して解除
                    // (1 + m) * (1 + (-m / (1 + m))) = 1 となるように
                    if (System.Math.Abs(magnitude + 1f) < 0.0001f) // magnitude == -1 に近い場合
                    {
                        // magnitude == -1 の場合は解除できない
                        // この場合は空の修飾値を返す（解除されない）
                        // 警告: この効果は削除されても属性値に影響が残ります
                        Debug.LogWarning(
                            $"Cannot remove multiplicative modifier with magnitude -1. " +
                            $"The effect will be removed but the attribute value will remain modified.");
                    }
                    else
                    {
                        modifier.Multiply = -magnitude / (1f + magnitude);
                    }
                    break;
                case AttributeModifierType.Override:
                    // 上書きの場合は 0 を設定して解除（元の値に戻すため）
                    modifier.OverrideValue = 0;
                    break;
            }

            return modifier;
        }

        /// <summary>
        /// 指定した修飾値のリストを属性システムから解除します。
        /// </summary>
        private void RemoveModifiers(AppliedModifierInfo[] modifiers)
        {
            if (modifiers == null) return;

            for (var i = 0; i < modifiers.Length; i++)
            {
                var info = modifiers[i];
                if (info.Attribute == null) continue;

                // 逆方向の修飾値を適用して解除
                _attributeSystem.ApplyModifier(info.Attribute, info.Modifier);
            }
        }
    }
}
