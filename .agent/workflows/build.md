---
description: WebGLビルドを実行してブラウザプレビューを起動する
---

# /build — WebGLビルド＆プレビュー

## ステップ

// turbo-all

1. Unity EditorのパスをスキャンしてWebGLビルドを実行する
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build.ps1 -Serve
```

2. ビルド完了後、プレビューサーバーが起動していることを確認する（http://localhost:8080）

3. ブラウザでプレビューを開いて動作確認する

4. コンソールエラーがあれば修正 → 再ビルド
