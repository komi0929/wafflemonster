using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// URP ポストプロセスVolume自動セットアップ
    /// Bloom, Vignette, Color Grading, Film Grain
    /// </summary>
    public static class PostProcessSetup
    {
        public static void Setup()
        {
            var existing = GameObject.Find("PostProcess_Volume");
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject("PostProcess_Volume");
            go.layer = 0;

            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // ── Bloom（ネオンの輝き）──
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.8f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 1.5f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.7f;

            // ── Vignette（周辺暗化）──
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.4f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.5f;

            // ── Color Adjustments（サイバーパンク色調）──
            var colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.overrideState = true;
            colorAdj.postExposure.value = 0.3f;
            colorAdj.contrast.overrideState = true;
            colorAdj.contrast.value = 15f;
            colorAdj.saturation.overrideState = true;
            colorAdj.saturation.value = 20f;

            // ── Lift Gamma Gain（ティール＆オレンジ調）──
            var lgg = profile.Add<LiftGammaGain>(true);
            lgg.lift.overrideState = true;
            lgg.lift.value = new Vector4(0.95f, 0.98f, 1.05f, 0f); // シャドウに青み
            lgg.gain.overrideState = true;
            lgg.gain.value = new Vector4(1.05f, 1.0f, 0.95f, 0f); // ハイライトに暖色

            // ── Film Grain（粒子ノイズ — 少量）──
            var grain = profile.Add<FilmGrain>(true);
            grain.intensity.overrideState = true;
            grain.intensity.value = 0.15f;

            volume.profile = profile;

            // プロファイルをアセットとして保存
            string profilePath = "Assets/Settings/PostProcess_CyberpunkProfile.asset";
            EnsureDirectory("Assets/Settings");
            AssetDatabase.CreateAsset(profile, profilePath);

            Debug.Log("[PostProcess] サイバーパンクポストプロセス設定完了");
        }

        private static void EnsureDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
