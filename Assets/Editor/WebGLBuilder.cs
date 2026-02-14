using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Soyya.Editor
{
    /// <summary>
    /// WebGLビルドをコマンドラインから実行するためのエディタ拡張
    /// Usage: Unity.exe -batchmode -nographics -projectPath . -executeMethod Soyya.Editor.WebGLBuilder.Build -quit
    /// </summary>
    public static class WebGLBuilder
    {
        private const string BUILD_OUTPUT = "WebGL-Build";

        [MenuItem("Soyya/Build WebGL")]
        public static void Build()
        {
            Debug.Log("[WebGLBuilder] ビルド開始...");

            // すべてのシーンを取得
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            // Build Settingsが空の場合、Assets/Scenesフォルダから自動検出
            if (scenes.Length == 0)
            {
                Debug.Log("[WebGLBuilder] Build Settingsにシーンが未登録。Assets/Scenesフォルダから自動検出します...");
                scenes = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
                if (scenes.Length == 0)
                {
                    Debug.LogError("[WebGLBuilder] 有効なシーンが見つかりません。Assets/Scenesフォルダにシーンを配置してください。");
                    EditorApplication.Exit(1);
                    return;
                }
            }

            Debug.Log($"[WebGLBuilder] ビルド対象シーン: {string.Join(", ", scenes)}");

            // WebGL Player Settings（GitHub Pages対応: 圧縮無効 + Decompression Fallback）
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true; // GitHub Pages対応
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            // メモリ最適化
            PlayerSettings.WebGL.initialMemorySize = 32; // 32MB初期メモリ
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BUILD_OUTPUT,
                target = BuildTarget.WebGL,
                options = BuildOptions.CleanBuildCache
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] ✅ ビルド成功! サイズ: {report.summary.totalSize / (1024 * 1024)}MB");
                Debug.Log($"[WebGLBuilder] 出力先: {BUILD_OUTPUT}/");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] ❌ ビルド失敗: {report.summary.totalErrors} エラー");
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Soyya/Build WebGL (Development)")]
        public static void BuildDev()
        {
            Debug.Log("[WebGLBuilder] 開発ビルド開始...");

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            // Build Settingsが空の場合、Assets/Scenesフォルダから自動検出
            if (scenes.Length == 0)
            {
                Debug.Log("[WebGLBuilder] Build Settingsにシーンが未登録。Assets/Scenesフォルダから自動検出します...");
                scenes = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
                if (scenes.Length == 0)
                {
                    Debug.LogError("[WebGLBuilder] 有効なシーンが見つかりません。");
                    EditorApplication.Exit(1);
                    return;
                }
            }

            // dev buildでは圧縮なし（高速）
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "WebGL-Build-Dev",
                target = BuildTarget.WebGL,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] ✅ 開発ビルド成功!");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] ❌ 開発ビルド失敗");
                EditorApplication.Exit(1);
            }
        }
    }
}
