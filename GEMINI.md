# SOYYA — プロジェクト憲法 (GEMINI.md)

## プロジェクト概要

**WaffleMonster** — 香港/台湾ネオン裏路地を舞台にした3D逃走アクションゲーム
バブルワッフルの球でモンスターの気をそらして逃げ切れ！

## 技術スタック

- **Engine**: Unity 6 LTS (6000.3.8f1) — URP (Universal Render Pipeline)
- **Build Target**: WebGL (WebAssembly)
- **Language**: C# (.NET Standard 2.1)
- **Version Control**: Git
- **IDE**: VS Code + Unity Extension
- **Deploy**: GitHub Pages / Vercel

## AI行動規範

### 絶対ルール

1. **日本語で報告** — すべてのコミットメッセージ・コメントは日本語
2. **確認不要** — 最適な判断で自律実行
3. **ビルドゲート** — WebGLビルドが成功してからコミット
4. **テスト駆動** — 重要ロジックにはPlayModeテスト必須

### コーディング規約

- **命名**: PascalCase (public), camelCase (private), `_` prefix for private fields
- **ファイル構成**: 1クラス1ファイル、namespaceは `Soyya.WaffleMonster`
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

## ゲームアーキテクチャ

### namespace: `Soyya.WaffleMonster`

```
Assets/
├── Scripts/
│   ├── Core/            # GameManager, GameState
│   ├── Gameplay/        # Player, Monster, Waffle, Goal, NeonSign, Lantern
│   ├── UI/              # VirtualJoystick, MobileHUD, ResultScreen
│   └── Utils/           # ObjectPool, AudioManager
├── Editor/              # GameSceneBuilder
├── Scenes/
├── Prefabs/
├── Materials/
├── Textures/
├── Models/
├── Shaders/
├── Audio/
└── Animations/
```

### モンスター仕様

| 名前 | クラス | 速度 | 食事時間 | 特殊能力 |
|---|---|---|---|---|
| Whi-chan | GhostMonster | 遅い | 5秒 | 壁透過検知、浮遊 |
| Mecha-paku | MechaMonster | 速い | 3秒 | 突進加速 |
| Fuzz-nom | FuzzyMonster | 中速 | 4秒 | 360°索敵、仲間呼び |
