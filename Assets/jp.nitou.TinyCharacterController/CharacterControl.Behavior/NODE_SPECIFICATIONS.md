# Unity Behavior ノード仕様検討

## 概要

見下ろし型シューティングゲームを想定したBehaviorノードの仕様を定義します。
設計方針: **選択肢A（ActorActionsレスアプローチ）** - BehaviorノードがTCCのControlコンポーネントを直接操作

## 既存実装

### Actions
- ✅ **NavigateToLocationAction**: MoveNavmeshControlを使った目標地点への移動

---

## 実装優先度の分類

### Phase 1: MVP（最小限の動作確認）

基本的な追跡＆射撃AIが作れる最小セット

### Phase 2: 拡張機能

巡回、カバー、複雑な条件判定など

### Phase 3: 高度な機能

フォーメーション、動的優先度変更、複雑な状態管理

---

## Phase 1: MVP ノード仕様

### 1. Movement Actions

#### 1.1 MoveToTargetAction

**目的**: ターゲットGameObjectに向かって移動

**TCCコンポーネント使用**: MoveControl（または MoveNavmeshControl）

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>  移動するキャラクター
[Target]          BlackboardVariable<GameObject>         追跡対象
[Speed]           BlackboardVariable<float> = 1.0        移動速度（0.0-1.0正規化）
[StoppingDistance] BlackboardVariable<float> = 2.0       停止距離（メートル）
[FaceTarget]      BlackboardVariable<bool> = true        ターゲットを向くか
[ActivePriority]  BlackboardVariable<int> = 1            アクティブ時の優先度
[DeactivePriority] BlackboardVariable<int> = -1          非アクティブ時の優先度
```

**動作**:
1. OnStart: Targetまでの距離チェック。既に範囲内ならSuccess
2. OnUpdate: 毎フレーム距離計算、StoppingDistance以内でSuccess
3. 実装: MoveControl.Move(Vector2 direction * Speed)を呼び出し
4. OnEnd: Priority を DeactivePriority に戻す

**NavigateToLocationActionとの違い**:
- NavigateToLocation: 固定座標（Vector3）への移動、NavMesh経路探索
- MoveToTarget: 動くターゲット（GameObject）への追跡、直線移動

**検討事項**:
- MoveControlとMoveNavmeshControlどちらを使うべきか？
  - **提案**: パラメータで選択可能にする、またはMoveControlのみで実装（シンプル）
- Targetが移動中の場合、動的に追跡できるか？
  - OnUpdate()で毎フレーム方向を再計算する実装にすれば可能

---

#### 1.2 SetMovementDirectionAction

**目的**: 指定した方向へ移動（シンプルな移動制御）

**TCCコンポーネント使用**: MoveControl

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Direction]       BlackboardVariable<Vector2>            移動方向（正規化済み）
[Speed]           BlackboardVariable<float> = 1.0
[Duration]        BlackboardVariable<float> = 0.0        0なら1フレームのみ、>0なら指定秒数
[ActivePriority]  BlackboardVariable<int> = 1
[DeactivePriority] BlackboardVariable<int> = -1
```

**動作**:
- MoveControl.Move(Direction * Speed)を呼び出し
- Duration = 0: OnStart()で1回だけ呼び出してSuccess
- Duration > 0: 指定時間経過までRunning、その後Success

**用途**:
- 固定方向への移動（逃走、回避など）
- 他のActionと組み合わせて複雑な移動パターン作成

**検討事項**:
- Durationの実装方法（Time.timeで経過時間計測）
- 1フレームだけの入力は意味があるか？→ ManualControlとの組み合わせ用途なら有用

---

### 2. Rotation/Look Actions

#### 2.1 LookAtTargetAction

**目的**: ターゲット方向を向く

**TCCコンポーネント使用**: CursorLookControl または ManualTurn

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[TurnSpeed]       BlackboardVariable<int> = 15           回転速度（-1で即座）
[ActivePriority]  BlackboardVariable<int> = 1
[DeactivePriority] BlackboardVariable<int> = -1
```

**動作**:
1. OnStart: ターゲット方向の計算
2. OnUpdate: ManualTurn.TurnAngleを設定
3. ターゲット方向を向いたらSuccess（角度閾値: 5度など）

**検討事項**:
- CursorLookControlは画面座標用、ManualTurnはワールド座標用
  - **提案**: ManualTurnを使用（GameObjectのworld positionから角度計算）
- TurnSpeedの制御方法
  - ManualTurn.TurnSpeed プロパティに設定

---

### 3. Combat Actions

#### 3.1 SetInputAction（汎用入力設定）

**目的**: 任意のActorActions入力を設定（攻撃、ジャンプ、回避等）

**TCCコンポーネント使用**: なし（Blackboardのみ）

**パラメータ**:
```
[InputName]       BlackboardVariable<string>             "Attack1", "Jump", "Dodge"等
[Value]           BlackboardVariable<bool>               true/false
[Duration]        BlackboardVariable<float> = 0.1        入力保持時間
```

**動作**:
- Blackboard変数 `Input_{InputName}` に Valueを設定
- Duration秒後にfalseに戻す

**検討事項**:
- **重要**: このActionは ActorActionsに依存する
  - 選択肢Aの方針（ActorActionsレス）と矛盾するか？
- **代替案**: 攻撃処理を行うControlコンポーネントを直接操作
  - 例: AttackControl.ExecuteAttack() のようなメソッドを呼ぶ
  - しかし、現状TCCにAttackControlは存在しない
- **結論**: このActionは保留。ユーザー側でAttackControl等を実装してから検討

---

#### 3.2 WaitAction

**目的**: 指定時間待機

**TCCコンポーネント使用**: なし

**パラメータ**:
```
[Duration]        BlackboardVariable<float>              待機時間（秒）
```

**動作**:
- OnStart: タイマー開始
- OnUpdate: Duration秒経過でSuccess

**用途**:
- 攻撃後のクールダウン
- 巡回ポイントでの待機
- Sequenceノード内でのタイミング調整

---

### 4. Condition Nodes

#### 4.1 IsGroundedCondition

**目的**: キャラクターが接地しているか判定

**TCCコンポーネント使用**: GroundCheck

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[UseFirmlyCheck]  BlackboardVariable<bool> = false       IsFirmlyOnGroundを使用するか
```

**動作**:
```csharp
public override bool IsTrue()
{
    return UseFirmlyCheck.Value
        ? groundCheck.IsFirmlyOnGround
        : groundCheck.IsOnGround;
}
```

**検討事項**:
- GroundCheckの2つのプロパティ:
  - `IsOnGround`: 通常の接地判定
  - `IsFirmlyOnGround`: より厳格な接地判定（ヒステリシス付き）
- デフォルトはどちらを使うべきか？→ IsOnGroundが一般的

---

#### 4.2 IsInRangeCondition

**目的**: ターゲットが範囲内にいるか判定

**TCCコンポーネント使用**: RangeTargetCheck（オプション）または 自前計算

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[Range]           BlackboardVariable<float>              判定距離（メートル）
[UseXZPlane]      BlackboardVariable<bool> = true        XZ平面距離のみ（Y無視）
```

**動作**:
- CharacterとTargetの距離を計算
- Range以内ならtrue

**検討事項**:
- RangeTargetCheckコンポーネントを使う？
  - 確認必要: RangeTargetCheckの実装を見る必要がある
  - もしシンプルな距離計算なら、自前実装で十分
- UseXZPlane: 見下ろし型ゲームではY座標を無視することが多い

---

#### 4.3 HasLineOfSightCondition

**目的**: ターゲットへの視線が通っているか判定

**TCCコンポーネント使用**: SightCheck（オプション）または Physics.Raycast

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[MaxDistance]     BlackboardVariable<float> = 50.0       最大視認距離
[ObstacleLayer]   BlackboardVariable<LayerMask>          遮蔽物レイヤー
[OriginOffset]    BlackboardVariable<Vector3> = (0,1,0)  視線の原点オフセット
```

**動作**:
1. Character位置 + OriginOffset から Target方向へRaycast
2. 障害物に当たらずTargetに届けばtrue

**検討事項**:
- SightCheckコンポーネントの実装を確認
- 自前のRaycastで十分な可能性
- 視野角（FOV）の判定は必要か？
  - Phase 1では不要、Phase 2で検討

---

#### 4.4 IsFacingTargetCondition

**目的**: ターゲット方向を向いているか判定

**TCCコンポーネント使用**: なし（Transform参照のみ）

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[AngleThreshold]  BlackboardVariable<float> = 10.0       許容角度（度）
```

**動作**:
```csharp
Vector3 toTarget = (Target.position - Character.position).normalized;
float angle = Vector3.Angle(Character.forward, toTarget);
return angle <= AngleThreshold.Value;
```

**検討事項**:
- XZ平面のみで角度計算するか？（見下ろし型用）
  - **提案**: デフォルトでXZ平面のみ計算

---

#### 4.5 CompareDistanceCondition

**目的**: 2つの距離を比較（A-BとA-Cの距離比較など）

**TCCコンポーネント使用**: なし

**パラメータ**:
```
[ObjectA]         BlackboardVariable<GameObject>
[ObjectB]         BlackboardVariable<GameObject>
[ObjectC]         BlackboardVariable<GameObject>
[CompareType]     BlackboardVariable<CompareType>        Closer/Farther
```

CompareType: enum { AIsCloserToB, AIsCloserToC }

**動作**:
- Distance(A, B) と Distance(A, C) を比較
- CompareTypeに応じてtrue/false

**用途**:
- 「最も近い敵を優先攻撃」
- 「プレイヤーより近い味方がいれば撤退」

**検討事項**:
- このConditionの必要性は低いかも（Phase 2で検討）

---

## Phase 2: 拡張機能ノード

### Movement Actions

#### 2.1 PatrolAction

**目的**: 巡回経路を移動

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Waypoints]       BlackboardVariable<Transform[]>        巡回ポイント配列
[Speed]           BlackboardVariable<float> = 1.0
[Loop]            BlackboardVariable<bool> = true        ループするか
[WaypointRadius]  BlackboardVariable<float> = 1.0        到達判定半径
```

**動作**:
1. 現在のwaypointに向かって移動
2. 到達したら次のwaypointへ
3. 最後まで到達したらSuccess（Loopならまた最初から）

**検討事項**:
- Waypoints配列をBlackboard変数で扱えるか？
  - Unity Behaviorの制約確認が必要
  - 代替案: Blackboard変数にPatrolPathコンポーネント参照を渡す

---

#### 2.2 FleeFromTargetAction

**目的**: ターゲットから逃げる

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[FleeDistance]    BlackboardVariable<float> = 10.0       逃げる距離
[Speed]           BlackboardVariable<float> = 1.0
```

**動作**:
- TargetとCharacterを結ぶベクトルの逆方向に移動
- FleeDistance以上離れたらSuccess

---

### Combat Actions

#### 2.3 AimAtTargetAction

**目的**: ターゲットへのエイム方向を計算（リード射撃対応）

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Target]          BlackboardVariable<GameObject>
[PredictMovement] BlackboardVariable<bool> = false       移動予測
[ProjectileSpeed] BlackboardVariable<float> = 20.0       弾速（予測用）
[OutputDirection] BlackboardVariable<Vector3>            出力: エイム方向
```

**動作**:
1. Targetの現在位置 + 速度 * 予測時間 を計算
2. 予測位置への方向をOutputDirectionに書き込み
3. LookAtTargetActionと組み合わせて使用

**検討事項**:
- ターゲットの速度取得方法
  - Rigidbody.velocity
  - または前フレームとの位置差分から計算
- 弾道計算の精度
  - Phase 1では不要、Phase 2で実装

---

### Condition Nodes

#### 2.6 IsHealthBelowCondition

**目的**: 体力が閾値以下か判定

**パラメータ**:
```
[Character]       BlackboardVariable<CharacterSettings>
[Threshold]       BlackboardVariable<float>              閾値（例: 30%）
[UsePercentage]   BlackboardVariable<bool> = true        パーセントか絶対値か
```

**動作**:
- Characterから Health コンポーネント取得
- Health値と閾値を比較

**検討事項**:
- TCCにHealthコンポーネントは存在しない
  - ユーザー側で実装が必要
  - インターフェース `IHealth` を定義して柔軟性確保？
- Phase 2以降で検討

---

## Phase 3: 高度な機能

### 3.1 CalculateFormationPositionAction

**目的**: フォーメーション位置を計算

### 3.2 SetPriorityAction

**目的**: Controlコンポーネントの優先度を動的に変更

### 3.3 WaitUntilGroundedAction

**目的**: 接地するまで待機（タイムアウト付き）

---

## 実装優先順位の提案

### 最優先（Phase 1 MVP）

**Actions**:
1. **MoveToTargetAction** - 追跡の基本
2. **WaitAction** - タイミング制御

**Conditions**:
1. **IsGroundedCondition** - 基本的な状態確認
2. **IsInRangeCondition** - 距離判定の基本
3. **IsFacingTargetCondition** - 向き判定

この5つで「追跡AI」が作れる：
```
Sequence
├─ IsInRange (Range: 10) → プレイヤーが近ければ
├─ MoveToTarget (StoppingDistance: 5)
└─ Wait (Duration: 1.0)
```

---

### 次点（Phase 1追加）

**Actions**:
3. **LookAtTargetAction** - エイム制御

**Conditions**:
4. **HasLineOfSightCondition** - 視線判定

この7つで「追跡＋エイムAI」が作れる：
```
Sequence
├─ IsInRange + HasLineOfSight
├─ MoveToTarget
├─ LookAtTarget
└─ Wait (射撃のタイミング)
```

---

## 検討が必要な事項

### 1. Blackboard変数のスコープ

**質問**:
- NavigateToLocationActionは Character, Location を Blackboard変数で受け取る
- これらの変数は誰が設定する？
  - シーン開始時にBehaviorAgentが設定？
  - 別のActionノードが設定？

**提案**:
- `Self` (CharacterSettings): シーン開始時に固定
- `Target` (GameObject): 動的に変更可能（検知システムが更新）

---

### 2. 攻撃処理の実装方法

**現状の問題**:
- TCCには攻撃を行うControlコンポーネントが存在しない
- ActorActionsに `attack1`, `attack2` は定義されているが、それを消費するコンポーネントがない

**選択肢**:
A. ユーザー側でAttackControlを実装してもらう
B. Behaviorノードから直接攻撃処理（弾丸生成等）を行う
C. Blackboard経由でActorActionsに攻撃入力を設定し、中間レイヤー（例: State機械）が処理

**推奨**:
- Phase 1ではスキップ
- Phase 2でユーザー実装のAttackControlを想定したActionノードを提供

---

### 3. TCCコンポーネントの機能確認

以下のコンポーネントの詳細実装を確認する必要あり:
- **RangeTargetCheck**: 範囲判定の機能
- **SightCheck**: 視線判定の機能
- **ClosestTargetCheck**: 最接近ターゲット検索

確認後、これらを活用するConditionノードの仕様を詰める。

---

## 次のステップ

1. Phase 1 MVP の5ノードの仕様確定
2. TCCの既存Checkコンポーネントの機能確認
3. 実装開始（MoveToTargetAction から）
