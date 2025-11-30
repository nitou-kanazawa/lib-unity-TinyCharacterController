# Unity Behavior Integration for TCC - Design Document

## 概要

このドキュメントは、Tiny Character Controller (TCC) と Unity Behavior パッケージを統合するためのシステム設計を定義します。

見下ろし型シューティングゲームなどのAI実装を想定し、Behaviorグラフから直感的にキャラクターの行動を制御できる仕組みを提供します。

## 設計目標

### 主要目標

1. **Behaviorグラフでキャラクターを制御**: ビジュアルスクリプティングで直感的にAI行動を構築
2. **TCCアーキテクチャとの調和**: 既存のCheck/Control/Effectコンポーネントを活用
3. **ActorActionsベースの入力**: 既存のPlayerBrain/EnemyBrainと同じ入力抽象化を使用
4. **見下ろし型シューター対応**: 移動、エイム、射撃、回避などの典型的な行動をサポート

### 非目標

- Unity Behaviorの完全な置き換え（標準機能を拡張する立場）
- リアルタイム戦略ゲーム用の高度な群集制御
- 複雑なアニメーション同期（Animancerとの連携は別レイヤー）

## アーキテクチャ概要

### 既存システムとの関係

```
Unity Behavior Graph (ビジュアル編集)
    ↓
BehaviorBrain (Unity Behavior実行 + ActorActions出力)
    ↓
ActorBrain基底クラス (ActorActions管理)
    ↓
Control Components (MoveControl, JumpControl, etc.)
    ↓
Brain (CharacterBrain, RigidbodyBrain, etc.)
```

### コンポーネント構成

```
GameObject (AI Character)
├─ BehaviorAgent (Unity標準)
│  └─ BehaviorGraph asset参照
├─ BehaviorBrain (新規: ActorBrainを継承)
│  ├─ ActorActionsを管理
│  └─ BehaviorAgentと連携
├─ CharacterBrain or RigidbodyBrain
├─ Check Components (GroundCheck, RangeTargetCheck, etc.)
└─ Control Components (MoveControl, JumpControl, etc.)
```

## 主要コンポーネント設計

### 1. BehaviorBrain

**役割**: Unity BehaviorとTCCのActorActionsをブリッジする

**継承関係**: `ActorBrain` → `BehaviorBrain`

**責務**:
- BehaviorAgent参照を保持
- Blackboard変数経由でActorActionsを更新
- 既存のPlayerBrain/EnemyBrainと同じインターフェースを提供

**実装方針**:
```csharp
public class BehaviorBrain : ActorBrain
{
    [SerializeField] private BehaviorAgent _behaviorAgent;

    protected override void UpdateBrainValues(float dt)
    {
        // Blackboardから値を取得してActorActionsに反映
        _characterActions.movement.value = GetBlackboardVector2("Movement");
        _characterActions.jump.value = GetBlackboardBool("Jump");
        // ... 他のアクション

        _characterActions.Update(dt);
    }
}
```

### 2. カスタムBehavior Actions（ノード群）

Unity BehaviorグラフでTCCを操作するためのカスタムActionノード群

#### 2.1 Movement Actions

**SetMovementDirectionAction**
- **目的**: 移動方向を設定
- **パラメータ**:
  - `Vector2 direction`: 移動方向（ワールド座標またはカメラ相対）
  - `MovementSpace space`: ワールド/カメラ相対/キャラクター相対
  - `bool normalize`: 正規化するか
- **Blackboard出力**: `Movement (Vector2)`

**MoveToTargetAction**
- **目的**: ターゲットに向かって移動
- **パラメータ**:
  - `GameObject target`: 目標対象
  - `float speed`: 移動速度
  - `float stoppingDistance`: 停止距離
  - `bool faceTarget`: ターゲットを向くか
- **Blackboard出力**: `Movement (Vector2)`, `LookDirection (Vector3)`

**PatrolAction**
- **目的**: 巡回経路を移動
- **パラメータ**:
  - `Transform[] waypoints`: 巡回ポイント配列
  - `float speed`: 移動速度
  - `bool loop`: ループするか
  - `float waypointRadius`: 到達判定半径
- **内部状態**: 現在のwaypointインデックス
- **Blackboard出力**: `Movement (Vector2)`

#### 2.2 Combat Actions

**AimAtTargetAction**
- **目的**: ターゲットへのエイム方向を計算
- **パラメータ**:
  - `GameObject target`: エイム対象
  - `bool predictMovement`: 移動予測（リード射撃）
  - `float projectileSpeed`: 弾速（予測計算用）
- **Blackboard出力**: `AimDirection (Vector3)`, `CanShoot (bool)`

**ShootAction**
- **目的**: 射撃入力を発行
- **パラメータ**:
  - `AttackType attackType`: Attack1/Attack2
  - `float duration`: 長押し時間
- **Blackboard出力**: `Attack1 (bool)` or `Attack2 (bool)`

**DodgeAction**
- **目的**: 回避行動を実行
- **パラメータ**:
  - `Vector2 direction`: 回避方向
- **Blackboard出力**: `Dodge (bool)`, `Movement (Vector2)`

#### 2.3 Utility Actions

**WaitUntilGroundedAction**
- **目的**: 接地するまで待機
- **パラメータ**:
  - `GroundCheck groundCheck`: 参照するGroundCheck
  - `float timeout`: タイムアウト時間
- **戻り値**: Success（接地）/ Failure（タイムアウト）

**SetPriorityAction**
- **目的**: Controlコンポーネントの優先度を動的に変更
- **パラメータ**:
  - `Component targetComponent`: 対象コンポーネント
  - `int priority`: 設定する優先度
- **用途**: 特定の行動中に他の入力を無効化

### 3. カスタムBehavior Conditions（条件ノード群）

#### 3.1 Check Component Conditions

**IsGroundedCondition**
- **パラメータ**: `GroundCheck groundCheck`
- **評価**: `groundCheck.IsOnGround`

**IsInRangeCondition**
- **パラメータ**:
  - `RangeTargetCheck rangeCheck`
  - `GameObject target`
- **評価**: ターゲットが範囲内にいるか

**HasLineOfSightCondition**
- **パラメータ**:
  - `SightCheck sightCheck`
  - `GameObject target`
- **評価**: ターゲットへの視線が通っているか

**IsFacingTargetCondition**
- **パラメータ**:
  - `GameObject self`
  - `GameObject target`
  - `float angleThreshold`: 許容角度（度）
- **評価**: ターゲット方向を向いているか

#### 3.2 State Conditions

**IsActorActionActiveCondition**
- **パラメータ**:
  - `ActorBrain brain`
  - `ActionType actionType`: Jump/Attack/Dodge等
- **評価**: 指定したアクションが現在アクティブか

**HasHighestPriorityCondition**
- **パラメータ**:
  - `Component component`: IMove/ITurnコンポーネント
- **評価**: 指定コンポーネントが最高優先度を持っているか

### 4. Blackboard変数定義

BehaviorBrainとグラフ間でやり取りする標準Blackboard変数

**入力系（Behavior → BehaviorBrain → ActorActions）**:
- `Movement (Vector2)`: 移動入力
- `LookDirection (Vector3)`: 視線方向
- `Jump (bool)`: ジャンプ入力
- `Attack1 (bool)`: 攻撃1入力
- `Attack2 (bool)`: 攻撃2入力
- `Dodge (bool)`: 回避入力
- `Guard (bool)`: ガード入力
- `Run (bool)`: ダッシュ入力

**状態系（TCC → Blackboard）**:
- `Self (GameObject)`: 自身の参照
- `Target (GameObject)`: 現在のターゲット
- `IsGrounded (bool)`: 接地状態
- `CurrentSpeed (float)`: 現在速度
- `DistanceToTarget (float)`: ターゲットまでの距離

## 見下ろし型シューター用のユースケース

### ユースケース1: 基本的な巡回＆追跡AI

**行動フロー**:
1. デフォルト: Patrol (巡回)
2. 条件: プレイヤーが範囲内 → Chase (追跡)
3. 条件: 射撃可能距離 → Combat (戦闘)
4. 条件: プレイヤーロスト → Patrol (巡回に戻る)

**Behaviorグラフ構成**:
```
Sequence
├─ Selector
│  ├─ [Condition: IsInRange(player, combatRange)]
│  │  └─ Combat Sequence
│  │     ├─ AimAtTarget(player)
│  │     ├─ Shoot()
│  │     └─ [Dodge if health < 30%]
│  ├─ [Condition: IsInRange(player, detectRange)]
│  │  └─ MoveToTarget(player, speed: 3, stopDist: 5)
│  └─ Patrol(waypoints, speed: 2)
```

### ユースケース2: カバーシューティング

**行動フロー**:
1. プレイヤー検出 → 最寄りのカバーポイントへ移動
2. カバー到達 → カバー裏で待機
3. 定期的にカバーから出て射撃
4. ダメージ受けたらカバーに戻る

**必要なカスタムアクション**:
- `FindNearestCoverAction`: 最寄りのカバーポイントを検索
- `MoveToPositionAction`: 指定座標へ移動
- `WaitAction`: 指定時間待機
- `PeekAndShootAction`: カバーから顔を出して射撃

### ユースケース3: フォーメーション移動

**行動フロー**:
1. リーダーの位置から自分のフォーメーション位置を計算
2. フォーメーション位置へ移動
3. 敵発見時はフォーメーション維持しながら射撃

**必要なカスタムアクション**:
- `CalculateFormationPositionAction`: フォーメーション位置計算
- `MoveToPositionWithAvoidanceAction`: 障害物回避付き移動

## 実装優先順位

### Phase 1: 基礎実装（MVP）

**必須コンポーネント**:
- ✅ BehaviorBrain
- ✅ SetMovementDirectionAction
- ✅ MoveToTargetAction
- ✅ ShootAction
- ✅ IsGroundedCondition
- ✅ IsInRangeCondition

**検証シナリオ**: 単純な追跡＆射撃AI

### Phase 2: 拡張機能

**追加コンポーネント**:
- PatrolAction
- AimAtTargetAction
- DodgeAction
- HasLineOfSightCondition
- IsFacingTargetCondition

**検証シナリオ**: 巡回＆戦闘AI、カバーシューティング

### Phase 3: 高度な機能

**追加コンポーネント**:
- フォーメーション関連Action
- 動的優先度変更
- 複雑な状態管理

## 技術的考慮事項

### パフォーマンス

- **Blackboard更新頻度**: 毎フレーム vs イベント駆動
  - 推奨: 毎フレーム（ActorBrainのUpdate/FixedUpdateと同期）
- **Check Component参照**: 直接参照 vs Blackboard経由
  - 推奨: 直接参照（型安全性、パフォーマンス）
- **複数AI同時実行**: Unity BehaviorのJobSystem対応は将来的に検討

### エディタ体験

- **カスタムノードのアイコン**: TCCコンポーネントと視覚的に統一
- **ノード説明文**: 各パラメータのTooltipを充実
- **デバッグ機能**: Blackboard変数のリアルタイム監視
- **エラーハンドリング**: コンポーネント未設定時の警告

### 互換性

- **Unity Behavior バージョン**: v1.0.13+ (CLAUDE.mdより)
- **TCC モジュール依存**:
  - CharacterControl: Check/Controlコンポーネントアクセス
  - Integration: ActorBrain/ActorActions
  - Foundation: 基礎ユーティリティ
- **アセンブリ定義**: `defineConstraints: ["ODIN_INSPECTOR"]` 必須

### 制約事項

- **ActorActionsの制限**: 既存の定義済みアクションのみ使用可能
  - カスタムアクション追加は ActorActions 構造体の変更が必要
- **Brainの単一性**: 1つのGameObjectに複数のActorBrainは不可
  - BehaviorBrainとPlayerBrain/EnemyBrainの共存は不可
- **優先度システム**: Behaviorから動的に優先度変更は可能だが、慎重に使用

## セキュリティとエラーハンドリング

### Null参照対策

- すべてのコンポーネント参照でnullチェック実装
- Blackboard変数の存在確認
- Editor上での参照未設定警告

### 無限ループ防止

- Behaviorグラフの実行タイムアウト設定
- 異常検知時の自動停止機能

### デバッグサポート

- Blackboard変数のログ出力オプション
- ActorActionsの現在値を視覚化するカスタムInspector
- Behavior実行トレース機能

## 今後の拡張性

### 考えられる拡張

1. **NavMesh統合**: NavmeshBrainとの連携強化
2. **Animancer連携**: アニメーション制御をBehaviorから実行
3. **GOAP統合**: 既存のGOAPモジュールとの相互運用
4. **マルチエージェント**: グループ行動、コミュニケーション
5. **ユーティリティAI**: Behaviorと組み合わせた意思決定

### プラグイン化

将来的に独立したUPMパッケージとして配布可能な構造を維持
- 依存関係の明確化
- サンプルシーンの提供
- ドキュメント整備

## 参考資料

- Unity Behavior公式ドキュメント: https://docs.unity3d.com/Packages/com.unity.behavior@latest
- TCC CLAUDE.md: プロジェクト固有のアーキテクチャ説明
- ActorBrain実装: `Integration/Runtime/Scripts/ActorBrain.cs`
- EnemyBrain実装: `Integration/Runtime/Scripts/EnemyBrain.cs`
