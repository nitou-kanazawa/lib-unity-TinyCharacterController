using Nitou.AbilitySystem.Core;
using UnityEngine;

namespace Nitou.AbilitySystem.Data.ModifierMagnitude
{
    public enum ECaptureAttributeFrom
    {
        Source,
        Target
    }

    public enum ECaptureAttributeWhen
    {
        OnCreation,
        OnApplication
    }

    [CreateAssetMenu(menuName = "Gameplay Ability System/Gameplay Effect/Modifier Magnitude/Attribute Backed")]
    public class AttributeBackedModifierMagnitude : ModifierMagnitudeAsset
    {
        [SerializeField]
        private AnimationCurve ScalingFunction;

        [SerializeField]
        private AttributeAsset CaptureAttributeWhich;

        [SerializeField]
        private ECaptureAttributeFrom CaptureAttributeFrom;

        [SerializeField]
        private ECaptureAttributeWhen CaptureAttributeWhen;

        public override void Initialize(IGameplayEffectSpec spec)
        {
            // OnApplication の場合は SourceCapturedAttribute を使用
            if (CaptureAttributeWhen == ECaptureAttributeWhen.OnApplication && 
                CaptureAttributeFrom == ECaptureAttributeFrom.Source)
            {
                CaptureSourceAttribute(spec);
            }
        }

        public override float? CalculateMagnitude(IGameplayEffectSpec spec)
        {
            var captured = GetCapturedAttribute(spec);
            return captured.HasValue ? ScalingFunction.Evaluate(captured.Value.CurrentValue) : null;
        }

        /// <summary>
        /// ソースの属性をキャプチャして SourceCapturedAttribute に設定します。
        /// </summary>
        private void CaptureSourceAttribute(IGameplayEffectSpec spec)
        {
            if (spec.Source?.AttributeSystem == null || CaptureAttributeWhich == null)
                return;

            // GameplayEffectSpec にキャストして設定
            if (spec is GameplayEffectSpec concreteSpec)
            {
                if (spec.Source.AttributeSystem.TryGetValue(CaptureAttributeWhich, out var attributeValue))
                {
                    concreteSpec.SourceCapturedAttribute = attributeValue;
                }
            }
        }

        /// <summary>
        /// キャプチャ対象の属性値を取得します。
        /// </summary>
        private Core.AttributeValue? GetCapturedAttribute(IGameplayEffectSpec spec)
        {
            // OnApplication で Source の場合は、既にキャプチャされた値を使用
            if (CaptureAttributeWhen == ECaptureAttributeWhen.OnApplication && 
                CaptureAttributeFrom == ECaptureAttributeFrom.Source)
            {
                return spec.SourceCapturedAttribute;
            }

            // OnCreation の場合は、現在の値を取得
            IAttributeSystem attributeSystem = null;
            switch (CaptureAttributeFrom)
            {
                case ECaptureAttributeFrom.Source:
                    attributeSystem = spec.Source?.AttributeSystem;
                    break;
                case ECaptureAttributeFrom.Target:
                    attributeSystem = spec.Target?.AttributeSystem;
                    break;
            }

            if (attributeSystem == null || CaptureAttributeWhich == null)
                return null;

            if (attributeSystem.TryGetValue(CaptureAttributeWhich, out var attributeValue))
            {
                return attributeValue;
            }

            return null;
        }
    }
}
