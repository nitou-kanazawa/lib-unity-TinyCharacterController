using System;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nitou.AbilitySystem.Data
{
    /// <summary>
    /// ゲームプレイ効果の属性修飾を表す構造体（Data レイヤ版）。
    /// Unity のシリアライズ用に Data レイヤに保持されます。
    /// </summary>
    [Serializable]
    public struct GameplayEffectModifier
    {
        public AttributeAsset Attribute;

        /// <summary>
        /// 修飾タイプ（Unity シリアライズ用に enum として保存）。
        /// </summary>
        [SerializeField]
        private AttributeModifierType ModifierType;

        [FormerlySerializedAs("ModifireMagnitude")]
        public ModifierMagnitudeAsset modifierMagnitude;

        /// <summary>
        /// マグニチュードの乗数。
        /// </summary>
        public float Multiplier;

        /// <summary>
        /// 修飾タイプ（Core レイヤの enum として取得）。
        /// </summary>
        public AttributeModifierType ModifierTypeValue
        {
            get => ModifierType;
            set => ModifierType = value;
        }
    }
}