---
description: WebGLビルド → Git自動コミット → デプロイ
---

# /deploy — ビルド＆デプロイ

## ステップ

// turbo-all

1. 本番WebGLビルドを実行する
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build.ps1 -Commit
```

2. ビルドが成功したら自動的にGit commit & pushが行われる

3. GitHub Pages / Vercelの場合はデプロイステータスを確認する

## 手動デプロイ（GitHub Pages）

WebGLビルド出力を `gh-pages` ブランチにプッシュ:
```powershell
git subtree push --prefix WebGL-Build origin gh-pages
```
