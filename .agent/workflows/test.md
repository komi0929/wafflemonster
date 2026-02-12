---
description: WebGLビルド検証とブラウザテスト
---

# /test — ビルド検証＆ブラウザテスト

## ステップ

// turbo-all

1. 開発ビルドを実行する（高速、デバッグシンボル付き）
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build.ps1 -Dev -Serve
```

2. ブラウザでプレビューを開く（http://localhost:8080）

3. 以下の項目を確認する:
   - WebGLコンテンツが正常にロードされること
   - コンソールにエラーがないこと
   - FPS（フレームレート）が安定していること
   - メモリ使用量が適正範囲内であること

4. テスト結果をターミナルに出力する

5. 問題があれば修正して再テスト
