## Nitou Ability System v2 設計書

本ドキュメントは、`jp.nitou.AbilitySystem` パッケージを **汎用的な GAS (Gameplay Ability System)** として再設計するためのアーキテクチャ設計を記述する。

大きく **Core / Data / Unity** の 3 レイヤに分割する。

---

## レイヤ構成

### Core レイヤ

**目的**: Unity 非依存のゲームロジックを提供する。  
`MonoBehaviour` / `ScriptableObject` を一切使わない純 C# コード。

- 例: `Runtime/Core`

#### 主要インターフェース

- `IAbilitySystem`
  - キャラクターが所持するアビリティおよび GE の窓口。
  - `IReadOnlyList<IAbilitySpec> GrantedAbilities`  
    所持アビリティ一覧。
  - `float Level`  
    アビリティや効果のスケーリングに使用するレベル。
  - `GrantAbility / RevokeAbility / RevokeAbilitiesWithTag`  
    アビリティ付与・剥奪 API。
  - `bool TryActivateAbility(IAbilitySpec spec)`  
    アビリティ起動可否の判定。実際のコルーチン起動は Unity 側で行う。

- `IAbilitySpec`
  - 実行時にインスタンス化されたアビリティ。状態とロジックを保持。
  - `IAbilityDefinition Definition` / `IAbilitySystem Owner`
  - `float Level` / `bool IsActive`
  - `bool CanActivate()` / `IEnumerator Activate()` / `void Cancel()`

- `IAbilityDefinition`
  - アビリティの静的定義。通常は ScriptableObject で実装。
  - `string DisplayName`
  - `AbilityTags Tags`
  - `IGameplayEffectDefinition? Cost` / `IGameplayEffectDefinition? Cooldown`
  - `IAbilitySpec CreateSpec(IAbilitySystem owner)`

- `IGameplayEffectDefinition`
  - GE の静的定義。
  - `DurationPolicy DurationPolicy`
  - `float BaseDuration`
  - `ModifierMagnitudeAsset? DurationMagnitude`
  - `GameplayEffectModifier[] Modifiers`
  - `ConditionalGameplayEffectContainer[] ConditionalEffects`
  - `GameplayEffectTags Tags`
  - `GameplayEffectPeriod Period`

- `IGameplayEffectSpec`
  - 実行時の GE インスタンス。
  - `IGameplayEffectDefinition Definition`
  - `float Level`
  - `float DurationRemaining` / `float TotalDuration`
  - `float TimeUntilPeriodTick`
  - `IAbilitySystem Source` / `IAbilitySystem? Target`
  - `AttributeValue? SourceCapturedAttribute`
  - `WithTarget / WithLevel / SetDuration / UpdateRemainingDuration / TickPeriodic`

- `IGameplayEffectSystem`
  - GE の寿命と Tick を管理。
  - `IReadOnlyList<IGameplayEffectInstance> ActiveEffects`
  - `ApplyEffect(IGameplayEffectSpec spec)`
  - `RemoveEffect(IGameplayEffectInstance instance)`
  - `Tick(float deltaTime)`

- `IAttributeSystem`
  - 属性値と修飾を管理。
  - `bool TryGetValue(AttributeAsset attribute, out AttributeValue value)`
  - `void SetBaseValue(AttributeAsset attribute, float value)`
  - `void ResetAll()`
  - `void ResetModifiers()`
  - `void ApplyModifier(AttributeAsset attribute, AttributeModifier modifier)`
  - `void UpdateCurrentValues()`

- `IGameplayTagProvider`
  - 所持タグ列挙用。
  - `IEnumerable<GameplayTagAsset> GetOwnedTags()`

#### 主要クラス

- `AttributeSystemCore : IAttributeSystem`
  - 内部に `List<AttributeValue>` と `Dictionary<AttributeAsset,int>` を保持。
  - ベース値・修飾値・現在値の管理を担当。
  - Unity 非依存。`AttributeSystemComponent` などから利用される。

- `AbilitySystemCore : IAbilitySystem`
  - `IAbilitySpec` のリストとレベルを保持。
  - アビリティ付与・剥奪・起動可否チェックを担当。
  - 内部に `IGameplayEffectSystem` と `IAttributeSystem` への参照を持つ。

- `GameplayEffectSystemCore : IGameplayEffectSystem`
  - `IGameplayEffectInstance` のリストを保持。
  - 即時 GE の適用と、継続 GE の Tick / 終了判定を担当。
  - 属性修飾の適用は `IAttributeSystem` 経由で行う。

- `GameplayTagQuery`
  - タグ判定のユーティリティ。
  - `HasAllTags`, `HasNoneTags`, `MatchesRequirements` を提供。
  - `GameplayTagAsset.IsDescendantOf` を利用して階層タグも扱える。

---

### Data レイヤ

**目的**: ScriptableObject やシリアライズ用 struct など、**「データ定義」**を集約する。  
Core レイヤの抽象インターフェースを実装する形で設計する。

- 例: `Runtime/Data`

#### 想定ファイル構成

- `Data/Abilities`
  - `AbstractAbilityScriptableObject` : `ScriptableObject, IAbilityDefinition`
  - `SimpleAbilityScriptableObject`
  - `InitialiseStatsAbilityScriptableObject`

- `Data/Effects`
  - `GameplayEffectAsset` : `ScriptableObject, IGameplayEffectDefinition`
  - `ModifierMagnitudeAsset` : GE のスケーリング定義
  - `ModifierMagnitude/*` : 各種実装
    - `SimpleFloatModifierMagnitude`
    - `AttributeBackedModifierMagnitude`

- `Data/Attributes`
  - `AttributeAsset` : 属性の定義（名前・計算ロジック）
  - `AbstractAttributeEventHandler` : 属性更新時のイベントハンドラ SO

- `Data/Tags`
  - `GameplayTagAsset`
  - `GameplayTagRequireIgnoreContainer`
  - `AbilityTags`
  - `GameplayEffectTags`

#### ポイント

- Data レイヤは **Unity に依存してよい（`ScriptableObject` など）** が、  
  Core の具象クラス（`AbilitySystemCore` や `AbilitySystemCharacter`）は知らないようにする。
- `ScriptableObject` は **`IAbilityDefinition` / `IGameplayEffectDefinition`** など Core の抽象インターフェースを実装する。

---

### Unity レイヤ

**目的**: Unity コンポーネント (`MonoBehaviour`) と Core/ Data を橋渡しする。  
入力、Update ループ、インスペクタからの参照など、Unity 依存の処理を担当する。

- 例: `Runtime/Unity`

#### 想定ファイル構成

- `Unity/Components`
  - `AbilitySystemCharacter`
    - シーン上の 1 体のキャラクターを表すコンポーネント。
    - 内部に `AbilitySystemCore` と `GameplayEffectSystemCore` を保持。
    - `Update()` で
      - 属性修飾のリセット
      - GE の Tick
      - 属性の現在値再計算
      を Core に委譲するだけの薄いラッパを目指す。
  - `AttributeSystemComponent`
    - シーン上の属性を保持するコンポーネント。
    - `AttributeAsset` のリストをシリアライズし、`AttributeSystemCore` を内部に生成。
    - `LateUpdate()` で `UpdateCurrentValues()` を呼び、  
      `AbstractAttributeEventHandler` によるフックを提供する。

#### ポイント

- Unity レイヤは **Core への依存のみを持ち、Data には直接強く依存しない** のが理想。
  - 例: Ability を付与する時は `IAbilityDefinition` を受け取り、`CreateSpec(IAbilitySystem)` で `IAbilitySpec` を生成する。
- `AbilitySystemCharacter` はできる限り「状態を持たないラッパ」として実装し、  
  実際の状態・ロジックは `AbilitySystemCore` / `GameplayEffectSystemCore` / `AttributeSystemCore` に任せる。

---

## 依存関係の方針

- **Core → Data / Unity への依存は禁止**
  - Core は純粋な C# ライブラリとして、任意の環境で再利用可能にする。
- **Data → Core への依存は許可**
  - `ScriptableObject` で Core インターフェースを実装する形。
- **Unity → Core / Data への依存は許可**
  - MonoBehaviour が Core の実装を内部に持ち、Data のアセットを参照する。

依存方向イメージ:

```text
Core    ←    Data
  ↑
  └──── Unity
```

---

## 移行方針（v1 → v2）

1. **Core インターフェースと Core 実装を追加**（済）
   - `IAbilitySystem` / `IAttributeSystem` / `IGameplayEffectSystem` など。
   - `AbilitySystemCore` / `AttributeSystemCore` / `GameplayEffectSystemCore` を用意。

2. **Attribute 周りのロジックを Core に移譲**（済）
   - `AttributeSystemComponent` は `AttributeSystemCore` のラッパとする。

3. **AbilitySystemCharacter の責務分割**
   - アビリティ管理・GE 管理・属性更新をそれぞれ Core へ移譲。
   - `Update()` は Core 呼び出しのみの薄い実装にする。

4. **Ability / GE / Modifier をインターフェース準拠に整理**
   - `AbstractAbilityScriptableObject` → `IAbilityDefinition` 実装。
   - `AbstractAbilitySpec` → `IAbilitySpec` 実装。
   - `GameplayEffectAsset` / `GameplayEffectSpec` → `IGameplayEffectDefinition` / `IGameplayEffectSpec` 実装。
   - ModifierMagnitude 系を `IGameplayEffectSpec` ベースに書き換え。

5. **物理ディレクトリと namespace の整理**
   - Unity エディタ上で `Core / Data / Unity` にフォルダ分割。
   - 各ファイルの namespace を `Nitou.AbilitySystem.Core` / `.Data` / `.Unity.Components` などに揃える。

---

## 補足

- この設計は「どのゲームにも差し込める GAS ライブラリ」を目標としているため、  
  可能な限り **インターフェースベース + Unity 非依存の Core** を優先する。
- 一方で、Unity プロジェクトとしての使い勝手を落とさないため、  
  ScriptableObject ベースの Data 層と、MonoBehaviour ベースの Unity 層を明確に切りつつも、  
  既存のワークフロー（インスペクタでの設定・SO ベースのバランス調整）は維持する方針とする。


