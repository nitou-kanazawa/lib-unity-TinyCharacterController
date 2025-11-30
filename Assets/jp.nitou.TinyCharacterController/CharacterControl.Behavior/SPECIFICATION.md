# Unity Behavior Integration - Implementation Specification

## 概要

このドキュメントは、TCC (Tiny Character Controller) と Unity Behavior パッケージ統合の詳細な実装仕様を定義します。

**対象読者**: 実装開発者

**関連ドキュメント**:
- `DESIGN.md`: 設計思想とアーキテクチャ
- `USAGE.md`: 使用方法とサンプル（実装後に作成）

## プロジェクト構成

### ディレクトリ構造

```
Assets/jp.nitou.TinyCharacterController/CharacterControl.Behavior/
├─ Scripts/
│  ├─ Runtime/
│  │  ├─ Core/
│  │  │  ├─ BehaviorBrain.cs              # メインBrainコンポーネント
│  │  │  ├─ BehaviorBlackboardExtensions.cs # Blackboard操作ヘルパー
│  │  │  └─ BehaviorConstants.cs          # 定数定義
│  │  ├─ Actions/
│  │  │  ├─ Movement/
│  │  │  │  ├─ SetMovementDirectionAction.cs
│  │  │  │  ├─ MoveToTargetAction.cs
│  │  │  │  └─ PatrolAction.cs
│  │  │  ├─ Combat/
│  │  │  │  ├─ AimAtTargetAction.cs
│  │  │  │  ├─ ShootAction.cs
│  │  │  │  └─ DodgeAction.cs
│  │  │  └─ Utility/
│  │  │     ├─ WaitUntilGroundedAction.cs
│  │  │     └─ SetPriorityAction.cs
│  │  ├─ Conditions/
│  │  │  ├─ CheckComponent/
│  │  │  │  ├─ IsGroundedCondition.cs
│  │  │  │  ├─ IsInRangeCondition.cs
│  │  │  │  └─ HasLineOfSightCondition.cs
│  │  │  └─ State/
│  │  │     ├─ IsFacingTargetCondition.cs
│  │  │     └─ IsActorActionActiveCondition.cs
│  │  └─ Modifiers/
│  │     └─ (将来的にModifierノード用)
│  ├─ Editor/
│  │  ├─ BehaviorBrainEditor.cs           # カスタムInspector
│  │  └─ BehaviorNodeIcons.cs             # ノードアイコン管理
│  └─ Nitou.TCC.CharacterControl.Behavior.asmdef
├─ Icons/
│  ├─ tcc_behavior_action.png             # Actionノード用アイコン
│  ├─ tcc_behavior_condition.png          # Conditionノード用アイコン
│  └─ (各ノード専用アイコン)
├─ Samples~/
│  ├─ TopDownShooter/
│  │  ├─ Scenes/
│  │  │  └─ TopDownShooterDemo.unity
│  │  ├─ BehaviorGraphs/
│  │  │  ├─ PatrolAndChase.asset
│  │  │  └─ CombatAI.asset
│  │  └─ Prefabs/
│  └─ README.md
├─ DESIGN.md
├─ SPECIFICATION.md (this file)
└─ USAGE.md (実装後に作成)
```

### アセンブリ依存関係

**Nitou.TCC.CharacterControl.Behavior.asmdef**:
```json
{
  "name": "Nitou.TCC.CharacterControl.Behavior",
  "rootNamespace": "Nitou.TCC.CharacterControl.Behavior",
  "references": [
    "Nitou.TCC.Foundation",
    "Nitou.TCC.CharacterControl",
    "Nitou.TCC.Integration",
    "Unity.Behavior",              // Unity Behavior
    "Unity.InputSystem",
    "UniTask",
    "UniRx"
  ],
  "precompiledReferences": [
    "R3.dll",
    "Sirenix.OdinInspector.Attributes.dll"
  ],
  "defineConstraints": [
    "ODIN_INSPECTOR"
  ],
  "versionDefines": [
    {
      "name": "com.nitou.ngizmo",
      "expression": "",
      "define": "TCC_USE_NGIZMOS"
    }
  ]
}
```

## Core実装仕様

### 1. BehaviorBrain

**ファイル**: `Runtime/Core/BehaviorBrain.cs`

**役割**: Unity BehaviorAgentとTCCのActorActionsをブリッジ

#### クラス定義

```csharp
using UnityEngine;
using Unity.Behavior;
using Sirenix.OdinInspector;
using Nitou.TCC.Integration;

namespace Nitou.TCC.CharacterControl.Behavior
{
    /// <summary>
    /// Unity BehaviorとTCCを統合するBrainコンポーネント
    /// </summary>
    [AddComponentMenu(MenuList.MenuBehavior + "Behavior Brain")]
    [DefaultExecutionOrder(int.MinValue)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BehaviorAgent))]
    public class BehaviorBrain : ActorBrain
    {
        // --- Inspector Fields ---

        [TitleGroup("Behavior Settings")]
        [Tooltip("Unity Behavior Agent reference (auto-assigned if null)")]
        [SerializeField, Required, Indent]
        private BehaviorAgent _behaviorAgent;

        [TitleGroup("Behavior Settings")]
        [Tooltip("Auto-update Blackboard variables from ActorActions")]
        [SerializeField, Indent]
        private bool _autoUpdateBlackboard = true;

        [TitleGroup("Debug")]
        [Tooltip("Log Blackboard variable updates for debugging")]
        [SerializeField, Indent]
        private bool _debugLogBlackboard = false;


        // --- Private Fields ---

        private BlackboardReference _blackboard;
        private bool _isInitialized = false;


        // --- Properties ---

        /// <summary>
        /// BehaviorAgentへの参照
        /// </summary>
        public BehaviorAgent Agent => _behaviorAgent;

        /// <summary>
        /// Blackboardへの参照
        /// </summary>
        public BlackboardReference Blackboard => _blackboard;


        // --- MonoBehaviour Lifecycle ---

        protected override void Awake()
        {
            base.Awake();
            InitializeBehaviorAgent();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isInitialized)
            {
                _behaviorAgent?.Start();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _behaviorAgent?.End();
        }


        // --- ActorBrain Override ---

        protected override void UpdateBrainValues(float dt)
        {
            if (!_isInitialized) return;

            // Blackboard → ActorActions (Behaviorグラフからの入力を受け取る)
            ReadBlackboardToActions();

            // ActorActionsの更新
            _characterActions.Update(dt);

            // ActorActions → Blackboard (オプション: 状態のフィードバック)
            if (_autoUpdateBlackboard)
            {
                WriteActionsToBlackboard();
            }
        }


        // --- Private Methods ---

        private void InitializeBehaviorAgent()
        {
            // Auto-assign BehaviorAgent
            if (_behaviorAgent == null)
            {
                _behaviorAgent = GetComponent<BehaviorAgent>();
            }

            if (_behaviorAgent == null)
            {
                Debug.LogError($"[BehaviorBrain] BehaviorAgent not found on {gameObject.name}");
                return;
            }

            // Get Blackboard reference
            _blackboard = _behaviorAgent.GetBlackboardReference();
            if (_blackboard == null)
            {
                Debug.LogError($"[BehaviorBrain] Blackboard not found on BehaviorAgent");
                return;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Blackboard変数からActorActionsへ値をコピー
        /// </summary>
        private void ReadBlackboardToActions()
        {
            // Movement
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Movement, out Vector2 movement))
            {
                _characterActions.movement.value = movement;
            }

            // Jump
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Jump, out bool jump))
            {
                _characterActions.jump.value = jump;
            }

            // Attack1
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Attack1, out bool attack1))
            {
                _characterActions.attack1.value = attack1;
            }

            // Attack2
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Attack2, out bool attack2))
            {
                _characterActions.attack2.value = attack2;
            }

            // Dodge
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Dodge, out bool dodge))
            {
                _characterActions.dodge.value = dodge;
            }

            // Guard
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Guard, out bool guard))
            {
                _characterActions.guard.value = guard;
            }

            // Run
            if (_blackboard.TryGetVariableValue(BehaviorConstants.BB_Run, out bool run))
            {
                _characterActions.run.value = run;
            }

            if (_debugLogBlackboard)
            {
                Debug.Log($"[BehaviorBrain] Read from Blackboard: Movement={movement}, Jump={jump}");
            }
        }

        /// <summary>
        /// ActorActionsからBlackboardへ状態をフィードバック（オプション）
        /// </summary>
        private void WriteActionsToBlackboard()
        {
            // 現在のアクション状態をBlackboardに書き戻す
            // Behaviorグラフ側で現在の入力状態を条件判定に使用可能

            _blackboard.SetVariableValue(BehaviorConstants.BB_IsJumping, _characterActions.jump.isBeingHeld);
            _blackboard.SetVariableValue(BehaviorConstants.BB_IsAttacking,
                _characterActions.attack1.isBeingHeld || _characterActions.attack2.isBeingHeld);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_behaviorAgent == null)
            {
                _behaviorAgent = GetComponent<BehaviorAgent>();
            }
        }
#endif
    }
}
```

### 2. BehaviorConstants

**ファイル**: `Runtime/Core/BehaviorConstants.cs`

**役割**: Blackboard変数名などの定数を一元管理

```csharp
namespace Nitou.TCC.CharacterControl.Behavior
{
    /// <summary>
    /// Unity Behavior統合で使用する定数定義
    /// </summary>
    public static class BehaviorConstants
    {
        // --- Blackboard Variable Names (Input) ---

        /// <summary>移動入力 (Vector2)</summary>
        public const string BB_Movement = "Movement";

        /// <summary>視線方向 (Vector3)</summary>
        public const string BB_LookDirection = "LookDirection";

        /// <summary>ジャンプ入力 (bool)</summary>
        public const string BB_Jump = "Jump";

        /// <summary>攻撃1入力 (bool)</summary>
        public const string BB_Attack1 = "Attack1";

        /// <summary>攻撃2入力 (bool)</summary>
        public const string BB_Attack2 = "Attack2";

        /// <summary>回避入力 (bool)</summary>
        public const string BB_Dodge = "Dodge";

        /// <summary>ガード入力 (bool)</summary>
        public const string BB_Guard = "Guard";

        /// <summary>ダッシュ入力 (bool)</summary>
        public const string BB_Run = "Run";


        // --- Blackboard Variable Names (State) ---

        /// <summary>自身のGameObject (GameObject)</summary>
        public const string BB_Self = "Self";

        /// <summary>現在のターゲット (GameObject)</summary>
        public const string BB_Target = "Target";

        /// <summary>接地状態 (bool)</summary>
        public const string BB_IsGrounded = "IsGrounded";

        /// <summary>ジャンプ中か (bool)</summary>
        public const string BB_IsJumping = "IsJumping";

        /// <summary>攻撃中か (bool)</summary>
        public const string BB_IsAttacking = "IsAttacking";

        /// <summary>現在速度 (float)</summary>
        public const string BB_CurrentSpeed = "CurrentSpeed";

        /// <summary>ターゲットまでの距離 (float)</summary>
        public const string BB_DistanceToTarget = "DistanceToTarget";


        // --- Menu Paths ---

        public const string MenuActionMovement = "TCC/Actions/Movement/";
        public const string MenuActionCombat = "TCC/Actions/Combat/";
        public const string MenuActionUtility = "TCC/Actions/Utility/";
        public const string MenuConditionCheck = "TCC/Conditions/Check/";
        public const string MenuConditionState = "TCC/Conditions/State/";
    }
}
```

### 3. BehaviorBlackboardExtensions

**ファイル**: `Runtime/Core/BehaviorBlackboardExtensions.cs`

**役割**: Blackboard操作を簡潔にするヘルパーメソッド

```csharp
using Unity.Behavior;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Behavior
{
    /// <summary>
    /// Blackboard操作の拡張メソッド
    /// </summary>
    public static class BehaviorBlackboardExtensions
    {
        /// <summary>
        /// Blackboard変数の取得を試みる（存在しない場合はデフォルト値）
        /// </summary>
        public static bool TryGetVariableValue<T>(this BlackboardReference blackboard, string variableName, out T value)
        {
            if (blackboard == null)
            {
                value = default;
                return false;
            }

            var variable = blackboard.GetVariable<T>(variableName);
            if (variable != null)
            {
                value = variable.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Blackboard変数の設定（存在しない場合は何もしない）
        /// </summary>
        public static void SetVariableValue<T>(this BlackboardReference blackboard, string variableName, T value)
        {
            if (blackboard == null) return;

            var variable = blackboard.GetVariable<T>(variableName);
            if (variable != null)
            {
                variable.Value = value;
            }
        }

        /// <summary>
        /// Blackboard変数が存在するかチェック
        /// </summary>
        public static bool HasVariable(this BlackboardReference blackboard, string variableName)
        {
            return blackboard?.GetVariable<object>(variableName) != null;
        }
    }
}
```

## Actions実装仕様

### Unity Behavior Actionノードの基本構造

すべてのActionノードは `Unity.Behavior.Action` を継承し、以下のパターンに従います：

```csharp
using Unity.Behavior;
using UnityEngine;

[NodeDescription(
    name: "Action Name",
    story: "[Agent] does something with [Parameter]",
    category: "TCC/Actions/Movement",
    id: "unique-guid-here"
)]
public class ExampleAction : Action
{
    [SerializeField]
    [Tooltip("Description of parameter")]
    private GameObject agent;

    protected override Status OnStart()
    {
        // 初期化処理
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 毎フレームの処理
        // Success, Failure, Running を返す
        return Status.Success;
    }

    protected override void OnEnd()
    {
        // 終了処理
    }
}
```

### Phase 1実装: MoveToTargetAction

**ファイル**: `Runtime/Actions/Movement/MoveToTargetAction.cs`

```csharp
using Unity.Behavior;
using UnityEngine;
using Nitou.TCC.CharacterControl.Check;

namespace Nitou.TCC.CharacterControl.Behavior.Actions
{
    [NodeDescription(
        name: "Move To Target",
        story: "[Agent] moves to [Target] at speed [Speed] stopping at [StoppingDistance]",
        category: "TCC/Actions/Movement",
        id: "tcc_move_to_target_001"
    )]
    public class MoveToTargetAction : Action
    {
        [SerializeField]
        [Tooltip("The character GameObject (should have BehaviorBrain)")]
        private GameObject agent;

        [SerializeField]
        [Tooltip("Target to move towards")]
        private GameObject target;

        [SerializeField]
        [Tooltip("Movement speed (0-1 normalized)")]
        private float speed = 1f;

        [SerializeField]
        [Tooltip("Distance to stop before reaching target")]
        private float stoppingDistance = 1f;

        [SerializeField]
        [Tooltip("Face towards target while moving")]
        private bool faceTarget = true;


        private BlackboardReference _blackboard;
        private Transform _agentTransform;
        private Transform _targetTransform;


        protected override Status OnStart()
        {
            // Validation
            if (agent == null || target == null)
            {
                Debug.LogError("[MoveToTargetAction] Agent or Target is null");
                return Status.Failure;
            }

            _agentTransform = agent.transform;
            _targetTransform = target.transform;
            _blackboard = GetBlackboardReference();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Calculate direction to target
            Vector3 directionToTarget = _targetTransform.position - _agentTransform.position;
            float distance = directionToTarget.magnitude;

            // Check if reached
            if (distance <= stoppingDistance)
            {
                // Stop movement
                SetMovementInput(Vector2.zero);
                return Status.Success;
            }

            // Calculate movement direction (XZ plane)
            Vector3 moveDirection = directionToTarget.normalized;
            Vector2 movement = new Vector2(moveDirection.x, moveDirection.z) * speed;

            // Set movement input to Blackboard
            SetMovementInput(movement);

            // Optional: Set look direction
            if (faceTarget)
            {
                _blackboard.SetVariableValue(BehaviorConstants.BB_LookDirection, moveDirection);
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Ensure movement stops when action ends
            SetMovementInput(Vector2.zero);
        }


        private void SetMovementInput(Vector2 movement)
        {
            _blackboard?.SetVariableValue(BehaviorConstants.BB_Movement, movement);
        }
    }
}
```

### Phase 1実装: ShootAction

**ファイル**: `Runtime/Actions/Combat/ShootAction.cs`

```csharp
using Unity.Behavior;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Behavior.Actions
{
    public enum AttackType
    {
        Attack1,
        Attack2
    }

    [NodeDescription(
        name: "Shoot",
        story: "[Agent] shoots using [AttackType] for [Duration] seconds",
        category: "TCC/Actions/Combat",
        id: "tcc_shoot_001"
    )]
    public class ShootAction : Action
    {
        [SerializeField] private GameObject agent;
        [SerializeField] private AttackType attackType = AttackType.Attack1;
        [SerializeField] private float duration = 0.1f;

        private BlackboardReference _blackboard;
        private float _timer;


        protected override Status OnStart()
        {
            if (agent == null)
            {
                Debug.LogError("[ShootAction] Agent is null");
                return Status.Failure;
            }

            _blackboard = GetBlackboardReference();
            _timer = 0f;

            // Set attack input
            string attackVariable = attackType == AttackType.Attack1
                ? BehaviorConstants.BB_Attack1
                : BehaviorConstants.BB_Attack2;

            _blackboard?.SetVariableValue(attackVariable, true);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_timer >= duration)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Release attack input
            string attackVariable = attackType == AttackType.Attack1
                ? BehaviorConstants.BB_Attack1
                : BehaviorConstants.BB_Attack2;

            _blackboard?.SetVariableValue(attackVariable, false);
        }
    }
}
```

## Conditions実装仕様

### Unity Behavior Conditionノードの基本構造

```csharp
using Unity.Behavior;
using UnityEngine;

[NodeDescription(
    name: "Condition Name",
    story: "[Agent] checks if something",
    category: "TCC/Conditions/Check",
    id: "unique-guid-here"
)]
public class ExampleCondition : Condition
{
    [SerializeField] private GameObject agent;

    public override bool IsTrue()
    {
        // 条件評価
        return true;
    }
}
```

### Phase 1実装: IsGroundedCondition

**ファイル**: `Runtime/Conditions/CheckComponent/IsGroundedCondition.cs`

```csharp
using Unity.Behavior;
using UnityEngine;
using Nitou.TCC.CharacterControl.Check;

namespace Nitou.TCC.CharacterControl.Behavior.Conditions
{
    [NodeDescription(
        name: "Is Grounded",
        story: "[Agent] is grounded (uses [GroundCheck])",
        category: "TCC/Conditions/Check",
        id: "tcc_is_grounded_001"
    )]
    public class IsGroundedCondition : Condition
    {
        [SerializeField]
        [Tooltip("The character GameObject")]
        private GameObject agent;

        [SerializeField]
        [Tooltip("Optional: Specific GroundCheck component (auto-find if null)")]
        private GroundCheck groundCheck;


        private bool _initialized = false;


        public override bool IsTrue()
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (groundCheck == null)
            {
                Debug.LogWarning("[IsGroundedCondition] GroundCheck not found");
                return false;
            }

            return groundCheck.IsOnGround;
        }


        private void Initialize()
        {
            if (agent != null && groundCheck == null)
            {
                groundCheck = agent.GetComponent<GroundCheck>();
            }

            _initialized = true;
        }
    }
}
```

### Phase 1実装: IsInRangeCondition

**ファイル**: `Runtime/Conditions/CheckComponent/IsInRangeCondition.cs`

```csharp
using Unity.Behavior;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Behavior.Conditions
{
    [NodeDescription(
        name: "Is In Range",
        story: "[Agent] is within [Range] of [Target]",
        category: "TCC/Conditions/Check",
        id: "tcc_is_in_range_001"
    )]
    public class IsInRangeCondition : Condition
    {
        [SerializeField] private GameObject agent;
        [SerializeField] private GameObject target;
        [SerializeField] private float range = 5f;


        public override bool IsTrue()
        {
            if (agent == null || target == null)
            {
                return false;
            }

            float distance = Vector3.Distance(agent.transform.position, target.transform.position);
            return distance <= range;
        }
    }
}
```

## Editor拡張仕様

### BehaviorBrainEditor

**ファイル**: `Editor/BehaviorBrainEditor.cs`

**機能**:
- BehaviorBrainのカスタムInspector
- Blackboard変数の現在値リアルタイム表示
- ActorActionsの状態可視化
- BehaviorAgent実行状態の表示

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nitou.TCC.CharacterControl.Behavior;

namespace Nitou.TCC.CharacterControl.Behavior.Editor
{
    [CustomEditor(typeof(BehaviorBrain))]
    public class BehaviorBrainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);

            var brain = target as BehaviorBrain;
            if (brain == null) return;

            // Blackboard状態表示
            DrawBlackboardStatus(brain);

            // ActorActions状態表示
            DrawActorActionsStatus(brain);

            // 自動更新
            Repaint();
        }

        private void DrawBlackboardStatus(BehaviorBrain brain)
        {
            EditorGUILayout.LabelField("Blackboard Variables:");
            EditorGUI.indentLevel++;

            if (brain.Blackboard != null)
            {
                // Movement
                if (brain.Blackboard.TryGetVariableValue(BehaviorConstants.BB_Movement, out Vector2 movement))
                {
                    EditorGUILayout.Vector2Field("Movement", movement);
                }

                // IsGrounded
                if (brain.Blackboard.TryGetVariableValue(BehaviorConstants.BB_IsGrounded, out bool isGrounded))
                {
                    EditorGUILayout.Toggle("Is Grounded", isGrounded);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawActorActionsStatus(BehaviorBrain brain)
        {
            EditorGUILayout.LabelField("Actor Actions:");
            EditorGUI.indentLevel++;

            var actions = brain.CharacterActions;
            EditorGUILayout.Vector2Field("Movement", actions.movement.value);
            EditorGUILayout.Toggle("Jump", actions.jump.value);
            EditorGUILayout.Toggle("Attack1", actions.attack1.value);

            EditorGUI.indentLevel--;
        }
    }
}
#endif
```

## テスト仕様

### ユニットテスト（将来的に）

**テスト対象**:
- BehaviorBrain初期化
- Blackboard変数の読み書き
- ActorActionsへの値変換

**テストフレームワーク**: Unity Test Framework (UTF)

### 統合テスト（サンプルシーン）

**シーン**: `Samples~/TopDownShooter/Scenes/TopDownShooterDemo.unity`

**テストシナリオ**:
1. **基本移動テスト**:
   - MoveToTargetActionでターゲットまで移動
   - stoppingDistanceで停止確認

2. **射撃テスト**:
   - IsInRangeCondition → ShootActionの動作確認
   - 攻撃入力がActorActionsに正しく伝わるか

3. **接地判定テスト**:
   - IsGroundedConditionとGroundCheckの連携確認

## パフォーマンス要件

- **Blackboard更新オーバーヘッド**: 1キャラあたり < 0.1ms
- **Actionノード実行**: 1ノードあたり < 0.05ms
- **同時実行AI数**: 20体以上でフレームレート60fps維持

## 命名規約

### クラス名
- Actionノード: `{動詞}{対象}Action` (例: MoveToTargetAction)
- Conditionノード: `{Is/Has}{状態}Condition` (例: IsGroundedCondition)

### Blackboard変数名
- Input系: PascalCase (Movement, Jump, Attack1)
- State系: Is/Has + PascalCase (IsGrounded, HasTarget)

### ファイル名
- クラス名と一致

## エラーハンドリング規約

### Null参照
- すべてのコンポーネント参照でnullチェック
- nullの場合はDebug.LogErrorまたはDebug.LogWarning

### Blackboard変数不在
- TryGetVariableValueでfalse時はデフォルト値使用
- 警告ログ出力（初回のみ）

### Action失敗時
- Status.Failureを返す
- Debug.LogErrorでエラー内容を記録

## ドキュメント要件

### コード内ドキュメント
- すべてのpublicメソッドにXMLコメント
- 複雑なロジックにはインラインコメント

### サンプルドキュメント
- 各サンプルシーンにREADME.md
- BehaviorGraphのスクリーンショット付き説明

## バージョニング

**初期バージョン**: v0.1.0 (Phase 1完了時)

**セマンティックバージョニング**:
- MAJOR: 破壊的変更
- MINOR: 新機能追加（後方互換性あり）
- PATCH: バグフィックス

## 今後の実装タスク

### Phase 1 (MVP)
- [x] 仕様策定
- [ ] BehaviorBrain実装
- [ ] MoveToTargetAction実装
- [ ] ShootAction実装
- [ ] IsGroundedCondition実装
- [ ] IsInRangeCondition実装
- [ ] サンプルシーン作成

### Phase 2
- [ ] PatrolAction実装
- [ ] AimAtTargetAction実装
- [ ] その他Conditionノード実装
- [ ] Editor拡張強化

### Phase 3
- [ ] 高度なアクション実装
- [ ] パフォーマンス最適化
- [ ] ドキュメント整備
