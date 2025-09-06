# Tiny Character Controller

Unity公式サンプルの[Project_TCC](https://github.com/unity3d-jp/Project_TCC?tab=readme-ov-file)を自分用にカスタムしたパッケージ．

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

## 概要

- Tiny Character Controller (TCC) は、キャラクターの挙動を複数の小さなコンポーネントを組み合わせて実現するシステム．


## 特徴

1. Brain
- **キャラクターの最終的な座標を更新する**コンポーネント．
- `Check`、`Effect`、`Control`の結果を集約し、`Transform`に書き込む．
- 移動・ベクトル計算・センサー処理を集中管理する．

2. Check
- **周囲の情報を収集するセンサー**コンポーネント．
- 地面の接地判定、頭上の接触判定、視界判定などを行います。
- 更新時に値をキャッシュし、他コンポーネントへ処理結果を提供する．

3. Control
- プレイヤーの入力に応じて**キャラクターの動きを制御する**コンポーネント．
- 移動、ジャンプ、カメラ制御などのキャラクター操作を管理する．
- 移動方向や移動速度、ジャンプの高さなどを調整する．

4. Effect
- キャラクターに追加の動きや影響を与えるコンポーネント．
- 重力、プラットフォームとの相互作用、追加力(AddForce)などを扱う．
- キャラクターの動きにバリエーションやリアリズムを加える働きをする．

これらのコンポーネントを組み合わせることで、複雑なキャラクター挙動を簡単に構築し、カスタマイズすることが可能となっています。

各コンポーネントは以下のようなネットワークで、キャラクター制御に必要な情報を収集し、座標や動作を更新する処理命令を通知しています。

> <img width=600 src="https://gamemakers.jp/cms/wp-content/uploads/2024/02/99df26f8b79eb80fca049abb780e5122-e1707813799204.jpg">



## セットアップ
#### 要件 / 開発環境
- Unity 6000.0

#### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する
```
https://github.com/nitou-kanazawa/lib-unity-TinyCharacterController.git?path=Assets/TinyCharacterController
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記
```
{
    "dependencies": {
        "com.nitou.tiny-character-controller": "https://github.com/nitou-kanazawa/<リポジトリ名>.git?path=<パッケージパス>"
    }
}
```


## ドキュメント

