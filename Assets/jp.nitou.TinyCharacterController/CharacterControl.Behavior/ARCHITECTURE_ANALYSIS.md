# TCCの入力システムアーキテクチャ分析

## 現状の入力フロー

### プレイヤーの場合

```
InputSystem (Unity Input System)
  ↓
InputSystemHandler.SetValues()
  ↓
ActorBrain.UpdateBrainValues()
  ↓ ActorActions.SetValues(InputHandler)
  ↓
ActorActions (構造体) ← movement, jump, attack1, etc.
  ↓
[中間レイヤー: Demoの場合はStateマシン]
  ↓ Brain.CharacterActions.movement.value を取得
  ↓ MoveControl.Move(movement) を呼び出し
  ↓
MoveControl
  ├─ Move(Vector2) メソッドで _inputValue に格納
  └─ OnUpdate() で _inputValue を使用して移動処理
  ↓
BrainBase (MoveManager が最高優先度のIMoveを選択)
```

### 敵AIの場合（現状）

```
EnemyBehaviour (継承: EnemyBehaviourBase)
  ↓ Update()
  ↓ SetMovement() 呼び出し（※実装未完成）
  ↓
EnemyBrain.UpdateBrainValues()
  ↓ ActorActions.SetValues(EnemyBehaviourBase.CharacterActions)
  ↓
ActorActions
  ↓
[中間レイヤー: 未実装]
  ↓
MoveControl.Move()
```

**問題点**: EnemyBehaviourBase.SetMovement()は未実装（TODOコメント）

## 重要な発見

### 1. Controlコンポーネントは入力を直接取得しない

**MoveControl.cs**:
```csharp
// ❌ ActorActionsへの直接参照なし
// ⭕ 外部からMove(Vector2)メソッドで値を受け取る

public void Move(Vector2 leftStick)
{
    _inputValue = leftStick;
    _hasInput = leftStick.sqrMagnitude > 0;
}
```

**CursorLookControl.cs**:
- 同様にActorActionsを参照せず
- `LookTargetPoint(Vector3 screenPosition)` メソッドで外部から値を受け取る

### 2. ActorActionsは入力値のコンテナ

**ActorActions.cs**:
```csharp
[Serializable]
public struct ActorActions
{
    public BoolAction jump;
    public BoolAction attack1;
    public Vector2Action movement;
    // ...
}
```

- 値を保持するだけの構造体
- PlayerBrain/EnemyBrainが更新
- **外部（Stateマシン等）から読み取り可能**

### 3. 中間レイヤーの存在

**Demoの実装（StrafeMoveState.cs）**:
```csharp
public override void UpdateBehaviour(float dt)
{
    // ActorActionsから値を取得
    MoveControl.Move(InputActions.movement.value);

    // CursorLookControlに直接指示
    _cursorLookControl.LookTargetPoint(screenPosition: Input.mousePosition);
}
```

この中間レイヤー（Stateマシン）が：
- ActorActionsから入力値を読み取り
- Controlコンポーネントのメソッドを呼び出す

## ご指摘の問題点への回答

### 問題1: 入力システムの二重性

> 入力を取る部分としてControlコンポーネントとActorBrainの2つがある

**回答**: これは誤解でした。**正確には**：
- **ActorBrain**: 入力値の提供者（ActorActionsに値をセット）
- **Controlコンポーネント**: 入力値の消費者（Move()等のメソッドで受け取る）
- **中間レイヤー**: 橋渡し（ActorActions → MoveControl.Move()）

**懸念点**:
- ActorActionsAssetは名前リストのみ保持（自動生成用）
- 実際の値は ActorActions構造体 に保持
- この設計でBehaviorを組み込む余地はある

### 問題2: 手続き的な記述にならないか

> BehaviorTreeが手続き的（imperative）にならないか

**現状の分析**:
```
[手続き的] Blackboard.Movement = direction
           ↓
           ActorActions.movement = Blackboard.Movement
           ↓
           MoveControl.Move(ActorActions.movement)
```

**代替案（宣言的）**:
```
[宣言的] MoveToTargetAction
           └─ 内部で直接 MoveControl.Move() を呼ぶ
           └─ Blackboard経由せず、直接Controlを操作
```

**重要な洞察**:
- 現状のDemoも手続き的（StrafeMoveState内で逐次的に処理）
- ActorActionsを経由する必要性は低い可能性
- **BehaviorノードがControlコンポーネントを直接操作する方が自然**

### 問題3: Blackboard経由の必要性

> GroundCheck.IsGroundを見に行くConditionノードを作るより、Blackboardに代入して標準Conditionで見る方が優れているか？

**一般的なBehavior Tree設計**:
- **Blackboard中心**: Unity BehaviorやUnreal Engineの標準スタイル
  - 利点: データの一元管理、デバッグ容易、グラフエディタで可視化
  - 欠点: 間接参照のオーバーヘッド、型安全性の低下

- **直接参照**: コンポーネント指向アーキテクチャ
  - 利点: 高速、型安全、TCCの設計哲学と一致
  - 欠点: Blackboardの汎用性を失う

**TCCの場合の推奨**:
```csharp
// ❌ 非推奨: Blackboard経由
IsConditionTrue(Blackboard.GetBool("IsGrounded"))

// ⭕ 推奨: 直接参照
IsGroundedCondition(groundCheck: GroundCheckコンポーネント)
  → groundCheck.IsOnGround を直接参照
```

**理由**:
1. TCCはコンポーネント指向設計
2. GroundCheckはすでに公開プロパティ `IsOnGround` を持つ
3. Blackboardに複製する意味がない
4. パフォーマンス向上

## 修正された設計方針

### 提案A: ActorActionsレスアプローチ（推奨）

**特徴**: BehaviorノードがControlコンポーネントを直接操作

```
Behavior Graph
  ↓
MoveToTargetAction
  ├─ ターゲット位置を計算
  └─ MoveControl.Move(direction) を直接呼び出し

IsGroundedCondition
  └─ GroundCheck.IsOnGround を直接参照
```

**利点**:
- ActorActionsを経由しない → シンプル
- Blackboard最小限 → 宣言的な記述
- TCCのコンポーネント設計と調和

**欠点**:
- ActorActionsとの互換性なし
- PlayerBrain/EnemyBrainとの共通化不可

### 提案B: ハイブリッドアプローチ

**特徴**: 重要な入力のみActorActionsを経由

```
Behavior Graph
  ↓
MoveToTargetAction
  ├─ Blackboard.Movement に値を設定
  └─ BehaviorBrainが ActorActions.movement に反映

BehaviorBrain.UpdateBrainValues()
  ↓
[外部の中間レイヤー]
  ↓ ActorActions.movement を取得
  ↓ MoveControl.Move(movement)
```

**利点**:
- PlayerBrain/EnemyBrainとの共通化可能
- 既存のActorActions基盤を活用

**欠点**:
- 手続き的になりやすい
- 中間レイヤーが必要（誰がMoveControl.Move()を呼ぶ？）

### 提案C: BehaviorBrainが中間レイヤーを内蔵

**特徴**: BehaviorBrainがActorActions → Controlの橋渡しを自動化

```csharp
class BehaviorBrain : ActorBrain
{
    [SerializeField] private MoveControl _moveControl;

    protected override void UpdateBrainValues(float dt)
    {
        // Blackboard → ActorActions
        ReadBlackboardToActions();

        // ActorActions → Control (自動)
        _moveControl.Move(_characterActions.movement.value);
    }
}
```

**利点**:
- ActorActionsとの互換性維持
- Behaviorグラフは宣言的に記述可能
- 自動化されたブリッジ

**欠点**:
- BehaviorBrainがControlへの参照を保持（密結合）
- 柔軟性低下

## 結論と推奨

### ユーザーの懸念への最終回答

1. **入力システムの複雑性**:
   - ActorBrainは入力提供者、Controlは消費者という明確な役割分担
   - 問題は中間レイヤーの不在（誰がMoveControl.Move()を呼ぶか）

2. **手続き的な記述**:
   - ActorActionsを経由すると確かに手続き的になる
   - **推奨**: BehaviorノードがControlを直接操作する方が宣言的

3. **Blackboard経由の必要性**:
   - TCCのコンポーネント指向設計では直接参照が推奨
   - Blackboardは最小限（Self, Target等のコンテキスト情報のみ）

### 最終推奨設計

**「提案A: ActorActionsレスアプローチ」を推奨**

理由:
- TCCの設計哲学（コンポーネント指向）と一致
- Behaviorグラフが宣言的に記述可能
- オーバーヘッド最小

実装例:
```csharp
// MoveToTargetAction
[NodeDescription(name: "Move To Target", ...)]
public class MoveToTargetAction : Action
{
    [SerializeField] private MoveControl moveControl; // 直接参照
    [SerializeField] private GameObject target;

    protected override Status OnUpdate()
    {
        Vector3 direction = (target.position - moveControl.transform.position).normalized;
        moveControl.Move(new Vector2(direction.x, direction.z));
        return Status.Running;
    }
}

// IsGroundedCondition
public class IsGroundedCondition : Condition
{
    [SerializeField] private GroundCheck groundCheck; // 直接参照

    public override bool IsTrue() => groundCheck.IsOnGround;
}
```

**Blackboardの役割**:
- Self, Target などのコンテキスト情報の共有
- 複数ノード間でのデータ受け渡し（必要な場合のみ）
