using System.IO;
using UnityEngine;

namespace Nitou.AbilitySystem.Data
{
    [CreateAssetMenu(
        menuName = "Ability System/GameplayEffectAsset",
        fileName = "GameplayEffectAsset"
    )]
    public class GameplayEffectAsset : ScriptableObject
    {
        [SerializeField] public GameplayEffectDefinitionContainer gameplayEffect;

        [SerializeField] public GameplayEffectTags gameplayEffectTags;

        [SerializeField] public GameplayEffectPeriod Period;
    }
}