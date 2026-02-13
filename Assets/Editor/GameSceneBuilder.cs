using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ゲームシーンをプログラムで構築するエディタスクリプト
    /// メニュー: Soyya > シーンセットアップ > ネオン裏路地を生成
    /// </summary>
    public static class GameSceneBuilder
    {
        [MenuItem("Soyya/シーンセットアップ/ネオン裏路地を生成")]
        public static void BuildNeonAlleyScene()
        {
            Debug.Log("[GameSceneBuilder] ネオン裏路地シーン生成開始...");

            CreateGround();
            CreateBuildings();
            CreateNeonLighting();
            CreatePlayer();
            CreateMonsters();
            CreateGoal();
            CreatePickups();
            SetupCamera();

            // シーンを保存するようマーク
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[GameSceneBuilder] シーン生成完了！");
            Debug.Log("[GameSceneBuilder] ⚠ Window > AI > Navigation からNavMeshをベイクしてください");
        }

        [MenuItem("Soyya/シーンセットアップ/シーンをクリア＆再生成")]
        public static void ClearAndRebuild()
        {
            // 既存オブジェクトを削除（Main CameraとDirectional Light以外）
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name == "Main Camera" || obj.name == "Directional Light") continue;
                Object.DestroyImmediate(obj);
            }

            // マテリアルフォルダを削除して再作成
            if (AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.DeleteAsset("Assets/Materials");
            }

            AssetDatabase.Refresh();
            BuildNeonAlleyScene();
        }

        [MenuItem("Soyya/シーンセットアップ/シーンをクリア")]
        public static void ClearScene()
        {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name == "Main Camera" || obj.name == "Directional Light") continue;
                Object.DestroyImmediate(obj);
            }
            Debug.Log("[GameSceneBuilder] シーンをクリアしました");
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 8f);
            ground.isStatic = true;

            var mat = CreateURPMaterial(new Color(0.08f, 0.08f, 0.1f));
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.3f);
            ground.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_WetAsphalt");
        }

        private static void CreateBuildings()
        {
            var buildingParent = new GameObject("Buildings");

            Vector3[][] buildingLayout = new Vector3[][]
            {
                new Vector3[] { new Vector3(-8f, 3f, 15f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(-8f, 4f, 5f), new Vector3(6f, 8f, 4f) },
                new Vector3[] { new Vector3(-8f, 3.5f, -5f), new Vector3(6f, 7f, 5f) },
                new Vector3[] { new Vector3(-8f, 3f, -18f), new Vector3(6f, 6f, 6f) },
                new Vector3[] { new Vector3(8f, 3f, 15f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(8f, 4.5f, 5f), new Vector3(6f, 9f, 4f) },
                new Vector3[] { new Vector3(8f, 3f, -5f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(8f, 3.5f, -18f), new Vector3(6f, 7f, 6f) },
                new Vector3[] { new Vector3(0f, 1f, 10f), new Vector3(2f, 2f, 3f) },
                new Vector3[] { new Vector3(-2f, 1.5f, 0f), new Vector3(3f, 3f, 2f) },
                new Vector3[] { new Vector3(2f, 1f, -10f), new Vector3(2.5f, 2f, 2.5f) },
            };

            var wallMat = CreateURPMaterial(new Color(0.12f, 0.1f, 0.15f));
            SaveMaterial(wallMat, "M_DarkWall");

            for (int i = 0; i < buildingLayout.Length; i++)
            {
                var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{i}";
                building.transform.SetParent(buildingParent.transform);
                building.transform.position = buildingLayout[i][0];
                building.transform.localScale = buildingLayout[i][1];
                building.isStatic = true;
                building.GetComponent<Renderer>().material = wallMat;
            }
        }

        private static void CreateNeonLighting()
        {
            var lightParent = new GameObject("Lighting");

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.05f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.03f;

            var dirLight = GameObject.Find("Directional Light");
            if (dirLight != null)
            {
                var light = dirLight.GetComponent<Light>();
                light.color = new Color(0.1f, 0.1f, 0.3f);
                light.intensity = 0.3f;
                light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            }

            Color[] neonColors = {
                new Color(1f, 0.2f, 0.8f),
                new Color(0f, 0.8f, 1f),
                new Color(1f, 0.8f, 0f),
                new Color(0.6f, 0.2f, 1f),
                new Color(1f, 0.3f, 0.1f),
                new Color(0f, 1f, 0.5f),
            };

            Vector3[] lightPositions = {
                new Vector3(-4.5f, 4f, 14f),
                new Vector3(4.5f, 3.5f, 8f),
                new Vector3(-4.5f, 3f, 2f),
                new Vector3(4.5f, 4f, -4f),
                new Vector3(-4.5f, 3.5f, -12f),
                new Vector3(4.5f, 3f, -16f),
            };

            for (int i = 0; i < lightPositions.Length; i++)
            {
                var lightObj = new GameObject($"NeonLight_{i}");
                lightObj.transform.SetParent(lightParent.transform);
                lightObj.transform.position = lightPositions[i];

                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = neonColors[i % neonColors.Length];
                light.intensity = 2.5f;
                light.range = 10f;
                light.shadows = LightShadows.Soft;
            }

            Vector3[] lanternPositions = {
                new Vector3(-2f, 3.5f, 12f),
                new Vector3(2f, 3.5f, 6f),
                new Vector3(0f, 3f, -2f),
                new Vector3(-1f, 3.5f, -8f),
                new Vector3(1f, 3f, -14f),
            };

            for (int i = 0; i < lanternPositions.Length; i++)
            {
                var lanternObj = new GameObject($"Lantern_{i}");
                lanternObj.transform.SetParent(lightParent.transform);
                lanternObj.transform.position = lanternPositions[i];

                var light = lanternObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.7f, 0.3f);
                light.intensity = 1.5f;
                light.range = 6f;
            }
        }

        private static void CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 20f);
            player.tag = "Player";

            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            player.AddComponent<PlayerController>();

            var mat = CreateURPMaterial(new Color(0.9f, 0.8f, 0.5f));
            mat.SetFloat("_Smoothness", 0.3f);
            player.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_Player");

            var throwPoint = new GameObject("ThrowPoint");
            throwPoint.transform.SetParent(player.transform);
            throwPoint.transform.localPosition = new Vector3(0f, 0.8f, 1f);
        }

        private static void CreateMonsters()
        {
            var monsterParent = new GameObject("Monsters");

            CreateMonsterObj("Whi-chan", new Vector3(0f, 1f, 8f),
                new Color(0.9f, 0.95f, 1f), monsterParent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, 10f),
                    new Vector3(3f, 0.5f, 6f),
                    new Vector3(0f, 0.5f, 8f)
                }, "ghost");

            CreateMonsterObj("Mecha-paku", new Vector3(-2f, 1f, -2f),
                new Color(0.5f, 0.55f, 0.6f), monsterParent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, -4f),
                    new Vector3(3f, 0.5f, 0f),
                    new Vector3(0f, 0.5f, -2f)
                }, "mecha");

            CreateMonsterObj("Fuzz-nom", new Vector3(2f, 1f, -12f),
                new Color(0.6f, 0.3f, 0.7f), monsterParent.transform,
                new Vector3[] {
                    new Vector3(3f, 0.5f, -14f),
                    new Vector3(-3f, 0.5f, -10f),
                    new Vector3(0f, 0.5f, -12f)
                }, "fuzzy");
        }

        private static void CreateMonsterObj(string name, Vector3 pos, Color color,
            Transform parent, Vector3[] patrolPoints, string type)
        {
            var monster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            monster.name = name;
            monster.transform.SetParent(parent);
            monster.transform.position = pos;
            monster.transform.localScale = Vector3.one * 1.5f;

            var agent = monster.AddComponent<NavMeshAgent>();
            agent.speed = 2f;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.6f;
            agent.height = 1.5f;

            switch (type)
            {
                case "ghost": monster.AddComponent<GhostMonster>(); break;
                case "mecha": monster.AddComponent<MechaMonster>(); break;
                case "fuzzy": monster.AddComponent<FuzzyMonster>(); break;
            }

            var patrolParent = new GameObject($"{name}_PatrolPoints");
            patrolParent.transform.SetParent(parent);
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var pp = new GameObject($"PP_{i}");
                pp.transform.SetParent(patrolParent.transform);
                pp.transform.position = patrolPoints[i];
            }

            var mat = CreateURPMaterial(color);
            mat.SetFloat("_Smoothness", 0.6f);
            monster.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, $"M_{name}");

            if (type == "mecha")
            {
                var eyeLight = new GameObject("EyeLight");
                eyeLight.transform.SetParent(monster.transform);
                eyeLight.transform.localPosition = new Vector3(0f, 0.3f, 0.6f);
                var light = eyeLight.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = Color.green;
                light.intensity = 2f;
                light.range = 8f;
                light.spotAngle = 30f;
            }
        }

        private static void CreateGoal()
        {
            var goal = new GameObject("Goal");
            goal.transform.position = new Vector3(0f, 0.5f, -25f);

            var col = goal.AddComponent<BoxCollider>();
            col.size = new Vector3(4f, 3f, 2f);
            col.isTrigger = true;

            goal.AddComponent<GoalZone>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "GoalVisual";
            visual.transform.SetParent(goal.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 0.1f, 2f);
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

            var mat = CreateURPMaterial(new Color(0f, 1f, 0.5f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 1f, 0.5f) * 2f);
            visual.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_Goal");

            var lightObj = new GameObject("GoalLight");
            lightObj.transform.SetParent(goal.transform);
            lightObj.transform.localPosition = Vector3.up * 2f;
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0f, 1f, 0.5f);
            light.intensity = 3f;
            light.range = 8f;
        }

        private static void CreatePickups()
        {
            var pickupParent = new GameObject("Pickups");

            Vector3[] positions = {
                new Vector3(4f, 0.5f, 12f),
                new Vector3(-3f, 0.5f, -7f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = $"WafflePickup_{i}";
                pickup.transform.SetParent(pickupParent.transform);
                pickup.transform.position = positions[i];
                pickup.transform.localScale = Vector3.one * 0.4f;

                Object.DestroyImmediate(pickup.GetComponent<SphereCollider>());
                var sc = pickup.AddComponent<SphereCollider>();
                sc.radius = 1.5f;
                sc.isTrigger = true;

                pickup.AddComponent<WafflePickup>();

                var mat = CreateURPMaterial(new Color(1f, 0.85f, 0.5f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.5f) * 1.5f);
                pickup.GetComponent<Renderer>().material = mat;
            }
        }

        private static void SetupCamera()
        {
            var mainCam = Camera.main;
            var player = GameObject.Find("Player");

            if (mainCam != null && player != null)
            {
                // カメラをプレイヤーの子に設定（FPSモード）
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = new Vector3(0f, 0.7f, 0.2f); // 目の位置
                mainCam.transform.localRotation = Quaternion.identity;

                mainCam.gameObject.AddComponent<FollowCamera>();
                mainCam.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
                mainCam.fieldOfView = 70f;
                mainCam.nearClipPlane = 0.1f;

                var urpData = mainCam.GetComponent<UniversalAdditionalCameraData>();
                if (urpData == null)
                {
                    urpData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }
                urpData.renderPostProcessing = true;

                // Playerのレンダラーを非表示（FPSでは自分が見えない）
                var playerRenderer = player.GetComponent<Renderer>();
                if (playerRenderer != null)
                {
                    playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
        }

        private static Material _baseMaterial;

        private static Material CreateURPMaterial(Color color)
        {
            if (_baseMaterial == null)
            {
                // 方法1: RenderPipelineのデフォルトマテリアルを使用
                var rpAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                if (rpAsset != null)
                {
                    _baseMaterial = rpAsset.defaultMaterial;
                    Debug.Log($"[GameSceneBuilder] Using RenderPipeline default material: {_baseMaterial?.shader?.name}");
                }

                // 方法2: プロジェクト内の既存Litマテリアルを検索
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
                            Debug.Log($"[GameSceneBuilder] Found existing Lit material at: {path}");
                            break;
                        }
                    }
                }

                // 方法3: 最終フォールバック
                if (_baseMaterial == null)
                {
                    _baseMaterial = new Material(Shader.Find("Standard"));
                    Debug.LogWarning("[GameSceneBuilder] URP material not found, using Standard shader");
                }
            }

            var mat = new Material(_baseMaterial);
            mat.color = color;
            return mat;
        }

        private static void SaveMaterial(Material mat, string name)
        {
            string dirPath = "Assets/Materials";
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
                AssetDatabase.Refresh();
            }
            string path = $"{dirPath}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
            {
                AssetDatabase.CreateAsset(mat, path);
            }
        }
    }
}
