using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// マスターシーンビルダー v2.0
    /// 全アセット生成 → シーン構築 → ポストプロセスを1コマンドで完結
    /// メニュー: Soyya > シーンセットアップ
    /// </summary>
    public static class GameSceneBuilder
    {
        private static Material _baseMaterial;

        // ─── メインエントリポイント ───

        [MenuItem("Soyya/シーンセットアップ/★ 完全ビルド（全アセット + シーン）")]
        public static void FullBuild()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[Master] 完全ビルド開始...");

            // Step 1: アセット生成
            ProceduralTextureGenerator.GenerateAll();
            ProceduralAudioGenerator.GenerateAll();
            AssetDatabase.Refresh();

            // Step 2: シーンクリア & 再構築
            ClearScene();
            BuildScene();

            // Step 3: VFX & ポストプロセス
            VFXFactory.GenerateAll();
            PostProcessSetup.Setup();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // シーンダーティマーク
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[Master] 完全ビルド完了！");
            Debug.Log("[Master] ⚠ Groundを選択 → NavMeshSurface の Bake を実行してください");
            Debug.Log("═══════════════════════════════════════");
        }

        [MenuItem("Soyya/シーンセットアップ/シーンのみ再構築")]
        public static void RebuildSceneOnly()
        {
            ClearScene();
            BuildScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Soyya/シーンセットアップ/シーンをクリア")]
        public static void ClearScene()
        {
            _baseMaterial = null;
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name == "Main Camera" || obj.name == "Directional Light") continue;
                Object.DestroyImmediate(obj);
            }

            // Main Camera のコンポーネントクリーン
            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<FollowCamera>();
                if (follow != null) Object.DestroyImmediate(follow);
                cam.transform.SetParent(null);
                cam.transform.position = new Vector3(0, 5, -10);
                cam.transform.rotation = Quaternion.Euler(20, 0, 0);
            }

            Debug.Log("[Scene] シーンクリア完了");
        }

        // ─── シーン構築 ───

        private static void BuildScene()
        {
            CreateGround();
            CreateAlleyBuildings();
            CreateNeonSigns();
            CreateLighting();
            CreateSkybox();
            CreatePlayer();
            CreateMonsters();
            CreateGoal();
            CreatePickups();
            SetupCamera();
            SetupGameSystems();
            UIBuilder.BuildAllUI();
        }

        // ─── 地面（濡れたアスファルト） ───

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 8f);
            ground.isStatic = true;

            var mat = CreateURPMaterial(new Color(0.06f, 0.06f, 0.08f));
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetFloat("_Metallic", 0.3f);

            // テクスチャ適用
            var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/T_WetAsphalt_Normal.png");
            if (normalMap != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", 1.5f);
            }
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/T_Ground_Albedo.png");
            if (albedo != null)
            {
                mat.SetTexture("_BaseMap", albedo);
                mat.SetTextureScale("_BaseMap", new Vector2(3f, 4f));
            }

            ground.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_WetAsphalt");
        }

        // ─── ビル群（窓テクスチャ付き）───

        private static void CreateAlleyBuildings()
        {
            var parent = new GameObject("Buildings");

            // 窓テクスチャ
            var windowTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/T_Building_Windows.png");

            // 左壁ビル群
            CreateBuilding(parent.transform, new Vector3(-7.5f, 4f, 15f), new Vector3(5f, 8f, 6f), windowTex, 0);
            CreateBuilding(parent.transform, new Vector3(-7.5f, 5f, 6f), new Vector3(5f, 10f, 5f), windowTex, 1);
            CreateBuilding(parent.transform, new Vector3(-7.5f, 3.5f, -3f), new Vector3(5f, 7f, 4f), windowTex, 2);
            CreateBuilding(parent.transform, new Vector3(-7.5f, 4.5f, -12f), new Vector3(5f, 9f, 6f), windowTex, 3);
            CreateBuilding(parent.transform, new Vector3(-7.5f, 3f, -22f), new Vector3(5f, 6f, 5f), windowTex, 4);

            // 右壁ビル群
            CreateBuilding(parent.transform, new Vector3(7.5f, 5f, 15f), new Vector3(5f, 10f, 5f), windowTex, 5);
            CreateBuilding(parent.transform, new Vector3(7.5f, 4f, 7f), new Vector3(5f, 8f, 6f), windowTex, 6);
            CreateBuilding(parent.transform, new Vector3(7.5f, 3.5f, -2f), new Vector3(5f, 7f, 4f), windowTex, 7);
            CreateBuilding(parent.transform, new Vector3(7.5f, 4f, -10f), new Vector3(5f, 8f, 5f), windowTex, 8);
            CreateBuilding(parent.transform, new Vector3(7.5f, 5f, -20f), new Vector3(5f, 10f, 6f), windowTex, 9);

            // 通路内の障害物ビル
            CreateBuilding(parent.transform, new Vector3(-2f, 1.5f, 10f), new Vector3(2.5f, 3f, 2.5f), windowTex, 10);
            CreateBuilding(parent.transform, new Vector3(2.5f, 1.2f, 0f), new Vector3(2f, 2.4f, 2f), windowTex, 11);
            CreateBuilding(parent.transform, new Vector3(-1.5f, 1f, -8f), new Vector3(2f, 2f, 3f), windowTex, 12);
            CreateBuilding(parent.transform, new Vector3(2f, 1.5f, -16f), new Vector3(2.5f, 3f, 2f), windowTex, 13);
        }

        private static void CreateBuilding(Transform parent, Vector3 pos, Vector3 scale, Texture2D windowTex, int index)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Building_{index}";
            building.transform.SetParent(parent);
            building.transform.position = pos;
            building.transform.localScale = scale;
            building.isStatic = true;

            Color[] wallColors = {
                new Color(0.08f, 0.06f, 0.1f),
                new Color(0.1f, 0.07f, 0.08f),
                new Color(0.06f, 0.08f, 0.1f),
                new Color(0.1f, 0.06f, 0.12f),
            };

            var mat = CreateURPMaterial(wallColors[index % wallColors.Length]);
            mat.SetFloat("_Smoothness", 0.2f);

            if (windowTex != null)
            {
                mat.SetTexture("_BaseMap", windowTex);
                mat.EnableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", windowTex);
                mat.SetColor("_EmissionColor", Color.white * 0.5f);
                // タイリング: ビルの高さに応じて
                float tilingX = scale.x / 3f;
                float tilingY = scale.y / 3f;
                mat.SetTextureScale("_BaseMap", new Vector2(tilingX, tilingY));
                mat.SetTextureScale("_EmissionMap", new Vector2(tilingX, tilingY));
            }

            building.GetComponent<Renderer>().material = mat;
        }

        // ─── ネオン看板 ───

        private static void CreateNeonSigns()
        {
            var parent = new GameObject("NeonSigns");

            Vector3[] signPositions = {
                new Vector3(-4.8f, 4.5f, 12f),
                new Vector3(4.8f, 3.5f, 4f),
                new Vector3(-4.8f, 5f, -5f),
                new Vector3(4.8f, 4f, -14f),
                new Vector3(-4.8f, 3f, -20f),
            };

            Quaternion[] signRotations = {
                Quaternion.Euler(0, 90, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
            };

            Color[] neonColors = {
                new Color(1f, 0.2f, 0.6f),
                new Color(0f, 0.9f, 1f),
                new Color(1f, 0.8f, 0f),
                new Color(0.6f, 0.2f, 1f),
                new Color(1f, 0.4f, 0.1f),
            };

            for (int i = 0; i < signPositions.Length; i++)
            {
                var sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
                sign.name = $"NeonSign_{i}";
                sign.transform.SetParent(parent.transform);
                sign.transform.position = signPositions[i];
                sign.transform.rotation = signRotations[i];
                sign.transform.localScale = new Vector3(2.5f, 1.2f, 1f);
                sign.isStatic = true;
                Object.DestroyImmediate(sign.GetComponent<MeshCollider>());

                var signTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Textures/T_NeonSign_{i}.png");
                var mat = CreateURPMaterial(neonColors[i]);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", neonColors[i] * 3f);
                if (signTex != null)
                {
                    mat.SetTexture("_BaseMap", signTex);
                    mat.SetTexture("_EmissionMap", signTex);
                }
                sign.GetComponent<Renderer>().material = mat;

                // 看板用ポイントライト
                var lightObj = new GameObject($"NeonSignLight_{i}");
                lightObj.transform.SetParent(sign.transform);
                lightObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = neonColors[i];
                light.intensity = 3f;
                light.range = 8f;
                light.shadows = LightShadows.None;
            }
        }

        // ─── ライティング ───

        private static void CreateLighting()
        {
            var parent = new GameObject("Lighting");

            // 環境設定
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.015f, 0.015f, 0.04f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.04f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.025f;

            // Directional Light（月明かり）
            var dirLight = GameObject.Find("Directional Light");
            if (dirLight != null)
            {
                var light = dirLight.GetComponent<Light>();
                light.color = new Color(0.08f, 0.08f, 0.2f);
                light.intensity = 0.2f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                light.shadows = LightShadows.Soft;
            }

            // ランタンライト（暖かいオレンジ）
            Vector3[] lanternPositions = {
                new Vector3(-2f, 3f, 14f),
                new Vector3(1.5f, 3.5f, 8f),
                new Vector3(-1f, 2.8f, 2f),
                new Vector3(2f, 3f, -4f),
                new Vector3(-1.5f, 3.2f, -10f),
                new Vector3(1f, 2.8f, -16f),
                new Vector3(0f, 3.5f, -22f),
            };

            for (int i = 0; i < lanternPositions.Length; i++)
            {
                var lanternObj = new GameObject($"Lantern_{i}");
                lanternObj.transform.SetParent(parent.transform);
                lanternObj.transform.position = lanternPositions[i];

                // ランタン本体（光る球）
                var lanternVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lanternVisual.name = "Visual";
                lanternVisual.transform.SetParent(lanternObj.transform);
                lanternVisual.transform.localPosition = Vector3.zero;
                lanternVisual.transform.localScale = Vector3.one * 0.3f;
                Object.DestroyImmediate(lanternVisual.GetComponent<SphereCollider>());

                float hue = (i * 0.1f) % 1f;
                Color lanternColor = Color.HSVToRGB(0.07f + hue * 0.05f, 0.7f, 1f);

                var lanternMat = CreateURPMaterial(lanternColor);
                lanternMat.EnableKeyword("_EMISSION");
                lanternMat.SetColor("_EmissionColor", lanternColor * 4f);
                lanternVisual.GetComponent<Renderer>().material = lanternMat;

                // ライト
                var light = lanternObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.7f, 0.3f);
                light.intensity = 2f;
                light.range = 6f;
            }
        }

        // ─── スカイボックス ───

        private static void CreateSkybox()
        {
            var skyShader = Shader.Find("Soyya/CyberpunkSkybox");
            if (skyShader != null)
            {
                var skyMat = new Material(skyShader);
                skyMat.SetColor("_TopColor", new Color(0.005f, 0.005f, 0.03f));
                skyMat.SetColor("_HorizonColor", new Color(0.03f, 0.01f, 0.08f));
                skyMat.SetColor("_NeonColor1", new Color(1f, 0.2f, 0.6f));
                skyMat.SetColor("_NeonColor2", new Color(0f, 0.7f, 1f));
                skyMat.SetFloat("_StarDensity", 0.25f);
                skyMat.SetFloat("_NeonIntensity", 0.4f);

                RenderSettings.skybox = skyMat;
                SaveMaterial(skyMat, "M_CyberpunkSkybox");
                Debug.Log("[Scene] サイバーパンクスカイボックス設定完了");
            }
            else
            {
                Debug.LogWarning("[Scene] CyberpunkSkybox シェーダーが見つかりません");
            }
        }

        // ─── プレイヤー ───

        private static void CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1f, 22f);
            player.tag = "Player";

            // CharacterController
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0, 0);

            // PlayerController
            player.AddComponent<PlayerController>();

            // ランチャー視覚（右手に見える武器）
            var launcher = new GameObject("WaffleLauncher");
            launcher.transform.SetParent(player.transform);
            launcher.transform.localPosition = new Vector3(0.35f, 0.3f, 0.5f);
            launcher.transform.localRotation = Quaternion.Euler(0, 0, -5f);

            // ランチャーボディ
            var launcherBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            launcherBody.name = "LauncherBody";
            launcherBody.transform.SetParent(launcher.transform);
            launcherBody.transform.localPosition = Vector3.zero;
            launcherBody.transform.localScale = new Vector3(0.12f, 0.1f, 0.35f);
            Object.DestroyImmediate(launcherBody.GetComponent<BoxCollider>());
            var launcherMat = CreateURPMaterial(new Color(0.3f, 0.3f, 0.35f));
            launcherMat.SetFloat("_Metallic", 0.8f);
            launcherMat.SetFloat("_Smoothness", 0.7f);
            launcherBody.GetComponent<Renderer>().material = launcherMat;

            // ランチャー撃ち出し口
            var muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muzzle.name = "Muzzle";
            muzzle.transform.SetParent(launcher.transform);
            muzzle.transform.localPosition = new Vector3(0, 0, 0.22f);
            muzzle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            muzzle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Object.DestroyImmediate(muzzle.GetComponent<CapsuleCollider>());
            var muzzleMat = CreateURPMaterial(new Color(0.15f, 0.15f, 0.2f));
            muzzleMat.SetFloat("_Metallic", 0.9f);
            muzzle.GetComponent<Renderer>().material = muzzleMat;

            // ランチャー内のワッフル弾（見た目だけ）
            var ammoVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ammoVisual.name = "AmmoVisual";
            ammoVisual.transform.SetParent(launcher.transform);
            ammoVisual.transform.localPosition = new Vector3(0, 0.08f, 0.05f);
            ammoVisual.transform.localScale = Vector3.one * 0.08f;
            Object.DestroyImmediate(ammoVisual.GetComponent<SphereCollider>());
            var ammoMat = CreateURPMaterial(new Color(0.85f, 0.65f, 0.35f));
            ammoMat.EnableKeyword("_EMISSION");
            ammoMat.SetColor("_EmissionColor", new Color(0.85f, 0.65f, 0.35f) * 0.5f);
            ammoVisual.GetComponent<Renderer>().material = ammoMat;

            // ThrowPoint
            var throwPoint = new GameObject("ThrowPoint");
            throwPoint.transform.SetParent(launcher.transform);
            throwPoint.transform.localPosition = new Vector3(0, 0, 0.3f);
        }

        // ─── モンスター（複合ボディ） ───

        private static void CreateMonsters()
        {
            var parent = new GameObject("Monsters");

            // Whi-chan（ゴースト）
            CreateMonsterWithBody("Whi-chan", new Vector3(1f, 1.2f, 8f),
                parent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, 12f), new Vector3(3f, 0.5f, 6f),
                    new Vector3(-1f, 0.5f, 10f), new Vector3(2f, 0.5f, 4f)
                }, "ghost");

            // Mecha-paku（メカ）
            CreateMonsterWithBody("Mecha-paku", new Vector3(-2f, 1f, -2f),
                parent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, -4f), new Vector3(3f, 0.5f, 0f),
                    new Vector3(0f, 0.5f, -6f), new Vector3(-2f, 0.5f, 2f)
                }, "mecha");

            // Fuzz-nom（毛玉）
            CreateMonsterWithBody("Fuzz-nom", new Vector3(2f, 1f, -14f),
                parent.transform,
                new Vector3[] {
                    new Vector3(3f, 0.5f, -16f), new Vector3(-3f, 0.5f, -10f),
                    new Vector3(0f, 0.5f, -18f), new Vector3(-2f, 0.5f, -12f)
                }, "fuzzy");
        }

        private static void CreateMonsterWithBody(string name, Vector3 pos,
            Transform parent, Vector3[] patrolPoints, string type)
        {
            var monster = new GameObject(name);
            monster.transform.SetParent(parent);
            monster.transform.position = pos;

            // NavMeshAgent
            var agent = monster.AddComponent<NavMeshAgent>();
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;

            // タイプ別設定
            Material baseMat = GetBaseMaterial();

            switch (type)
            {
                case "ghost":
                    agent.speed = 1.8f;
                    agent.radius = 0.7f;
                    agent.height = 2.2f;
                    monster.AddComponent<GhostMonster>();
                    MonsterBodyBuilder.BuildGhostBody(monster, baseMat);
                    break;
                case "mecha":
                    agent.speed = 3.5f;
                    agent.radius = 0.6f;
                    agent.height = 1.8f;
                    monster.AddComponent<MechaMonster>();
                    MonsterBodyBuilder.BuildMechaBody(monster, baseMat);
                    // サーチライト
                    var searchLight = new GameObject("SearchLight");
                    searchLight.transform.SetParent(monster.transform);
                    searchLight.transform.localPosition = new Vector3(0, 0.5f, 0.8f);
                    var sl = searchLight.AddComponent<Light>();
                    sl.type = LightType.Spot;
                    sl.color = new Color(0, 1f, 0.3f);
                    sl.intensity = 3f;
                    sl.range = 12f;
                    sl.spotAngle = 35f;
                    sl.shadows = LightShadows.Soft;
                    break;
                case "fuzzy":
                    agent.speed = 2.5f;
                    agent.radius = 0.8f;
                    agent.height = 2f;
                    monster.AddComponent<FuzzyMonster>();
                    MonsterBodyBuilder.BuildFuzzyBody(monster, baseMat);
                    break;
            }

            // 巡回ポイント
            var patrolParent = new GameObject($"{name}_PatrolPoints");
            patrolParent.transform.SetParent(parent);
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var pp = new GameObject($"PP_{i}");
                pp.transform.SetParent(patrolParent.transform);
                pp.transform.position = patrolPoints[i];
            }
        }

        // ─── ゴール ───

        private static void CreateGoal()
        {
            var goal = new GameObject("Goal");
            goal.transform.position = new Vector3(0f, 0.5f, -28f);

            var col = goal.AddComponent<BoxCollider>();
            col.size = new Vector3(4f, 3f, 2f);
            col.isTrigger = true;
            goal.AddComponent<GoalZone>();

            // ゴール台座
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "GoalPlatform";
            platform.transform.SetParent(goal.transform);
            platform.transform.localPosition = new Vector3(0, -0.4f, 0);
            platform.transform.localScale = new Vector3(3f, 0.1f, 3f);
            Object.DestroyImmediate(platform.GetComponent<CapsuleCollider>());
            var platMat = CreateURPMaterial(new Color(0f, 0.8f, 0.5f));
            platMat.EnableKeyword("_EMISSION");
            platMat.SetColor("_EmissionColor", new Color(0f, 1f, 0.5f) * 2f);
            platMat.SetFloat("_Smoothness", 0.9f);
            platform.GetComponent<Renderer>().material = platMat;

            // ゴール柱（4本）
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"GoalPillar_{i}";
                pillar.transform.SetParent(goal.transform);
                pillar.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 1.3f, 1f, Mathf.Sin(angle) * 1.3f);
                pillar.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
                Object.DestroyImmediate(pillar.GetComponent<CapsuleCollider>());

                var pillarMat = CreateURPMaterial(new Color(0f, 1f, 0.5f));
                pillarMat.EnableKeyword("_EMISSION");
                pillarMat.SetColor("_EmissionColor", new Color(0f, 1f, 0.6f) * 3f);
                pillar.GetComponent<Renderer>().material = pillarMat;
            }

            // ゴールライト
            var lightObj = new GameObject("GoalLight");
            lightObj.transform.SetParent(goal.transform);
            lightObj.transform.localPosition = Vector3.up * 3f;
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0f, 1f, 0.5f);
            light.intensity = 5f;
            light.range = 12f;
        }

        // ─── ワッフルピックアップ ───

        private static void CreatePickups()
        {
            var parent = new GameObject("Pickups");

            Vector3[] positions = {
                new Vector3(4f, 0.5f, 16f),
                new Vector3(-3f, 0.5f, 10f),
                new Vector3(3f, 0.5f, 3f),
                new Vector3(-2f, 0.5f, -5f),
                new Vector3(4f, 0.5f, -12f),
                new Vector3(-3f, 0.5f, -18f),
                new Vector3(0f, 0.5f, -23f),
            };

            var waffleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/T_Waffle_Albedo.png");

            for (int i = 0; i < positions.Length; i++)
            {
                var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = $"WafflePickup_{i}";
                pickup.transform.SetParent(parent.transform);
                pickup.transform.position = positions[i];
                pickup.transform.localScale = Vector3.one * 0.35f;

                Object.DestroyImmediate(pickup.GetComponent<SphereCollider>());
                var sc = pickup.AddComponent<SphereCollider>();
                sc.radius = 2f;
                sc.isTrigger = true;
                pickup.AddComponent<WafflePickup>();

                var mat = CreateURPMaterial(new Color(0.85f, 0.65f, 0.35f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.5f) * 1.5f);
                if (waffleTex != null) mat.SetTexture("_BaseMap", waffleTex);
                pickup.GetComponent<Renderer>().material = mat;

                // 浮遊光
                var glow = new GameObject($"PickupGlow_{i}");
                glow.transform.SetParent(pickup.transform);
                glow.transform.localPosition = Vector3.zero;
                var glowLight = glow.AddComponent<Light>();
                glowLight.type = LightType.Point;
                glowLight.color = new Color(1f, 0.8f, 0.3f);
                glowLight.intensity = 1.5f;
                glowLight.range = 3f;
            }
        }

        // ─── カメラ ───

        private static void SetupCamera()
        {
            var mainCam = Camera.main;
            var player = GameObject.Find("Player");

            if (mainCam != null && player != null)
            {
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = new Vector3(0f, 0.7f, 0.15f);
                mainCam.transform.localRotation = Quaternion.identity;

                mainCam.gameObject.AddComponent<FollowCamera>();
                mainCam.backgroundColor = new Color(0.005f, 0.005f, 0.02f);
                mainCam.fieldOfView = 75f;
                mainCam.nearClipPlane = 0.05f;
                mainCam.farClipPlane = 200f;

                var urpData = mainCam.GetComponent<UniversalAdditionalCameraData>();
                if (urpData == null)
                    urpData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                urpData.renderPostProcessing = true;
            }
        }

        // ─── ゲームシステム ───

        private static void SetupGameSystems()
        {
            // GameManager
            var gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            // DifficultyManager
            gmObj.AddComponent<DifficultyManager>();

            // AudioManager（AudioSource付き）
            var audioObj = new GameObject("AudioManager");
            var bgmSource = audioObj.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;

            var seSource = audioObj.AddComponent<AudioSource>();
            seSource.playOnAwake = false;

            var audioMgr = audioObj.AddComponent<AudioManager>();

            // SerializedFieldにAudioSourceを割り当て
            var so = new SerializedObject(audioMgr);
            var bgmProp = so.FindProperty("_bgmSource");
            if (bgmProp != null) bgmProp.objectReferenceValue = bgmSource;
            var seProp = so.FindProperty("_seSource");
            if (seProp != null) seProp.objectReferenceValue = seSource;
            so.ApplyModifiedPropertiesWithoutUndo();

            // AudioClip割り当て
            AssignAudioClips(audioMgr);

            // ObjectPool（ワッフル弾用）
            var poolObj = new GameObject("WafflePool");
            poolObj.AddComponent<ObjectPool>();

            // ワッフル弾テンプレート
            CreateWaffleBallPrefab();

            Debug.Log("[Scene] ゲームシステム構築完了");
        }

        private static void AssignAudioClips(AudioManager mgr)
        {
            // 全SEフィールドを直接割り当て
            var type = typeof(AudioManager);
            string[] fieldNames = {
                "SeThrow", "SeHit", "SePickup", "SeCaught",
                "SeGoal", "SeMonsterEat", "SeMonsterDetect", "SeCountdown"
            };
            string[] clipNames = {
                "SE_Throw", "SE_Hit", "SE_Pickup", "SE_Caught",
                "SE_Goal", "SE_Paku", "SE_Pickup", "SE_Countdown"
            };

            for (int i = 0; i < fieldNames.Length; i++)
            {
                var field = type.GetField(fieldNames[i]);
                if (field == null) continue;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{clipNames[i]}.wav");
                if (clip != null)
                    field.SetValue(mgr, clip);
            }
        }

        private static void CreateWaffleBallPrefab()
        {
            string prefabPath = "Assets/Prefabs/WaffleBall.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            EnsureDirectory("Assets/Prefabs");

            var waffle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waffle.name = "WaffleBall";
            waffle.transform.localScale = Vector3.one * 0.25f;

            var rb = waffle.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.useGravity = true;

            waffle.AddComponent<WaffleBall>();

            // トレイルレンダラー
            var trail = waffle.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trail.material.color = new Color(1f, 0.8f, 0.3f);
            trail.startColor = new Color(1f, 0.8f, 0.3f, 1f);
            trail.endColor = new Color(1f, 0.6f, 0.2f, 0f);

            // マテリアル
            var mat = CreateURPMaterial(new Color(0.85f, 0.65f, 0.35f));
            var waffleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/T_Waffle_Albedo.png");
            if (waffleTex != null) mat.SetTexture("_BaseMap", waffleTex);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.85f, 0.65f, 0.35f) * 0.3f);
            waffle.GetComponent<Renderer>().material = mat;

            PrefabUtility.SaveAsPrefabAsset(waffle, prefabPath);
            Object.DestroyImmediate(waffle);
            Debug.Log("[Scene] ワッフル弾Prefab生成完了");
        }


        // ─── ユーティリティ ───

        private static Material GetBaseMaterial()
        {
            if (_baseMaterial == null)
            {
                var rpAsset = GraphicsSettings.currentRenderPipeline;
                if (rpAsset != null)
                    _baseMaterial = rpAsset.defaultMaterial;

                if (_baseMaterial == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:Material");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (m != null && m.shader != null && m.shader.name.Contains("Lit"))
                        {
                            _baseMaterial = m;
                            break;
                        }
                    }
                }

                if (_baseMaterial == null)
                    _baseMaterial = new Material(Shader.Find("Standard"));
            }
            return _baseMaterial;
        }

        private static Material CreateURPMaterial(Color color)
        {
            var mat = new Material(GetBaseMaterial());
            mat.color = color;
            return mat;
        }

        private static void SaveMaterial(Material mat, string name)
        {
            EnsureDirectory("Assets/Materials");
            string path = $"Assets/Materials/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
                AssetDatabase.CreateAsset(mat, path);
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
