using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// WebGLビルド & デプロイ準備
    /// テクスチャ圧縮、ビルドサイズ最適化、index.html OGPカスタマイズ
    /// </summary>
    public static class WebGLBuildTool
    {
        private const string BUILD_PATH = "Build/WebGL";

        [MenuItem("Soyya/WebGL/★ ビルド（最適化済み）")]
        public static void BuildWebGL()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[WebGL] ビルド開始...");

            // ビルド前の最適化設定
            OptimizeProjectSettings();

            // シーンリスト
            string[] scenes = GetBuildScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[WebGL] ビルドするシーンがありません。シーンを保存してください。");
                return;
            }

            // ビルドオプション
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BUILD_PATH,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            // ビルド実行
            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGL] ビルド成功！サイズ: {report.summary.totalSize / (1024 * 1024f):F1} MB");
                Debug.Log($"[WebGL] 出力先: {Path.GetFullPath(BUILD_PATH)}");

                // OGP付きindex.html生成
                GenerateCustomHTML();

                Debug.Log("[WebGL] 完了！Build/WebGL をGitHub Pagesにデプロイしてください");
            }
            else
            {
                Debug.LogError($"[WebGL] ビルド失敗: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Warning)
                            Debug.LogError($"  {msg.content}");
                    }
                }
            }

            Debug.Log("═══════════════════════════════════════");
        }

        [MenuItem("Soyya/WebGL/プロジェクト設定を最適化")]
        public static void OptimizeProjectSettings()
        {
            // WebGL Player Settings
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;

            // テクスチャ品質
            QualitySettings.globalTextureMipmapLimit = 0;

            // ストリッピング
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL,
                ManagedStrippingLevel.Medium);

            // 製品名・会社名
            PlayerSettings.productName = "WaffleMonster";
            PlayerSettings.companyName = "Soyya";

            // 解像度
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 600;

            // カーソル
            PlayerSettings.defaultCursor = null;
            PlayerSettings.runInBackground = true;

            Debug.Log("[WebGL] プロジェクト設定最適化完了");
        }

        [MenuItem("Soyya/WebGL/テクスチャ圧縮最適化")]
        public static void OptimizeTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Textures" });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;

                // WebGL用設定
                var webglSettings = importer.GetPlatformTextureSettings("WebGL");
                if (!webglSettings.overridden)
                {
                    webglSettings.overridden = true;
                    webglSettings.maxTextureSize = 512;
                    webglSettings.format = TextureImporterFormat.DXT5;
                    webglSettings.compressionQuality = 50;
                    importer.SetPlatformTextureSettings(webglSettings);
                    changed = true;
                }

                // ミップマップ無効化（UI系テクスチャ）
                if (path.Contains("NeonSign") || path.Contains("Window"))
                {
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }

            Debug.Log($"[WebGL] テクスチャ {count} 個を最適化しました");
        }

        private static string[] GetBuildScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            // シーンが設定されていない場合、現在のシーンを使用
            if (scenes.Count == 0)
            {
                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(currentScene))
                {
                    scenes.Add(currentScene);
                }
            }

            return scenes.ToArray();
        }

        private static void GenerateCustomHTML()
        {
            string htmlPath = Path.Combine(BUILD_PATH, "index.html");
            if (!File.Exists(htmlPath)) return;

            string html = File.ReadAllText(htmlPath);

            // OGPメタタグ挿入
            string ogpTags = @"
    <!-- OGP -->
    <meta property=""og:title"" content=""WaffleMonster 〜ネオン裏路地の脱出劇〜"">
    <meta property=""og:description"" content=""ワッフルでモンスターを撃退しながらゴールを目指せ！サイバーパンクな裏路地を駆け抜けるFPSアクション。"">
    <meta property=""og:type"" content=""website"">
    <meta property=""og:image"" content=""ogp.png"">
    <meta name=""twitter:card"" content=""summary_large_image"">
    <meta name=""twitter:title"" content=""WaffleMonster"">
    <meta name=""twitter:description"" content=""ワッフル × モンスター × ネオン裏路地 FPSアクション"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">";

            html = html.Replace("<head>", "<head>" + ogpTags);

            // タイトル変更
            html = html.Replace("<title>Unity WebGL Player", "<title>WaffleMonster");

            // モバイルフルスクリーン対応CSS
            string mobileCSS = @"
    <style>
      body { margin: 0; padding: 0; background: #050510; overflow: hidden; }
      #unity-container { width: 100vw; height: 100vh; }
      #unity-canvas { width: 100% !important; height: 100% !important; }
      #unity-loading-bar { position: absolute; left: 50%; top: 50%; transform: translate(-50%, -50%); }
      #unity-progress-bar-full { background: #ff3399; }
      @media (max-width: 768px) {
        #unity-canvas { touch-action: none; }
      }
    </style>";

            html = html.Replace("</head>", mobileCSS + "\n  </head>");

            File.WriteAllText(htmlPath, html);
            Debug.Log("[WebGL] カスタムindex.html生成完了（OGP + モバイル対応）");
        }
    }
}
