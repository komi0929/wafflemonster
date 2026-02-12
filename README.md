# SOYYA — Unity WebGL 3Dゲーム

世界最高峰のブラウザ3Dゲーム

## 技術スタック
- **Engine**: Unity 6 LTS (URP)
- **Target**: WebGL (WebAssembly)
- **Language**: C#

## セットアップ

### 前提条件
- Unity Hub 3.15+
- Unity 6 LTS (WebGLモジュール)
- Node.js 24+
- Git 2.52+

### ビルド
```powershell
# 本番ビルド
.\scripts\build.ps1

# 開発ビルド（高速）
.\scripts\build.ps1 -Dev

# ビルド → プレビュー
.\scripts\build.ps1 -Serve

# ビルド → Git自動コミット
.\scripts\build.ps1 -Commit
```

### プレビュー
```powershell
node scripts/serve.js
# → http://localhost:8080
```

## ディレクトリ構造
```
soyya/
├── Assets/              # Unityアセット
│   ├── Scenes/          # シーン
│   ├── Scripts/         # C#スクリプト
│   ├── Prefabs/         # プレハブ
│   ├── Materials/       # マテリアル
│   ├── Textures/        # テクスチャ
│   ├── Models/          # 3Dモデル
│   ├── Shaders/         # シェーダー
│   └── Editor/          # エディタ拡張
├── scripts/             # ビルド自動化
├── .agent/workflows/    # AIワークフロー
└── GEMINI.md            # プロジェクト憲法
```
