# ObjectPool アーキテクチャ設計書

## 📋 概要

このObjectPoolシステムは、UnityのGameObjectを効率的に再利用するための仕組みです。シーン単位でのプール管理、ライフタイム自動管理、Rigidbodyの自動リセットなど、ゲーム開発に必要な機能を備えています。

---

## 🏗️ アーキテクチャ概要

### レイヤー構造

```
┌─────────────────────────────────────┐
│      ユーザーコード層                 │
│  (ゲームロジック、エフェクト制御等)    │
└─────────────────────────────────────┘
              ↓ 使用
┌─────────────────────────────────────┐
│    GameObjectPoolManager (管理層)    │
│  - プールの検索・登録                 │
│  - シングルトン管理                   │
└─────────────────────────────────────┘
              ↓ 管理
┌─────────────────────────────────────┐
│     GameObjectPool (プール層)        │
│  - オブジェクトの生成・再利用          │
│  - プレウォーム                       │
└─────────────────────────────────────┘
              ↓ 管理
┌─────────────────────────────────────┐
│   PooledGameObject (オブジェクト層)   │
│  - 状態管理（使用中/未使用）           │
│  - ライフタイム管理                   │
└─────────────────────────────────────┘
              ↓ 監視
┌─────────────────────────────────────┐
│  PooledGameObjectSystem (システム層)  │
│  - ライフタイムの自動チェック          │
│  - 期限切れオブジェクトの自動解放      │
└─────────────────────────────────────┘
```

---

## 📦 クラス詳細

### 1. インターフェース層

#### IGameObjectPool

**責務：** GameObjectプールの基本操作を定義

| メンバー                  | 型              | 説明                           |
| ------------------------- | --------------- | ------------------------------ |
| `PrefabID`                | `int`           | プレハブを識別するID（検索用） |
| `Scene`                   | `Scene`         | プールが属するシーン           |
| `Get()`                   | `IPooledObject` | プールからオブジェクトを取得   |
| `Get(position, rotation)` | `IPooledObject` | 位置・回転を指定して取得       |
| `Release(obj)`            | `void`          | オブジェクトをプールに返却     |

#### IPooledObject

**責務：** プールされるオブジェクトの基本操作を定義

| メンバー                          | 型           | 説明                     |
| --------------------------------- | ------------ | ------------------------ |
| `GameObject`                      | `GameObject` | 対応するGameObject       |
| `InstanceId`                      | `int`        | インスタンスID（識別用） |
| `IsUsed`                          | `bool`       | 使用中かどうか           |
| `Initialize(owner, hasRigidbody)` | `void`       | 初期化処理               |
| `Release()`                       | `void`       | プールに返却             |
| `OnGet()`                         | `void`       | 取得時のコールバック     |
| `OnRelease()`                     | `void`       | 返却時のコールバック     |

---

### 2. 実装層

#### GameObjectPool

**責務：** 特定のPrefabに対するオブジェクトプールを実装

**主要フィールド：**

| フィールド        | 型                        | 説明                             |
| ----------------- | ------------------------- | -------------------------------- |
| `_prefab`         | `PooledGameObject`        | プール対象のPrefab               |
| `_createdObjects` | `List<PooledGameObject>`  | 生成した全オブジェクト           |
| `_free`           | `Stack<PooledGameObject>` | 未使用オブジェクトのスタック     |
| `_isActiveOnGet`  | `bool`                    | 取得時に自動でアクティブ化するか |
| `_parent`         | `Transform`               | 生成オブジェクトの親Transform    |
| `_prewarmCount`   | `int`                     | 事前生成数                       |

**主要メソッド：**

```csharp
// 静的メソッド（便利API）
public static GameObject Get(PooledGameObject prefab)
public static GameObject Get(PooledGameObject prefab, Vector3 pos, Quaternion rot)
public static void Release(GameObject obj)

// インスタンスメソッド
public GameObject Get()
public GameObject Get(Vector3 position, Quaternion rotation)
public void DestroyUnusedObjects()
public void ReleaseAllInstance()

// 内部処理
private PooledGameObject CreateInstance()
private PooledGameObject GetNewInstance()
private void Initialize()
private void PrewarmProcess()
```

**ライフサイクル：**

```
Awake() → Register to Manager
  ↓
Initialize() → Cache Prefab Info
  ↓
PrewarmProcess() → Create Initial Objects
  ↓
Start() → Set Sibling Index
  ↓
[Runtime] Get/Release Cycles
  ↓
OnDestroy() → Unregister & Cleanup
```

---

#### PooledGameObject

**責務：** プールされるGameObjectの状態管理とライフタイム管理

**主要フィールド：**

| フィールド            | 型                | 説明                         |
| --------------------- | ----------------- | ---------------------------- |
| `_isUseLifetime`      | `bool`            | ライフタイム機能を使用するか |
| `_lifeTime`           | `float`           | 有効期間（秒）               |
| `_owner`              | `IGameObjectPool` | 所属するプール               |
| `_rigidbody`          | `Rigidbody`       | キャッシュされたRigidbody    |
| `OnReleaseByLifeTime` | `UnityEvent`      | 自動解放時のイベント         |

**主要プロパティ：**

```csharp
public bool IsUsed { get; private set; }        // 使用中フラグ
public float ReleaseTime { get; private set; }  // 解放予定時刻
public bool IsPlaying                            // アクティブかつ有効期間内か
```

**状態遷移：**

```
[未使用・非アクティブ]
        ↓ OnGet()
[使用中・アクティブ]
        ↓ OnRelease() or ライフタイム切れ
[未使用・非アクティブ]
```

---

#### GameObjectPoolManager

**責務：** 全GameObjectPoolの管理とプール検索

**設計パターン：** Singleton (ScriptableObject)

**主要フィールド：**

```csharp
private readonly List<IGameObjectPool> _components
```

**主要メソッド：**

```csharp
// 登録・登録解除
public static bool Register(IGameObjectPool pool)
public static void Unregister(IGameObjectPool pool)

// オブジェクト取得（便利API）
public static GameObject Get(IPooledObject prefab)
public static GameObject Get(IPooledObject prefab, Vector3 pos, Quaternion rot)
public static GameObject Get(IPooledObject prefab, Scene scene)

// プール検索
public static IGameObjectPool GetPool(IPooledObject prefab)
public static IGameObjectPool GetPool(IPooledObject prefab, Scene scene)

// プール破棄
public static void DestroyPool(PooledGameObject prefab)
```

**検索ロジック：**

```
PrefabのInstanceId → List検索 → IGameObjectPool
         ↓ (Scene指定あり)
PrefabId + Scene → List検索 → IGameObjectPool
```

---

#### PooledGameObjectSystem

**責務：** ライフタイム機能が有効なオブジェクトの自動監視と解放

**設計パターン：** System (Update駆動のシングルトン)

**処理フロー：**

```csharp
void OnUpdate()
{
    1. GetReleaseObjects()
       → 期限切れオブジェクトを収集
    
    2. InvokeEvents()
       → OnReleaseByLifeTimeイベント発火
    
    3. ReleasePooledObjects()
       → オブジェクトをプールに返却
}
```

**パフォーマンス特性：**

- 毎フレーム全登録オブジェクトをチェック（O(n)）
- オブジェクト数が多い場合は負荷に注意
- **改善提案：** 優先度キューを使った最適化（レビュー#6参照）

---

## 🔄 主要なユースケース

### ケース1: 基本的な取得と解放

```csharp
// 取得
var bullet = GameObjectPool.Get(bulletPrefab, position, rotation);

// 使用
bullet.GetComponent<Bullet>().Fire(direction);

// 解放
GameObjectPool.Release(bullet);
```

### ケース2: ライフタイム自動管理

```csharp
// PooledGameObjectの設定
// - _isUseLifetime = true
// - _lifeTime = 3.0f (3秒後に自動解放)

var effect = GameObjectPool.Get(effectPrefab, position, rotation);
// 3秒後、自動的にプールに返却される
```

### ケース3: Scene単位のプール管理

```csharp
// 特定シーンのプールから取得
var obj = GameObjectPool.Get(prefab, targetScene);

// シーンアンロード時、そのシーンのプールも自動的に破棄される
```

---

## ⚙️ 設定パラメータ

### GameObjectPoolの設定

| パラメータ          | 説明                                   | 推奨値                       |
| ------------------- | -------------------------------------- | ---------------------------- |
| `Prefab`            | プール対象のPrefab（必須）             | -                            |
| `Is Active On Get`  | 取得時に自動アクティブ化               | `true`                       |
| `Parent`            | 生成オブジェクトの親                   | UI以外は未設定推奨           |
| `Prewarm Count`     | 事前生成数                             | 最大同時使用数の50-100%      |
| `Hide Spawn Object` | エディタでスポーンオブジェクトを非表示 | `true` (デバッグ時は`false`) |

### PooledGameObjectの設定

| パラメータ        | 説明                   | 推奨値               |
| ----------------- | ---------------------- | -------------------- |
| `Is Use Lifetime` | ライフタイム機能を使用 | エフェクトなどに有効 |
| `Life Time`       | 有効期間（秒）         | 用途に応じて設定     |

---

## 🎯 パフォーマンス特性

### メモリ使用量

```
1プール当たり:
  - _createdObjects: 12 bytes × オブジェクト数
  - _free: 16 bytes × 未使用オブジェクト数
  - その他フィールド: 約50 bytes

100プール × 平均50オブジェクト = 約60KB
```

### CPU負荷

| 処理                   | 計算量 | 備考             |
| ---------------------- | ------ | ---------------- |
| `Get()`                | O(1)   | Stack.Pop        |
| `Release()`            | O(1)   | Stack.Push       |
| `GetPool()`            | O(n)   | List検索 ⚠️要改善 |
| `ReleaseAllInstance()` | O(n²)  | ⚠️要改善          |
| `ライフタイムチェック` | O(n)   | 毎フレーム実行   |

---

## 🔒 スレッドセーフ性

**現状：** ⚠️ **スレッドセーフではありません**

- すべての操作はメインスレッドから実行する必要があります
- Unity JobSystemから直接使用不可
- 必要に応じてメインスレッドにディスパッチしてください

---

## 📊 Unity標準ObjectPoolとの比較

| 機能                 | 本実装         | Unity標準 |
| -------------------- | -------------- | --------- |
| GameObject対応       | ✅              | ✅         |
| Scene単位管理        | ✅              | ❌         |
| ライフタイム自動管理 | ✅              | ❌         |
| プレウォーム         | ✅              | ✅         |
| 最大数制限           | ❌              | ✅         |
| スレッドセーフ       | ❌              | ✅ (一部)  |
| 依存関係             | Odin Inspector | なし      |

---

## 🐛 既知の問題と改善案

### 高優先度

1. **`ReleaseAllInstance()`のO(n²)問題**
   - HashSetを使った最適化が必要

2. **`GameObjectPoolManager`の検索効率**
   - DictionaryベースのキャッシュV実装推奨

3. **null参照のリスク**
   - Prefabのnullチェック強化が必要

### 中優先度

4. **ライフタイムチェックの効率化**
   - 優先度キューによる最適化を検討

5. **スレッドセーフ化**
   - JobSystem対応の場合は要実装

---

## 🔗 関連リンク

- [Unity公式: Object Pooling](https://learn.unity.com/tutorial/object-pooling)
- [Unity Pool API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Pool.ObjectPool_1.html)
- [元実装: Project_TCC](https://github.com/unity3d-jp/Project_TCC)

---

## 📝 使用例

### 弾丸のプーリング

```csharp
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private PooledGameObject bulletPrefab;
    
    public void SpawnBullet(Vector3 position, Vector3 direction)
    {
        // プールから取得
        var bullet = GameObjectPool.Get(bulletPrefab, position, Quaternion.identity);
        
        // 初期化
        var bulletComponent = bullet.GetComponent<Bullet>();
        bulletComponent.Initialize(direction);
        
        // 3秒後に自動的にプールに返却される（ライフタイム設定済みの場合）
    }
}

public class Bullet : MonoBehaviour
{
    public void Initialize(Vector3 direction)
    {
        GetComponent<Rigidbody>().linearVelocity = direction * 10f;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突時に手動でプールに返却
        GameObjectPool.Release(gameObject);
    }
}
```

### エフェクトのプーリング

```csharp
public class EffectManager : MonoBehaviour
{
    [SerializeField] private PooledGameObject explosionPrefab;
    
    public void PlayExplosion(Vector3 position)
    {
        // エフェクトを取得して再生
        // ライフタイム機能により自動的にプールに返却される
        var effect = GameObjectPool.Get(explosionPrefab, position, Quaternion.identity);
    }
}
```

---

**作成日：** 2025年10月4日  
**バージョン：** 1.0  
**レビュー対象コミット：** main branch