# Nitou Ability System v2 - 使用方法ガイド

本ドキュメントは、Nitou Ability System v2 の実践的な使用方法をステップバイステップで解説します。

---

## 📚 目次

1. [クイックスタート](#クイックスタート)
2. [基本的なセットアップ](#基本的なセットアップ)
3. [属性システムの使用](#属性システムの使用)
4. [ゲームプレイ効果の作成と適用](#ゲームプレイ効果の作成と適用)
5. [アビリティの作成と使用](#アビリティの作成と使用)
6. [ゲームプレイタグの使用](#ゲームプレイタグの使用)
7. [実装例](#実装例)

---

## 🚀 クイックスタート

### 最小限のセットアップ（5分で動かす）

#### ステップ1: キャラクターの準備

1. Unity シーンに空の GameObject を作成（例: "Player"）
2. `AttributeSystemComponent` を追加
3. `AbilitySystemCharacter` を追加
4. `AbilitySystemCharacter` の `Attribute System` フィールドに `AttributeSystemComponent` をドラッグ&ドロップ

#### ステップ2: 属性の作成

1. プロジェクトウィンドウで右クリック → `Create > Ability System > Attribute`
2. アセット名を `HealthAttribute` に変更
3. インスペクタで `Attribute Name` を "Health" に設定
4. `AttributeSystemComponent` の `Attributes` リストに `HealthAttribute` を追加

#### ステップ3: ダメージ効果の作成

1. プロジェクトウィンドウで右クリック → `Create > Ability System > GameplayEffectAsset`
2. アセット名を `DamageEffect` に変更
3. インスペクタで以下を設定:
   - `Gameplay Effect > Duration Policy`: `Instant`
   - `Gameplay Effect > Modifiers` のサイズを 1 に設定
   - `Modifiers[0]`:
     - `Attribute`: `HealthAttribute`
     - `Modifier Type Value`: `Add`
     - `Modifier Magnitude`: 新規作成（`Create > Gameplay Ability System > Gameplay Effect > Modifier Magnitude > Simple Float`）
     - `Multiplier`: 1.0
4. 作成した `SimpleFloatModifierMagnitude` アセットを開き:
   - `Scaling Function` のアニメーションカーブを編集
   - キーポイント: (0, -10) と (1, -10) を設定（固定値 -10）

#### ステップ4: 効果を適用するスクリプト

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class SimpleDamageTest : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private GameplayEffectAsset damageEffect;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ダメージ効果を適用
            abilitySystem.ApplyGameplayEffect(damageEffect);
            
            // 現在のHPを表示
            var attributeSystem = abilitySystem.AttributeSystem;
            if (attributeSystem.Core.TryGetValue(
                damageEffect.gameplayEffect.Modifiers[0].Attribute, 
                out var health))
            {
                Debug.Log($"Health: {health.CurrentValue}");
            }
        }
    }
}
```

5. このスクリプトを `Player` に追加し、インスペクタで `Ability System` と `Damage Effect` を設定
6. 再生して Space キーを押すと、HP が 10 ずつ減ります

---

## 基本的なセットアップ

### 1. キャラクターのセットアップ

#### Unity エディタでの操作

1. **GameObject の作成**
   - シーンに空の GameObject を作成（例: "Player"）

2. **コンポーネントの追加**
   - `AttributeSystemComponent` を追加
   - `AbilitySystemCharacter` を追加

3. **コンポーネントの接続**
   - `AbilitySystemCharacter` のインスペクタで `Attribute System` フィールドに `AttributeSystemComponent` をドラッグ&ドロップ

#### コードでの設定（オプション）

```csharp
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    private AbilitySystemCharacter _abilitySystem;
    private AttributeSystemComponent _attributeSystem;

    void Awake()
    {
        // コンポーネントを取得
        _attributeSystem = GetComponent<AttributeSystemComponent>();
        _abilitySystem = GetComponent<AbilitySystemCharacter>();
        
        // 接続（インスペクタで設定している場合は不要）
        _abilitySystem.AttributeSystem = _attributeSystem;
    }
}
```

### 2. 属性アセットの作成

#### Unity エディタでの操作

1. **属性アセットの作成**
   - プロジェクトウィンドウで右クリック
   - `Create > Ability System > Attribute` を選択
   - アセット名を設定（例: `HealthAttribute`, `ManaAttribute`）

2. **属性の設定**
   - 作成したアセットを選択
   - インスペクタで `Attribute Name` を設定（例: "Health"）

3. **属性システムへの登録**
   - `AttributeSystemComponent` を選択
   - `Attributes` リストに作成した属性アセットを追加

#### よく使う属性の例

- **HealthAttribute**: 体力
- **ManaAttribute**: マナ/MP
- **StrengthAttribute**: 力
- **AgilityAttribute**: 敏捷性
- **DefenseAttribute**: 防御力

---

## 属性システムの使用

### 属性値の取得

```csharp
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private AttributeSystemComponent attributeSystem;
    [SerializeField] private AttributeAsset healthAttribute;

    void Update()
    {
        // 属性値を取得
        if (attributeSystem.Core.TryGetValue(healthAttribute, out var healthValue))
        {
            Debug.Log($"HP: {healthValue.CurrentValue} / {healthValue.BaseValue}");
            
            // CurrentValue: 現在の値（修飾値が適用された後）
            // BaseValue: ベース値（修飾値が適用される前）
        }
    }
}
```

### 属性のベース値を設定

```csharp
public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] private AttributeSystemComponent attributeSystem;
    [SerializeField] private AttributeAsset healthAttribute;
    [SerializeField] private AttributeAsset manaAttribute;

    void Start()
    {
        // 初期値を設定
        attributeSystem.Core.SetBaseValue(healthAttribute, 100f);
        attributeSystem.Core.SetBaseValue(manaAttribute, 50f);
        
        // 現在値を更新（重要！）
        attributeSystem.Core.UpdateCurrentValues();
    }
}
```

### 属性値の変更

```csharp
public class HealthManager : MonoBehaviour
{
    [SerializeField] private AttributeSystemComponent attributeSystem;
    [SerializeField] private AttributeAsset healthAttribute;

    public void Heal(float amount)
    {
        var currentBase = attributeSystem.Core.GetBaseValue(healthAttribute);
        attributeSystem.Core.SetBaseValue(healthAttribute, currentBase + amount);
        attributeSystem.Core.UpdateCurrentValues();
    }

    public void TakeDamage(float damage)
    {
        var currentBase = attributeSystem.Core.GetBaseValue(healthAttribute);
        attributeSystem.Core.SetBaseValue(healthAttribute, currentBase - damage);
        attributeSystem.Core.UpdateCurrentValues();
    }
}
```

---

## ゲームプレイ効果の作成と適用

### 1. ゲームプレイ効果アセットの作成

#### Unity エディタでの操作

1. **アセットの作成**
   - プロジェクトウィンドウで右クリック
   - `Create > Ability System > GameplayEffectAsset` を選択

2. **基本設定**
   - `Gameplay Effect > Duration Policy` を選択:
     - **Instant**: 即座に適用され、継続しない（ダメージなど）
     - **HasDuration**: 指定時間継続（バフ/デバフなど）
     - **Infinite**: 無期限で継続（手動で削除するまで）

### 2. 即時効果の作成例（ダメージ）

#### ステップ1: モディファイアマグニチュードの作成

1. プロジェクトウィンドウで右クリック
2. `Create > Gameplay Ability System > Gameplay Effect > Modifier Magnitude > Simple Float` を選択
3. アセット名を `Damage10Magnitude` に変更
4. インスペクタで `Scaling Function` を設定:
   - アニメーションカーブエディタを開く
   - キーポイントを追加: (0, -10) と (1, -10)
   - これで固定値 -10 のダメージになります

#### ステップ2: ゲームプレイ効果の設定

1. `GameplayEffectAsset` を選択
2. インスペクタで以下を設定:
   ```
   Gameplay Effect:
     Duration Policy: Instant
     Modifiers (Size: 1):
       Element 0:
         Attribute: HealthAttribute
         Modifier Type Value: Add
         Modifier Magnitude: Damage10Magnitude
         Multiplier: 1.0
   ```

#### ステップ3: 効果の適用

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class DamageApplier : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter target;
    [SerializeField] private GameplayEffectAsset damageEffect;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            // ダメージ効果を適用
            target.ApplyGameplayEffect(damageEffect);
        }
    }
}
```

### 3. 継続効果の作成例（バフ）

#### ステップ1: 継続時間のマグニチュードを作成

1. `Create > Gameplay Ability System > Gameplay Effect > Modifier Magnitude > Simple Float` を選択
2. アセット名を `Duration5Seconds` に変更
3. `Scaling Function` を設定: (0, 5) と (1, 5)（5秒間）

#### ステップ2: 効果量のマグニチュードを作成

1. 同様に `StrengthBuff50Percent` を作成
2. `Scaling Function` を設定: (0, 0.5) と (1, 0.5)（50%増加）

#### ステップ3: ゲームプレイ効果の設定

1. `GameplayEffectAsset` を選択
2. インスペクタで以下を設定:
   ```
   Gameplay Effect:
     Duration Policy: HasDuration
     Duration Modifier: Duration5Seconds
     Duration Multiplier: 1.0
     Modifiers (Size: 1):
       Element 0:
         Attribute: StrengthAttribute
         Modifier Type Value: Multiply
         Modifier Magnitude: StrengthBuff50Percent
         Multiplier: 1.0
   ```

#### ステップ4: 効果の適用と削除

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset strengthBuff;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // バフを適用
            character.ApplyGameplayEffect(strengthBuff);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // すべてのバフを削除
            var activeEffects = character.ActiveGameplayEffects;
            foreach (var effect in activeEffects)
            {
                if (effect.Spec.Definition == strengthBuff)
                {
                    character.RemoveGameplayEffect(effect);
                }
            }
        }
    }
}
```

### 4. 修飾タイプの説明

#### Add（加算）
- 属性値に直接加算します
- 例: HP +10, MP -5

#### Multiply（乗算）
- 属性値に乗算します（1.0 = 100%, 0.5 = 50%増加, -0.5 = 50%減少）
- 例: 攻撃力 ×1.5（50%増加）

#### Override（上書き）
- 属性値を指定値で上書きします
- 例: HP を 100 に固定

### 5. モディファイアマグニチュードの種類

#### SimpleFloatModifierMagnitude（固定値）

**使い方:**
1. アセットを作成
2. `Scaling Function` のアニメーションカーブを設定
3. レベルに応じた値の変化をカーブで定義

**例:**
- 固定ダメージ -10: カーブを (0, -10) と (1, -10) に設定
- レベルに応じたダメージ: カーブを (0, -10) から (10, -100) に設定

#### AttributeBackedModifierMagnitude（属性ベース）

**使い方:**
1. アセットを作成
2. `Capture Attribute Which`: キャプチャする属性を選択
3. `Capture Attribute From`: Source（ソース）または Target（ターゲット）を選択
4. `Capture Attribute When`: OnCreation（作成時）または OnApplication（適用時）を選択
5. `Scaling Function`: キャプチャした属性値から効果量への変換カーブを設定

**例: ソースの攻撃力に基づくダメージ**
```
Capture Attribute Which: StrengthAttribute
Capture Attribute From: Source
Capture Attribute When: OnCreation
Scaling Function: (0, 0) から (100, 1.0) の線形カーブ
→ ソースの Strength が 100 の場合、効果量は 1.0 倍
```

---

## アビリティの作成と使用

### 1. シンプルなアビリティの作成

#### ステップ1: アビリティアセットの作成

1. プロジェクトウィンドウで右クリック
2. `Create > Gameplay Ability System > Abilities > Simple Ability` を選択
3. アセット名を設定（例: `FireballAbility`）

#### ステップ2: アビリティの設定

インスペクタで以下を設定:

```
Ability Name: "Fireball"
Cost Asset: [コスト効果を設定]（例: Mana -20）
Cooldown Asset: [クールダウン効果を設定]（例: 3秒間のクールダウン）
Gameplay Effect: [メイン効果を設定]（例: ダメージ効果）
```

#### ステップ3: コスト効果の作成

1. `GameplayEffectAsset` を作成（`ManaCost20`）
2. 設定:
   ```
   Duration Policy: Instant
   Modifiers:
     Attribute: ManaAttribute
     Modifier Type Value: Add
     Modifier Magnitude: [固定値 -20 のマグニチュード]
   ```

#### ステップ4: クールダウン効果の作成

1. `GameplayEffectAsset` を作成（`FireballCooldown`）
2. 設定:
   ```
   Duration Policy: HasDuration
   Duration Modifier: [3秒のマグニチュード]
   Gameplay Effect Tags:
     Granted Tags: [FireballCooldownTag]（新規作成）
   ```

#### ステップ5: アビリティの使用

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class AbilityUser : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private SimpleAbilityScriptableObject fireballAbility;

    void Start()
    {
        // アビリティを付与
        abilitySystem.GrantAbility(fireballAbility);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 付与されたアビリティを取得
            var grantedAbilities = abilitySystem.GrantedAbilities;
            foreach (var spec in grantedAbilities)
            {
                if (spec.Definition == fireballAbility)
                {
                    // アビリティを起動
                    if (abilitySystem.TryActivateAbility(spec))
                    {
                        Debug.Log("Fireball activated!");
                    }
                    else
                    {
                        Debug.Log("Cannot activate fireball (cooldown or insufficient mana)");
                    }
                    break;
                }
            }
        }
    }
}
```

### 2. カスタムアビリティの作成

`SimpleAbilityScriptableObject` を継承してカスタムアビリティを作成できます。

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay Ability System/Abilities/Custom Ability")]
public class CustomAbilityScriptableObject : AbstractAbilityScriptableObject
{
    [SerializeField] public GameplayEffectAsset customEffect;

    public override IAbilitySpec CreateSpec(IAbilitySystem owner)
    {
        if (owner == null) throw new System.ArgumentNullException(nameof(owner));
        var spec = new CustomAbilitySpec(this, owner);
        spec.Level = owner.Level;
        return spec;
    }

    public class CustomAbilitySpec : AbstractAbilitySpec
    {
        public CustomAbilitySpec(AbstractAbilityScriptableObject abilitySO, IAbilitySystem owner) 
            : base(abilitySO, owner)
        {
        }

        public override void CancelAbility()
        {
            // アビリティがキャンセルされたときの処理
        }

        protected override IEnumerator ActivateAbility()
        {
            // コストとクールダウンを適用
            if (Ability.CooldownAsset != null)
            {
                var cdSpec = GameplayEffectSpec.CreateNew(Ability.CooldownAsset, Owner, Level);
                Owner.GameplayEffectSystem.ApplyEffect(cdSpec);
            }

            if (Ability.CostAsset != null)
            {
                var costSpec = GameplayEffectSpec.CreateNew(Ability.CostAsset, Owner, Level);
                Owner.GameplayEffectSystem.ApplyEffect(costSpec);
            }

            // カスタムロジック
            var ability = Ability as CustomAbilityScriptableObject;
            if (ability?.customEffect != null)
            {
                var effectSpec = GameplayEffectSpec.CreateNew(ability.customEffect, Owner, Level);
                Owner.GameplayEffectSystem.ApplyEffect(effectSpec);
            }

            yield return null;
        }

        public override bool CheckGameplayTags()
        {
            // タグチェック（必要に応じて実装）
            var tags = Ability.Tags;
            return true; // 常に許可する例
        }

        protected override IEnumerator PreActivate()
        {
            // アクティベート前の処理
            yield return null;
        }
    }
}
```

---

## ゲームプレイタグの使用

### 1. タグアセットの作成

#### Unity エディタでの操作

1. プロジェクトウィンドウで右クリック
2. `Create > Ability System > Tag` を選択
3. アセット名を設定（例: `CombatTag`, `StunTag`）

#### 階層タグの作成

1. 親タグを作成（例: `CombatTag`）
2. 子タグを作成（例: `AttackTag`）
3. 子タグのインスペクタで `Parent` に親タグを設定

**例: タグ階層**
```
Combat (親)
  ├─ Attack (子)
  │   ├─ Melee (孫)
  │   └─ Ranged (孫)
  └─ Defense (子)
      └─ Block (孫)
```

### 2. タグを使用した条件チェック

#### アビリティのタグ要件

アビリティアセットのインスペクタで設定:

```
Ability Tags:
  Owner Tags:
    Require Tags: [必要なタグを追加]
    Ignore Tags: [持っていてはいけないタグを追加]
  Source Tags: [同様に設定]
  Target Tags: [同様に設定]
```

#### ゲームプレイ効果のタグ要件

ゲームプレイ効果アセットのインスペクタで設定:

```
Gameplay Effect Tags:
  Application Tag Requirements:
    Require Tags: [適用時に必要なタグ]
    Ignore Tags: [適用時に持っていてはいけないタグ]
  Ongoing Tag Requirements: [継続中に必要なタグ]
  Removal Tag Requirements: [削除時に必要なタグ]
```

### 3. タグの動的管理

```csharp
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private GameplayTagAsset stunTag;

    public void ApplyStun()
    {
        // タグを追加
        abilitySystem.AbilitySystem.AddTag(stunTag);
    }

    public void RemoveStun()
    {
        // タグを削除
        abilitySystem.AbilitySystem.RemoveTag(stunTag);
    }

    public bool IsStunned()
    {
        // タグの確認
        foreach (var tag in abilitySystem.OwnedTags)
        {
            if (ReferenceEquals(tag, stunTag))
            {
                return true;
            }
        }
        return false;
    }
}
```

---

## 実装例

### 例1: 完全なダメージシステム

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter attacker;
    [SerializeField] private AbilitySystemCharacter target;
    [SerializeField] private GameplayEffectAsset damageEffect;

    public void DealDamage(float damageAmount)
    {
        // ダメージ効果のスペックを生成
        var spec = attacker.MakeOutgoingSpec(damageEffect);
        
        // ターゲットに適用
        target.ApplyGameplayEffect(spec);
    }

    // レベルに応じたダメージ
    public void DealDamageWithLevel(float level)
    {
        var spec = attacker.MakeOutgoingSpec(damageEffect, level);
        target.ApplyGameplayEffect(spec);
    }
}
```

### 例2: バフ/デバフシステム

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class BuffSystem : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter character;
    [SerializeField] private GameplayEffectAsset strengthBuff;
    [SerializeField] private GameplayEffectAsset weaknessDebuff;

    public void ApplyStrengthBuff(float duration, float level = 1f)
    {
        var spec = character.MakeOutgoingSpec(strengthBuff, level);
        spec.SetDuration(duration);
        character.ApplyGameplayEffect(spec);
    }

    public void ApplyWeaknessDebuff(float duration, float level = 1f)
    {
        var spec = character.MakeOutgoingSpec(weaknessDebuff, level);
        spec.SetDuration(duration);
        character.ApplyGameplayEffect(spec);
    }

    public void RemoveAllBuffs()
    {
        var activeEffects = character.ActiveGameplayEffects;
        foreach (var effect in activeEffects)
        {
            character.RemoveGameplayEffect(effect);
        }
    }
}
```

### 例3: アビリティクールダウンの確認

```csharp
using Nitou.AbilitySystem.Data;
using Nitou.AbilitySystem.Core;
using Nitou.AbilitySystem.Unity.Components;
using UnityEngine;

public class AbilityCooldownChecker : MonoBehaviour
{
    [SerializeField] private AbilitySystemCharacter abilitySystem;
    [SerializeField] private SimpleAbilityScriptableObject ability;

    void Update()
    {
        var grantedAbilities = abilitySystem.GrantedAbilities;
        foreach (var spec in grantedAbilities)
        {
            if (spec.Definition == ability)
            {
                // アビリティが使用可能かチェック
                if (spec.CanActivate())
                {
                    Debug.Log("Ability is ready!");
                }
                else
                {
                    Debug.Log("Ability is on cooldown or cannot be activated");
                }
                break;
            }
        }
    }
}
```

---

## よくある質問（FAQ）

### Q: 属性値が更新されない

**A:** 以下の点を確認してください:
1. `AttributeSystemComponent` が `AbilitySystemCharacter` に正しく設定されているか
2. `AbilitySystemCharacter` の `Update()` が呼ばれているか（自動的に処理されます）
3. 属性が `AttributeSystemComponent` の `Attributes` リストに登録されているか

### Q: アビリティが起動しない

**A:** 以下を確認してください:
1. アビリティが `GrantAbility()` で付与されているか
2. コストが足りているか（`CostAsset` が設定されている場合）
3. クールダウン中でないか（`CooldownAsset` が設定されている場合）
4. タグ要件を満たしているか

### Q: 効果が適用されない

**A:** 以下を確認してください:
1. 効果の `Duration Policy` が正しく設定されているか
2. 属性が `AttributeSystemComponent` に登録されているか
3. 効果の `Modifiers` が正しく設定されているか
4. `Modifier Magnitude` が正しく設定されているか

### Q: レベルシステムの使い方

**A:** レベルに応じて効果をスケーリングするには:

```csharp
// システム全体のレベルを設定
abilitySystem.Level = 5f;

// レベルに応じて効果がスケーリングされる
// ModifierMagnitude の ScalingFunction でレベルを使用
```

`SimpleFloatModifierMagnitude` の `ScalingFunction` は、X軸がレベル、Y軸が効果量を表します。

---

## ベストプラクティス

### 1. 属性システムの初期化

```csharp
void Start()
{
    // 属性のベース値を設定
    attributeSystem.Core.SetBaseValue(healthAttribute, 100f);
    attributeSystem.Core.SetBaseValue(manaAttribute, 50f);
    
    // 現在値を更新（重要！）
    attributeSystem.Core.UpdateCurrentValues();
}
```

### 2. エラーハンドリング

```csharp
// 属性値の取得時は TryGetValue を使用
if (attributeSystem.Core.TryGetValue(healthAttribute, out var healthValue))
{
    // 正常に取得できた場合の処理
}
else
{
    Debug.LogWarning("Health attribute not found");
}
```

### 3. パフォーマンス

- ゲームプレイ効果アセットのプロパティは自動的にキャッシュされるため、頻繁にアクセスしても問題ありません
- アビリティの付与/剥奪は O(1) で実行されます（HashSet を使用）
- タグクエリは階層関係を考慮する場合のみ List を生成します

---

## 参考資料

- [DESIGN.md](DESIGN.md) - アーキテクチャ設計の詳細
- [CODE_REVIEW.md](CODE_REVIEW.md) - コードレビューと評価
