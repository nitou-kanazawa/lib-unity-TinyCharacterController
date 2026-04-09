# Nitou Ability System v2 - 見下ろし型3Dシューティングゲームでの活用例

「Escape from Tarkov」のような見下ろし型3Dシューティングゲームでの Ability System の活用方法を解説します。

---

## 🎯 活用できる主要なシステム

### 1. **武器システム**
### 2. **ステータス管理（HP、スタミナ、負傷など）**
### 3. **バフ/デバフ（薬物、負傷、疲労など）**
### 4. **スキルシステム**
### 5. **装備効果**
### 6. **環境効果（天候、時間帯など）**

---

## 1. 武器システム

### コンセプト
各武器を「アビリティ」として扱い、発射、リロード、スコープ切り替えなどをアビリティとして実装します。

### 実装例

#### 武器アビリティの作成

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Shooter/Abilities/Weapon Ability")]
public class WeaponAbilityScriptableObject : AbstractAbilityScriptableObject
{
    [Header("Weapon Settings")]
    [SerializeField] public float Damage;
    [SerializeField] public float FireRate; // 秒間発射数
    [SerializeField] public int MagazineSize;
    [SerializeField] public float ReloadTime;
    [SerializeField] public float Range;
    [SerializeField] public GameplayEffectAsset DamageEffect; // ダメージ効果

    public override IAbilitySpec CreateSpec(IAbilitySystem owner)
    {
        return new WeaponAbilitySpec(this, owner);
    }

    public class WeaponAbilitySpec : AbstractAbilitySpec
    {
        private int _currentAmmo;
        private float _lastFireTime;
        private bool _isReloading;

        public WeaponAbilitySpec(AbstractAbilityScriptableObject abilitySO, IAbilitySystem owner) 
            : base(abilitySO, owner)
        {
            var weapon = abilitySO as WeaponAbilityScriptableObject;
            _currentAmmo = weapon?.MagazineSize ?? 0;
        }

        public override void CancelAbility()
        {
            _isReloading = false;
        }

        protected override IEnumerator ActivateAbility()
        {
            var weapon = Ability as WeaponAbilityScriptableObject;
            if (weapon == null) yield break;

            // リロード中は発射できない
            if (_isReloading) yield break;

            // クールダウンチェック（発射レート制限）
            var timeSinceLastFire = Time.time - _lastFireTime;
            var fireInterval = 1f / weapon.FireRate;
            if (timeSinceLastFire < fireInterval)
            {
                yield break;
            }

            // 弾薬チェック
            if (_currentAmmo <= 0)
            {
                // 自動リロード
                yield return StartCoroutine(Reload());
                yield break;
            }

            // 発射
            _currentAmmo--;
            _lastFireTime = Time.time;

            // レイキャストで命中判定
            var hit = PerformRaycast(weapon.Range);
            if (hit.collider != null)
            {
                // 命中したターゲットにダメージ効果を適用
                var target = hit.collider.GetComponent<AbilitySystemCharacter>();
                if (target != null)
                {
                    var damageSpec = GameplayEffectSpec.CreateNew(weapon.DamageEffect, Owner, Level);
                    target.ApplyGameplayEffect(damageSpec);
                }
            }

            yield return null;
        }

        private IEnumerator Reload()
        {
            var weapon = Ability as WeaponAbilityScriptableObject;
            if (weapon == null) yield break;

            _isReloading = true;
            yield return new WaitForSeconds(weapon.ReloadTime);
            _currentAmmo = weapon.MagazineSize;
            _isReloading = false;
        }

        private RaycastHit PerformRaycast(float range)
        {
            // カメラの中心からレイキャスト
            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Physics.Raycast(ray, out var hit, range);
            return hit;
        }

        public override bool CheckGameplayTags()
        {
            // 武器使用可能な状態かチェック（例: スタン中は使用不可）
            return true;
        }

        protected override IEnumerator PreActivate()
        {
            yield return null;
        }

        // 現在の弾薬数を取得
        public int CurrentAmmo => _currentAmmo;
        public bool IsReloading => _isReloading;
    }
}
```

#### 武器の使用

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private WeaponAbilityScriptableObject primaryWeapon;
    [SerializeField] private WeaponAbilityScriptableObject secondaryWeapon;

    private IAbilitySpec _currentWeaponSpec;

    void Start()
    {
        // プライマリ武器を付与
        abilitySystem.GrantAbility(primaryWeapon);
        
        // 武器スペックを取得
        foreach (var spec in abilitySystem.GrantedAbilities)
        {
            if (spec.Definition == primaryWeapon)
            {
                _currentWeaponSpec = spec;
                break;
            }
        }
    }

    void Update()
    {
        // マウスクリックで発射
        if (Input.GetMouseButton(0) && _currentWeaponSpec != null)
        {
            abilitySystem.TryActivateAbility(_currentWeaponSpec);
        }

        // Rキーでリロード
        if (Input.GetKeyDown(KeyCode.R) && _currentWeaponSpec is WeaponAbilityScriptableObject.WeaponAbilitySpec weaponSpec)
        {
            if (!weaponSpec.IsReloading && weaponSpec.CurrentAmmo < (primaryWeapon.MagazineSize))
            {
                // リロードアビリティを起動（実装が必要）
            }
        }
    }
}
```

---

## 2. ステータス管理

### 属性の定義

#### 必要な属性アセット

1. **HealthAttribute** - 体力
2. **StaminaAttribute** - スタミナ
3. **HungerAttribute** - 空腹度
4. **ThirstAttribute** - 喉の渇き
5. **BleedingAttribute** - 出血度
6. **PainAttribute** - 痛み
7. **EnergyAttribute** - エネルギー

#### 属性の初期化

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class PlayerStatusInitializer : MonoBehaviour
{
    [SerializeField] private AttributeSystemComponent attributeSystem;
    [SerializeField] private AttributeAsset healthAttribute;
    [SerializeField] private AttributeAsset staminaAttribute;
    [SerializeField] private AttributeAsset hungerAttribute;
    [SerializeField] private AttributeAsset thirstAttribute;

    void Start()
    {
        // 初期値を設定
        attributeSystem.Core.SetBaseValue(healthAttribute, 100f);
        attributeSystem.Core.SetBaseValue(staminaAttribute, 100f);
        attributeSystem.Core.SetBaseValue(hungerAttribute, 100f);
        attributeSystem.Core.SetBaseValue(thirstAttribute, 100f);
        
        attributeSystem.Core.UpdateCurrentValues();
    }
}
```

---

## 3. バフ/デバフシステム

### 負傷効果の実装

#### 軽傷効果

1. **ゲームプレイ効果アセットを作成** (`LightInjuryEffect`)
   - `Duration Policy`: `HasDuration`
   - `Duration Modifier`: 5秒
   - `Modifiers`:
     - `Attribute`: `PainAttribute`
     - `Modifier Type`: `Add`
     - `Magnitude`: 10（痛み +10）

#### 重傷効果

1. **ゲームプレイ効果アセットを作成** (`HeavyInjuryEffect`)
   - `Duration Policy`: `HasDuration`
   - `Duration Modifier`: 30秒
   - `Modifiers`:
     - `Attribute`: `HealthAttribute`
     - `Modifier Type`: `Add`
     - `Magnitude`: -2（毎秒 -2 HP）
     - `Period`: 1秒（毎秒ダメージ）

#### 出血効果

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class InjurySystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset lightInjuryEffect;
    [SerializeField] private GameplayEffectAsset heavyInjuryEffect;
    [SerializeField] private GameplayEffectAsset bleedingEffect;

    public void ApplyLightInjury()
    {
        character.ApplyGameplayEffect(lightInjuryEffect);
    }

    public void ApplyHeavyInjury()
    {
        character.ApplyGameplayEffect(heavyInjuryEffect);
        // 重傷時は自動的に出血も発生
        character.ApplyGameplayEffect(bleedingEffect);
    }

    public void StopBleeding()
    {
        // 出血効果を削除
        var activeEffects = character.ActiveGameplayEffects;
        foreach (var effect in activeEffects)
        {
            if (effect.Spec.Definition == bleedingEffect)
            {
                character.RemoveGameplayEffect(effect);
            }
        }
    }
}
```

### 薬物効果の実装

#### 鎮痛剤効果

1. **ゲームプレイ効果アセットを作成** (`PainkillerEffect`)
   - `Duration Policy`: `HasDuration`
   - `Duration Modifier`: 300秒（5分）
   - `Modifiers`:
     - `Attribute`: `PainAttribute`
     - `Modifier Type`: `Add`
     - `Magnitude`: -50（痛み -50）

#### スタミナ回復薬

1. **ゲームプレイ効果アセットを作成** (`StaminaBoostEffect`)
   - `Duration Policy`: `HasDuration`
   - `Duration Modifier`: 60秒
   - `Modifiers`:
     - `Attribute`: `StaminaAttribute`
     - `Modifier Type`: `Multiply`
     - `Magnitude`: 0.5（スタミナ回復速度 50%増加）

```csharp
public class MedicalSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset painkillerEffect;
    [SerializeField] private GameplayEffectAsset staminaBoostEffect;

    public void UsePainkiller()
    {
        character.ApplyGameplayEffect(painkillerEffect);
    }

    public void UseStaminaBoost()
    {
        character.ApplyGameplayEffect(staminaBoostEffect);
    }
}
```

---

## 4. スキルシステム

### スキルアビリティの実装

各スキルをアビリティとして実装し、レベルアップで効果が強化されます。

#### 例: 武器スキル

```csharp
[CreateAssetMenu(menuName = "Shooter/Abilities/Weapon Skill")]
public class WeaponSkillAbilityScriptableObject : AbstractAbilityScriptableObject
{
    [SerializeField] public GameplayEffectAsset skillBuffEffect; // スキルによるバフ

    public override IAbilitySpec CreateSpec(IAbilitySystem owner)
    {
        return new WeaponSkillAbilitySpec(this, owner);
    }

    public class WeaponSkillAbilitySpec : AbstractAbilitySpec
    {
        public WeaponSkillAbilitySpec(AbstractAbilityScriptableObject abilitySO, IAbilitySystem owner) 
            : base(abilitySO, owner)
        {
        }

        public override void CancelAbility() { }

        protected override IEnumerator ActivateAbility()
        {
            // スキルは常にアクティブ（パッシブスキル）
            // スキルレベルに応じたバフを適用
            var skill = Ability as WeaponSkillAbilityScriptableObject;
            if (skill?.skillBuffEffect != null)
            {
                var effectSpec = GameplayEffectSpec.CreateNew(skill.skillBuffEffect, Owner, Level);
                Owner.GameplayEffectSystem.ApplyEffect(effectSpec);
            }
            yield return null;
        }

        public override bool CheckGameplayTags() => true;
        protected override IEnumerator PreActivate() { yield return null; }
    }
}
```

#### スキルレベルアップ

```csharp
public class SkillSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private WeaponSkillAbilityScriptableObject weaponSkill;

    public void LevelUpWeaponSkill()
    {
        // スキルアビリティを取得
        foreach (var spec in abilitySystem.GrantedAbilities)
        {
            if (spec.Definition == weaponSkill)
            {
                // レベルアップ
                spec.Level += 1f;
                break;
            }
        }
    }
}
```

---

## 5. 装備効果

### 装備アイテムによる属性変更

装備アイテムを装備すると、属性や効果が付与されます。

#### 例: アーマー装備

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset armorEffect; // アーマーによる防御力増加

    private IGameplayEffectInstance _currentArmorEffect;

    public void EquipArmor(float defenseBonus)
    {
        // 既存のアーマー効果を削除
        if (_currentArmorEffect != null)
        {
            character.RemoveGameplayEffect(_currentArmorEffect);
        }

        // 新しいアーマー効果を適用
        var spec = character.MakeOutgoingSpec(armorEffect);
        // 防御力ボーナスをレベルで表現
        spec.WithLevel(defenseBonus);
        character.ApplyGameplayEffect(spec);

        // 効果インスタンスを保存
        var activeEffects = character.ActiveGameplayEffects;
        foreach (var effect in activeEffects)
        {
            if (effect.Spec.Definition == armorEffect)
            {
                _currentArmorEffect = effect;
                break;
            }
        }
    }

    public void UnequipArmor()
    {
        if (_currentArmorEffect != null)
        {
            character.RemoveGameplayEffect(_currentArmorEffect);
            _currentArmorEffect = null;
        }
    }
}
```

#### アーマー効果の設定

1. **ゲームプレイ効果アセットを作成** (`ArmorEffect`)
   - `Duration Policy`: `Infinite`（装備中は無期限）
   - `Modifiers`:
     - `Attribute`: `DefenseAttribute`（新規作成）
     - `Modifier Type`: `Add`
     - `Magnitude`: レベルに応じて変動（装備の防御力）

---

## 6. 環境効果

### 天候効果

#### 例: 雨による視界悪化

```csharp
public class WeatherSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset rainEffect; // 視界悪化効果

    public void StartRain()
    {
        character.ApplyGameplayEffect(rainEffect);
    }

    public void StopRain()
    {
        var activeEffects = character.ActiveGameplayEffects;
        foreach (var effect in activeEffects)
        {
            if (effect.Spec.Definition == rainEffect)
            {
                character.RemoveGameplayEffect(effect);
            }
        }
    }
}
```

#### 雨効果の設定

1. **ゲームプレイ効果アセットを作成** (`RainEffect`)
   - `Duration Policy`: `Infinite`
   - `Modifiers`:
     - `Attribute`: `VisionAttribute`（視界属性、新規作成）
     - `Modifier Type`: `Multiply`
     - `Magnitude`: -0.3（視界 30%減少）

---

## 7. 統合例: 完全なプレイヤーシステム

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private AttributeSystemComponent attributeSystem;

    [Header("Weapons")]
    [SerializeField] private WeaponAbilityScriptableObject primaryWeapon;
    [SerializeField] private WeaponAbilityScriptableObject secondaryWeapon;

    [Header("Status")]
    [SerializeField] private AttributeAsset healthAttribute;
    [SerializeField] private AttributeAsset staminaAttribute;

    [Header("Effects")]
    [SerializeField] private GameplayEffectAsset lightInjuryEffect;
    [SerializeField] private GameplayEffectAsset painkillerEffect;

    private IAbilitySpec _currentWeapon;
    private float _staminaConsumptionRate = 10f; // 秒間消費量

    void Start()
    {
        // 武器を付与
        abilitySystem.GrantAbility(primaryWeapon);
        
        // 初期ステータス設定
        attributeSystem.Core.SetBaseValue(healthAttribute, 100f);
        attributeSystem.Core.SetBaseValue(staminaAttribute, 100f);
        attributeSystem.Core.UpdateCurrentValues();
    }

    void Update()
    {
        HandleWeaponInput();
        HandleStaminaConsumption();
        HandleStatusEffects();
    }

    void HandleWeaponInput()
    {
        // 武器切り替え
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(primaryWeapon);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(secondaryWeapon);
        }

        // 発射
        if (Input.GetMouseButton(0) && _currentWeapon != null)
        {
            abilitySystem.TryActivateAbility(_currentWeapon);
        }

        // リロード
        if (Input.GetKeyDown(KeyCode.R))
        {
            // リロード処理
        }
    }

    void HandleStaminaConsumption()
    {
        // 走っている間はスタミナを消費
        if (Input.GetKey(KeyCode.LeftShift))
        {
            var currentStamina = attributeSystem.Core.GetBaseValue(staminaAttribute);
            var newStamina = Mathf.Max(0, currentStamina - _staminaConsumptionRate * Time.deltaTime);
            attributeSystem.Core.SetBaseValue(staminaAttribute, newStamina);
        }
        else
        {
            // スタミナ回復（効果が適用されていない場合）
            var currentStamina = attributeSystem.Core.GetBaseValue(staminaAttribute);
            var maxStamina = 100f; // 最大スタミナ
            if (currentStamina < maxStamina)
            {
                var recoveryRate = 5f; // 秒間回復量
                var newStamina = Mathf.Min(maxStamina, currentStamina + recoveryRate * Time.deltaTime);
                attributeSystem.Core.SetBaseValue(staminaAttribute, newStamina);
            }
        }
    }

    void HandleStatusEffects()
    {
        // 痛みが高い場合は移動速度を低下
        if (attributeSystem.Core.TryGetValue(healthAttribute, out var health))
        {
            if (health.CurrentValue < 30f)
            {
                // 重傷状態の効果を適用
                if (!HasEffect(lightInjuryEffect))
                {
                    abilitySystem.ApplyGameplayEffect(lightInjuryEffect);
                }
            }
        }
    }

    void SwitchWeapon(WeaponAbilityScriptableObject weapon)
    {
        foreach (var spec in abilitySystem.GrantedAbilities)
        {
            if (spec.Definition == weapon)
            {
                _currentWeapon = spec;
                break;
            }
        }
    }

    bool HasEffect(GameplayEffectAsset effect)
    {
        foreach (var activeEffect in abilitySystem.ActiveGameplayEffects)
        {
            if (activeEffect.Spec.Definition == effect)
            {
                return true;
            }
        }
        return false;
    }

    // デバッグ用: ダメージを受ける
    [ContextMenu("Take Damage")]
    void TakeDamage()
    {
        var currentHealth = attributeSystem.Core.GetBaseValue(healthAttribute);
        attributeSystem.Core.SetBaseValue(healthAttribute, currentHealth - 10f);
        attributeSystem.Core.UpdateCurrentValues();
    }

    // デバッグ用: 鎮痛剤を使用
    [ContextMenu("Use Painkiller")]
    void UsePainkiller()
    {
        abilitySystem.ApplyGameplayEffect(painkillerEffect);
    }
}
```

---

## 8. 推奨される属性と効果の構成

### 属性（Attributes）

| 属性名 | 用途 | 初期値 |
|--------|------|--------|
| Health | 体力 | 100 |
| Stamina | スタミナ | 100 |
| Hunger | 空腹度 | 100 |
| Thirst | 喉の渇き | 100 |
| Bleeding | 出血度 | 0 |
| Pain | 痛み | 0 |
| Energy | エネルギー | 100 |
| Vision | 視界 | 100 |
| Hearing | 聴覚 | 100 |
| Defense | 防御力 | 0 |
| Accuracy | 命中精度 | 100 |

### よく使う効果（Gameplay Effects）

| 効果名 | 種類 | 用途 |
|--------|------|------|
| LightInjury | HasDuration | 軽傷 |
| HeavyInjury | HasDuration | 重傷 |
| Bleeding | HasDuration | 出血 |
| Painkiller | HasDuration | 鎮痛剤 |
| StaminaBoost | HasDuration | スタミナ回復薬 |
| ArmorEffect | Infinite | アーマー装備 |
| WeaponSkillBuff | Infinite | 武器スキルバフ |
| RainEffect | Infinite | 雨による視界悪化 |
| NightVision | HasDuration | ナイトビジョン |

---

## 9. 実装のベストプラクティス

### 1. 属性の更新タイミング

```csharp
void Update()
{
    // AbilitySystemCharacter の Update() が自動的に以下を実行:
    // 1. 属性修飾のリセット
    // 2. ゲームプレイ効果の Tick
    // 3. 属性の現在値再計算
    
    // 手動でベース値を変更する場合は、UpdateCurrentValues() を呼ぶ
    attributeSystem.Core.SetBaseValue(staminaAttribute, newValue);
    attributeSystem.Core.UpdateCurrentValues();
}
```

### 2. 効果のスタック管理

同じ効果が複数適用される場合の処理:

```csharp
public void ApplyStackableEffect(GameplayEffectAsset effect, int stacks)
{
    // 既存の効果を削除
    RemoveEffect(effect);
    
    // スタック数に応じた効果を適用
    for (int i = 0; i < stacks; i++)
    {
        abilitySystem.ApplyGameplayEffect(effect);
    }
}
```

### 3. 効果の優先度

効果の削除順序を制御する場合:

```csharp
public void RemoveEffectByPriority(GameplayTagAsset priorityTag)
{
    var activeEffects = abilitySystem.ActiveGameplayEffects;
    var effectsToRemove = new List<IGameplayEffectInstance>();
    
    foreach (var effect in activeEffects)
    {
        // 優先度タグを持つ効果を優先的に削除
        if (effect.Spec.Definition.Tags.AssetTag == priorityTag)
        {
            effectsToRemove.Add(effect);
        }
    }
    
    foreach (var effect in effectsToRemove)
    {
        abilitySystem.RemoveGameplayEffect(effect);
    }
}
```

---

## 10. パフォーマンスの考慮

### 最適化のポイント

1. **効果の適用頻度**
   - 毎フレーム適用する効果は避ける
   - 周期処理（Period）を活用して、必要なタイミングのみ適用

2. **属性の更新**
   - ベース値の変更は必要な時のみ
   - `UpdateCurrentValues()` は `AbilitySystemCharacter` が自動的に呼ぶ

3. **効果の検索**
   - 効果の検索は `ActiveGameplayEffects` を直接使用
   - 頻繁に検索する場合は、キャッシュを検討

---

## まとめ

見下ろし型3Dシューティングゲームでは、Ability System を以下のように活用できます:

1. **武器システム**: 各武器をアビリティとして実装
2. **ステータス管理**: 属性システムで HP、スタミナ、負傷などを管理
3. **バフ/デバフ**: ゲームプレイ効果で薬物、負傷、疲労などを表現
4. **スキルシステム**: スキルをアビリティとして実装し、レベルアップで強化
5. **装備効果**: 装備アイテムが属性や効果を付与
6. **環境効果**: 天候や時間帯による効果を実装

このシステムにより、複雑なステータス管理や効果の組み合わせを、データ駆動型で柔軟に実装できます。

