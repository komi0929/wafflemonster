using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Soyya.WaffleMonster.Editor
{
    /// <summary>
    /// ゲームシーンをプログラムで構築するエディタスクリプト
    /// メニュー: Soyya > Setup Game Scene
    /// </summary>
    public class GameSceneBuilder : UnityEditor.Editor
    {
        [MenuItem("Soyya/シーンセットアップ/ネオン裏路地を生成")]
        public static void BuildNeonAlleyScene()
        {
            Debug.Log("[GameSceneBuilder] ネオン裏路地シーン生成開始...");

            // 地面
            CreateGround();

            // 壁と建物
            CreateBuildings();

            // ネオンライティング
            CreateNeonLighting();

            // プレイヤー
            CreatePlayer();

            // モンスター配置
            CreateMonsters();

            // ゴール
            CreateGoal();

            // ワッフルピックアップ
            CreatePickups();

            // カメラ
            SetupCamera();

            // NavMeshのベイクが必要
            Debug.Log("[GameSceneBuilder] シーン生成完了！");
            Debug.Log("[GameSceneBuilder] ⚠ Window > AI > Navigation からNavMeshをベイクしてください");
        }

        private static void CreateGround()
        {
            // 地面（暗いアスファルト）
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 8f);
            ground.isStatic = true;

            // Navigation Static設定
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

            // マテリアル（濡れたアスファルト）
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.08f, 0.08f, 0.1f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.3f);
            ground.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_WetAsphalt");
        }

        private static void CreateBuildings()
        {
            var buildingParent = new GameObject("Buildings");

            // 建物データ: position, scale
            Vector3[][] buildingLayout = new Vector3[][]
            {
                // 左側の建物群
                new Vector3[] { new Vector3(-8f, 3f, 15f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(-8f, 4f, 5f), new Vector3(6f, 8f, 4f) },
                new Vector3[] { new Vector3(-8f, 3.5f, -5f), new Vector3(6f, 7f, 5f) },
                new Vector3[] { new Vector3(-8f, 3f, -18f), new Vector3(6f, 6f, 6f) },

                // 右側の建物群
                new Vector3[] { new Vector3(8f, 3f, 15f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(8f, 4.5f, 5f), new Vector3(6f, 9f, 4f) },
                new Vector3[] { new Vector3(8f, 3f, -5f), new Vector3(6f, 6f, 5f) },
                new Vector3[] { new Vector3(8f, 3.5f, -18f), new Vector3(6f, 7f, 6f) },

                // 中央の障害物（屋台風）
                new Vector3[] { new Vector3(0f, 1f, 10f), new Vector3(2f, 2f, 3f) },
                new Vector3[] { new Vector3(-2f, 1.5f, 0f), new Vector3(3f, 3f, 2f) },
                new Vector3[] { new Vector3(2f, 1f, -10f), new Vector3(2.5f, 2f, 2.5f) },
            };

            var wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.12f, 0.1f, 0.15f);
            SaveMaterial(wallMat, "M_DarkWall");

            for (int i = 0; i < buildingLayout.Length; i++)
            {
                var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{i}";
                building.transform.SetParent(buildingParent.transform);
                building.transform.position = buildingLayout[i][0];
                building.transform.localScale = buildingLayout[i][1];
                building.isStatic = true;
                GameObjectUtility.SetStaticEditorFlags(building, StaticEditorFlags.NavigationStatic);
                building.GetComponent<Renderer>().material = wallMat;
            }
        }

        private static void CreateNeonLighting()
        {
            var lightParent = new GameObject("Lighting");

            // メインの環境光を暗く
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.05f);

            // フォグ設定
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.05f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.03f;

            // Directional Light（月明かり）
            var dirLight = GameObject.Find("Directional Light");
            if (dirLight != null)
            {
                var light = dirLight.GetComponent<Light>();
                light.color = new Color(0.1f, 0.1f, 0.3f);
                light.intensity = 0.3f;
                light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            }

            // ネオンライト（ピンク、シアン、イエロー、パープル）
            Color[] neonColors = {
                new Color(1f, 0.2f, 0.8f),   // ピンク
                new Color(0f, 0.8f, 1f),      // シアン
                new Color(1f, 0.8f, 0f),      // イエロー
                new Color(0.6f, 0.2f, 1f),    // パープル
                new Color(1f, 0.3f, 0.1f),    // オレンジ
                new Color(0f, 1f, 0.5f),      // グリーン
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

                // NeonSignスクリプト
                var neon = lightObj.AddComponent<NeonSign>();
            }

            // 提灯ライト
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

                var lantern = lanternObj.AddComponent<Lantern>();
            }
        }

        private static void CreatePlayer()
        {
            // プレイヤー（カプセル）
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 20f);
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            // CharacterController
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            // PlayerController
            player.AddComponent<PlayerController>();

            // マテリアル
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.8f, 0.5f);
            mat.SetFloat("_Smoothness", 0.3f);
            player.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_Player");

            // 投擲ポイント
            var throwPoint = new GameObject("ThrowPoint");
            throwPoint.transform.SetParent(player.transform);
            throwPoint.transform.localPosition = new Vector3(0f, 0.8f, 1f);
        }

        private static void CreateMonsters()
        {
            var monsterParent = new GameObject("Monsters");

            // GhostMonster
            CreateMonster<GhostMonster>(
                "Whi-chan",
                new Vector3(0f, 1f, 8f),
                new Color(0.9f, 0.95f, 1f, 0.6f),
                monsterParent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, 10f),
                    new Vector3(3f, 0.5f, 6f),
                    new Vector3(0f, 0.5f, 8f)
                }
            );

            // MechaMonster
            CreateMonster<MechaMonster>(
                "Mecha-paku",
                new Vector3(-2f, 1f, -2f),
                new Color(0.5f, 0.55f, 0.6f),
                monsterParent.transform,
                new Vector3[] {
                    new Vector3(-3f, 0.5f, -4f),
                    new Vector3(3f, 0.5f, 0f),
                    new Vector3(0f, 0.5f, -2f)
                }
            );

            // FuzzyMonster
            CreateMonster<FuzzyMonster>(
                "Fuzz-nom",
                new Vector3(2f, 1f, -12f),
                new Color(0.6f, 0.3f, 0.7f),
                monsterParent.transform,
                new Vector3[] {
                    new Vector3(3f, 0.5f, -14f),
                    new Vector3(-3f, 0.5f, -10f),
                    new Vector3(0f, 0.5f, -12f)
                }
            );
        }

        private static void CreateMonster<T>(string name, Vector3 pos, Color color,
            Transform parent, Vector3[] patrolPoints) where T : MonsterBase
        {
            var monster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            monster.name = name;
            monster.transform.SetParent(parent);
            monster.transform.position = pos;
            monster.transform.localScale = Vector3.one * 1.5f;

            // NavMeshAgent
            var agent = monster.AddComponent<NavMeshAgent>();
            agent.speed = 2f;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.6f;
            agent.height = 1.5f;

            // モンスターコンポーネント
            var monsterComp = monster.AddComponent<T>();

            // パトロールポイント
            var patrolParent = new GameObject($"{name}_PatrolPoints");
            patrolParent.transform.SetParent(parent);
            Transform[] patrols = new Transform[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var pp = new GameObject($"PP_{i}");
                pp.transform.SetParent(patrolParent.transform);
                pp.transform.position = patrolPoints[i];
                patrols[i] = pp.transform;
            }

            // マテリアル
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.6f);
            monster.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, $"M_{name}");

            // 目のライト（ロボット用）
            if (typeof(T) == typeof(MechaMonster))
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

            // トリガーコライダー
            var col = goal.AddComponent<BoxCollider>();
            col.size = new Vector3(4f, 3f, 2f);
            col.isTrigger = true;

            // GoalZone
            goal.AddComponent<GoalZone>();

            // ビジュアル（光る柱）
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "GoalVisual";
            visual.transform.SetParent(goal.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 0.1f, 2f);
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0f, 1f, 0.5f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 1f, 0.5f) * 2f);
            visual.GetComponent<Renderer>().material = mat;
            SaveMaterial(mat, "M_Goal");

            // ゴールライト
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
                var col = pickup.AddComponent<SphereCollider>();
                col.radius = 1.5f;
                col.isTrigger = true;

                pickup.AddComponent<WafflePickup>();

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(1f, 0.85f, 0.5f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.5f) * 1.5f);
                pickup.GetComponent<Renderer>().material = mat;
            }
        }

        private static void SetupCamera()
        {
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.gameObject.AddComponent<FollowCamera>();
                mainCam.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
                mainCam.fieldOfView = 60f;

                // URP追加データ
                var urpData = mainCam.GetComponent<UniversalAdditionalCameraData>();
                if (urpData == null)
                {
                    urpData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }
                urpData.renderPostProcessing = true;
            }
        }

        private static void SaveMaterial(Material mat, string name)
        {
            string path = $"Assets/Materials/{name}.mat";
            if (!System.IO.Directory.Exists("Assets/Materials"))
            {
                System.IO.Directory.CreateDirectory("Assets/Materials");
            }
            if (!AssetDatabase.LoadAssetAtPath<Material>(path))
            {
                AssetDatabase.CreateAsset(mat, path);
            }
        }
    }
}
