using UnityEngine;
using UnityEditor;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// パーティクル/VFX プリファブ自動生成
    /// </summary>
    public static class VFXFactory
    {
        private const string PREFAB_DIR = "Assets/Prefabs";

        public static void GenerateAll()
        {
            EnsureDirectory(PREFAB_DIR);
            CreateWaffleHitVFX();
            CreateGoalCelebrationVFX();
            CreateFootstepVFX();
            AssetDatabase.Refresh();
            Debug.Log("[VFX] 全エフェクト生成完了");
        }

        // ─── ワッフル命中エフェクト ───
        public static ParticleSystem CreateWaffleHitVFX()
        {
            var go = new GameObject("VFX_WaffleHit");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = 5f;
            main.startSize = 0.15f;
            main.startColor = new Color(1f, 0.85f, 0.4f);
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 15)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLife.color = gradient;

            // Renderer設定
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = new Color(1f, 0.85f, 0.4f);

            // プリファブ保存
            string path = $"{PREFAB_DIR}/VFX_WaffleHit.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VFX] WaffleHit エフェクト生成: {path}");

            return AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<ParticleSystem>();
        }

        // ─── ゴール祝福エフェクト ───
        public static ParticleSystem CreateGoalCelebrationVFX()
        {
            var go = new GameObject("VFX_GoalCelebration");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2f;
            main.loop = false;
            main.startLifetime = 1.5f;
            main.startSpeed = 8f;
            main.startSize = 0.2f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            // ランダム色
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.3f, 0.6f),
                new Color(0f, 0.9f, 1f)
            );

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50),
                new ParticleSystem.Burst(0.3f, 30),
                new ParticleSystem.Burst(0.6f, 20),
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.5f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));

            var gravity = ps.main;
            gravity.gravityModifier = 1.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = Color.white;

            string path = $"{PREFAB_DIR}/VFX_GoalCelebration.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VFX] GoalCelebration エフェクト生成: {path}");

            return AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<ParticleSystem>();
        }

        // ─── 足元ダストエフェクト ───
        public static ParticleSystem CreateFootstepVFX()
        {
            var go = new GameObject("VFX_Footstep");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = 0.3f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.3f, 0.3f, 0.35f, 0.5f);
            main.maxParticles = 8;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 5)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.2f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);

            string path = $"{PREFAB_DIR}/VFX_Footstep.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[VFX] Footstep エフェクト生成: {path}");

            return AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<ParticleSystem>();
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
