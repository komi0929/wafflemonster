# SOYYA — プロジェクト憲法 (GEMINI.md)

## プロジェクト概要

**世界最高峰の3Dゲーム** — Unity WebGL でブラウザ上に展開

## 技術スタック

- **Engine**: Unity 6 LTS (6000.0.x) — URP (Universal Render Pipeline)
- **Build Target**: WebGL (WebAssembly + WebGPU)
- **Language**: C# (.NET Standard 2.1)
- **Version Control**: Git
- **IDE**: VS Code + Unity Extension
- **Preview Server**: Node.js (Express)
- **Deploy**: GitHub Pages / Vercel

## AI行動規範

### 絶対ルール

1. **日本語で報告** — すべてのコミットメッセージ・コメントは日本語
2. **確認不要** — 最適な判断で自律実行
3. **ビルドゲート** — WebGLビルドが成功してからコミット
4. **テスト駆動** — 重要ロジックにはPlayModeテスト必須

### コーディング規約

- **命名**: PascalCase (public), camelCase (private), `_` prefix for private fields
- **ファイル構成**: 1クラス1ファイル、namespaceは `Soyya.*`
- **MonoBehaviour**: `Awake` > `Start` > `Update` の順序を厳守
- **パフォーマンス**: Object Pooling必須、GC Alloc最小化

### WebGL最適化ルール

- テクスチャ: 2のべき乗サイズ、Crunch圧縮 50%
- メッシュ: LOD 3段階（High/Mid/Low）
- ライティング: ベイク優先、動的ライト最小限
- 初期ビルドサイズ: 10MB以下を目標
- Addressable Assets: 大容量アセットはオンデマンドロード

### Git運用

- **ブランチ戦略**: `main` (安定) / `develop` (開発) / `feature/*`
- **コミット**: 機能単位で短く、日本語メッセージ
- **自動Push**: ビルド成功時に自動コミット＆プッシュ

## ディレクトリ構造

```
soyya/
├── Assets/
│   ├── Scenes/          # Unityシーン
│   ├── Scripts/         # C#スクリプト
│   │   ├── Core/        # ゲームコアロジック
│   │   ├── UI/          # UIコントローラー
│   │   ├── Gameplay/    # ゲームプレイ機能
│   │   └── Utils/       # ユーティリティ
│   ├── Prefabs/         # プレハブ
│   ├── Materials/       # マテリアル
│   ├── Textures/        # テクスチャ
│   ├── Models/          # 3Dモデル
│   ├── Shaders/         # カスタムシェーダー
│   ├── Audio/           # サウンド
│   ├── Animations/      # アニメーション
│   ├── Editor/          # エディタ拡張
│   └── Resources/       # ランタイムロードアセット
├── Packages/            # Unityパッケージ
├── ProjectSettings/     # Unity設定
├── scripts/             # ビルド自動化
├── WebGL-Build/         # ビルド出力 (gitignored)
├── .vscode/             # VS Code設定
├── .agent/workflows/    # AIエージェントワークフロー
└── GEMINI.md            # プロジェクト憲法（このファイル）
```
