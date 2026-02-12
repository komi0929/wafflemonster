---
description: Unity C#スクリプトの追加・編集ワークフロー
---

# /code — C#スクリプト追加・編集

## ルール
- namespace は `Soyya.*` を使用
- PascalCase (public), camelCase (private), `_` prefix for private fields
- 1クラス1ファイル
- MonoBehaviour のライフサイクル順序: Awake > OnEnable > Start > Update > LateUpdate > OnDisable > OnDestroy

## ファイル配置
| カテゴリ | パス |
|---|---|
| ゲームコアロジック | `Assets/Scripts/Core/` |
| UIコントローラー | `Assets/Scripts/UI/` |
| ゲームプレイ機能 | `Assets/Scripts/Gameplay/` |
| ユーティリティ | `Assets/Scripts/Utils/` |
| ScriptableObject | `Assets/Scripts/Data/` |
| カスタムシェーダー | `Assets/Shaders/` |

## ステップ

1. 要件を整理して適切なディレクトリにC#ファイルを作成する

2. namespace、class名、アクセス修飾子を正しく設定する

3. 作成後、WebGL開発ビルドで検証する
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build.ps1 -Dev
```
