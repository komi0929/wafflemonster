using UnityEngine;
using UnityEditor;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// モンスター複合ボディ自動構築
    /// プリミティブの合成で個性的なキャラクターを表現
    /// </summary>
    public static class MonsterBodyBuilder
    {
        // ─── Whi-chan（ゴーストモンスター） ───
        // 白い球体 + 紫の腹部 + 赤い目 + 舌 + 揺らぎアニメ
        public static void BuildGhostBody(GameObject root, Material baseMat)
        {
            // メインボディ（楕円形ゴースト）
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.8f, 2.2f, 1.5f);
            var bodyMat = new Material(baseMat);
            bodyMat.color = new Color(0.92f, 0.92f, 0.95f);
            bodyMat.EnableKeyword("_EMISSION");
            bodyMat.SetColor("_EmissionColor", new Color(0.3f, 0.25f, 0.4f) * 0.3f);
            bodyMat.SetFloat("_Smoothness", 0.85f);
            body.GetComponent<Renderer>().material = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // 腹部（紫のエミッシブ渦）
            var belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            belly.name = "Belly_Vortex";
            belly.transform.SetParent(root.transform);
            belly.transform.localPosition = new Vector3(0f, -0.3f, 0.5f);
            belly.transform.localScale = new Vector3(1.0f, 1.0f, 0.6f);
            var bellyMat = new Material(baseMat);
            bellyMat.color = new Color(0.3f, 0.05f, 0.5f);
            bellyMat.EnableKeyword("_EMISSION");
            bellyMat.SetColor("_EmissionColor", new Color(0.5f, 0.1f, 0.8f) * 2f);
            bellyMat.SetFloat("_Smoothness", 0.95f);
            belly.GetComponent<Renderer>().material = bellyMat;
            Object.DestroyImmediate(belly.GetComponent<Collider>());

            // 左目
            CreateEye(root.transform, new Vector3(-0.35f, 0.5f, 0.65f), 0.2f,
                      new Color(0.9f, 0.1f, 0.1f), baseMat);

            // 右目
            CreateEye(root.transform, new Vector3(0.35f, 0.5f, 0.65f), 0.2f,
                      new Color(0.9f, 0.1f, 0.1f), baseMat);

            // 口（開いた黒い楕円）
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mouth.name = "Mouth";
            mouth.transform.SetParent(root.transform);
            mouth.transform.localPosition = new Vector3(0f, -0.1f, 0.7f);
            mouth.transform.localScale = new Vector3(0.8f, 0.6f, 0.4f);
            var mouthMat = new Material(baseMat);
            mouthMat.color = new Color(0.05f, 0.02f, 0.05f);
            mouth.GetComponent<Renderer>().material = mouthMat;
            Object.DestroyImmediate(mouth.GetComponent<Collider>());

            // 舌
            var tongue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tongue.name = "Tongue";
            tongue.transform.SetParent(root.transform);
            tongue.transform.localPosition = new Vector3(0.1f, -0.25f, 0.8f);
            tongue.transform.localScale = new Vector3(0.3f, 0.1f, 0.4f);
            var tongueMat = new Material(baseMat);
            tongueMat.color = new Color(0.9f, 0.3f, 0.5f);
            tongue.GetComponent<Renderer>().material = tongueMat;
            Object.DestroyImmediate(tongue.GetComponent<Collider>());

            // ゴースト裾（揺らぐ下部）
            for (int i = 0; i < 5; i++)
            {
                var tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tail.name = $"Tail_{i}";
                tail.transform.SetParent(root.transform);
                float angle = (i - 2f) * 0.3f;
                tail.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 0.5f, -1.3f - i * 0.05f, Mathf.Cos(angle) * 0.2f
                );
                tail.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                var tailMat = new Material(baseMat);
                tailMat.color = new Color(0.85f, 0.85f, 0.9f, 0.7f);
                tailMat.EnableKeyword("_EMISSION");
                tailMat.SetColor("_EmissionColor", new Color(0.3f, 0.25f, 0.4f) * 0.2f);
                tail.GetComponent<Renderer>().material = tailMat;
                Object.DestroyImmediate(tail.GetComponent<Collider>());
            }

            // メインコライダーはrootから（MonsterBaseが管理）
            var col = root.GetComponent<CapsuleCollider>();
            if (col == null) col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0, 0);
            col.radius = 0.9f;
            col.height = 3f;
        }

        // ─── Mecha-paku（メカモンスター） ───
        // メタリック球体 + アンテナ + 多関節腕 + 緑目
        public static void BuildMechaBody(GameObject root, Material baseMat)
        {
            // メインボディ（メタリック球）
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.6f, 1.6f, 1.4f);
            var bodyMat = new Material(baseMat);
            bodyMat.color = new Color(0.5f, 0.5f, 0.55f);
            bodyMat.SetFloat("_Metallic", 0.9f);
            bodyMat.SetFloat("_Smoothness", 0.8f);
            body.GetComponent<Renderer>().material = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // 頭頂アンテナ（3本）
            for (int i = 0; i < 3; i++)
            {
                float angle = (i - 1) * 0.4f;
                var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antenna.name = $"Antenna_{i}";
                antenna.transform.SetParent(root.transform);
                antenna.transform.localPosition = new Vector3(Mathf.Sin(angle) * 0.3f, 1.2f, 0f);
                antenna.transform.localScale = new Vector3(0.06f, 0.4f, 0.06f);
                antenna.transform.localRotation = Quaternion.Euler(0, 0, angle * 20f);
                var aMat = new Material(baseMat);
                aMat.color = new Color(0.3f, 0.3f, 0.35f);
                aMat.SetFloat("_Metallic", 1f);
                antenna.GetComponent<Renderer>().material = aMat;
                Object.DestroyImmediate(antenna.GetComponent<Collider>());

                // アンテナ先端の光
                var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = $"AntennaTip_{i}";
                tip.transform.SetParent(antenna.transform);
                tip.transform.localPosition = new Vector3(0, 1.1f, 0);
                tip.transform.localScale = new Vector3(2f, 0.8f, 2f);
                var tipMat = new Material(baseMat);
                tipMat.color = new Color(0f, 0.8f, 0.2f);
                tipMat.EnableKeyword("_EMISSION");
                tipMat.SetColor("_EmissionColor", new Color(0f, 1f, 0.3f) * 2f);
                tip.GetComponent<Renderer>().material = tipMat;
                Object.DestroyImmediate(tip.GetComponent<Collider>());
            }

            // 目（3つ目 — メカらしく）
            CreateEye(root.transform, new Vector3(-0.4f, 0.3f, 0.6f), 0.22f,
                      new Color(0f, 1f, 0.3f), baseMat);
            CreateEye(root.transform, new Vector3(0.4f, 0.3f, 0.6f), 0.22f,
                      new Color(0f, 1f, 0.3f), baseMat);
            CreateEye(root.transform, new Vector3(0f, 0.7f, 0.55f), 0.18f,
                      new Color(1f, 0.5f, 0f), baseMat); // 第3の目

            // 口（メカジョー）
            var jaw = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jaw.name = "Jaw";
            jaw.transform.SetParent(root.transform);
            jaw.transform.localPosition = new Vector3(0f, -0.3f, 0.6f);
            jaw.transform.localScale = new Vector3(0.8f, 0.3f, 0.4f);
            var jawMat = new Material(baseMat);
            jawMat.color = new Color(0.15f, 0.15f, 0.18f);
            jawMat.SetFloat("_Metallic", 0.95f);
            jaw.GetComponent<Renderer>().material = jawMat;
            Object.DestroyImmediate(jaw.GetComponent<Collider>());

            // 腕 x4（スプーン/フォーク的なロボアーム）
            Vector3[] armPositions = {
                new Vector3(-0.9f, 0.2f, 0.2f),
                new Vector3(0.9f, 0.2f, 0.2f),
                new Vector3(-0.7f, -0.2f, 0.3f),
                new Vector3(0.7f, -0.2f, 0.3f),
            };
            float[] armAngles = { 30f, -30f, 45f, -45f };

            for (int i = 0; i < 4; i++)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = $"Arm_{i}";
                arm.transform.SetParent(root.transform);
                arm.transform.localPosition = armPositions[i];
                arm.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
                arm.transform.localRotation = Quaternion.Euler(0, 0, armAngles[i]);
                var armMat = new Material(baseMat);
                armMat.color = new Color(0.4f, 0.4f, 0.45f);
                armMat.SetFloat("_Metallic", 0.9f);
                arm.GetComponent<Renderer>().material = armMat;
                Object.DestroyImmediate(arm.GetComponent<Collider>());

                // 手先（スプーン的な球）
                var hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hand.name = $"Hand_{i}";
                hand.transform.SetParent(arm.transform);
                hand.transform.localPosition = new Vector3(0, -1.2f, 0);
                hand.transform.localScale = new Vector3(3f, 1.5f, 3f);
                var handMat = new Material(baseMat);
                handMat.color = new Color(0.6f, 0.6f, 0.65f);
                handMat.SetFloat("_Metallic", 1f);
                handMat.SetFloat("_Smoothness", 0.9f);
                hand.GetComponent<Renderer>().material = handMat;
                Object.DestroyImmediate(hand.GetComponent<Collider>());
            }

            var col2 = root.GetComponent<CapsuleCollider>();
            if (col2 == null) col2 = root.AddComponent<CapsuleCollider>();
            col2.center = new Vector3(0, 0, 0);
            col2.radius = 0.8f;
            col2.height = 2.5f;
        }

        // ─── Fuzz-nom（毛玉モンスター） ───
        // 紫の毛玉 + 3つ目 + ピンクの舌 + もふもふ球体群
        public static void BuildFuzzyBody(GameObject root, Material baseMat)
        {
            Color furColor = new Color(0.5f, 0.15f, 0.55f);

            // メインボディ
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.8f, 1.6f, 1.6f);
            var bodyMat = new Material(baseMat);
            bodyMat.color = furColor;
            bodyMat.SetFloat("_Smoothness", 0.2f); // マット（毛っぽい）
            body.GetComponent<Renderer>().material = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // もふもふ球（周囲に小球を付けて毛玉感）
            System.Random rng = new System.Random(99);
            for (int i = 0; i < 20; i++)
            {
                var fluff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fluff.name = $"Fluff_{i}";
                fluff.transform.SetParent(root.transform);

                float theta = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float phi = (float)(rng.NextDouble() * Mathf.PI - Mathf.PI / 2f);
                float r = 0.75f + (float)rng.NextDouble() * 0.15f;
                float px = Mathf.Cos(phi) * Mathf.Cos(theta) * r;
                float py = Mathf.Sin(phi) * r * 0.8f;
                float pz = Mathf.Cos(phi) * Mathf.Sin(theta) * r;

                fluff.transform.localPosition = new Vector3(px, py, pz);
                float s = 0.2f + (float)rng.NextDouble() * 0.25f;
                fluff.transform.localScale = new Vector3(s, s, s);

                var fluffMat = new Material(baseMat);
                float shade = 0.85f + (float)rng.NextDouble() * 0.3f;
                fluffMat.color = furColor * shade;
                fluffMat.SetFloat("_Smoothness", 0.15f);
                fluff.GetComponent<Renderer>().material = fluffMat;
                Object.DestroyImmediate(fluff.GetComponent<Collider>());
            }

            // 目 x3（異なるサイズ）
            CreateEye(root.transform, new Vector3(-0.35f, 0.35f, 0.6f), 0.25f,
                      new Color(1f, 0.5f, 0f), baseMat);
            CreateEye(root.transform, new Vector3(0.3f, 0.45f, 0.55f), 0.2f,
                      new Color(1f, 0.5f, 0f), baseMat);
            CreateEye(root.transform, new Vector3(0.05f, 0.6f, 0.5f), 0.15f,
                      new Color(1f, 0.3f, 0f), baseMat);

            // 大きな口
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mouth.name = "Mouth";
            mouth.transform.SetParent(root.transform);
            mouth.transform.localPosition = new Vector3(0f, -0.15f, 0.65f);
            mouth.transform.localScale = new Vector3(0.9f, 0.5f, 0.4f);
            var mouthMat = new Material(baseMat);
            mouthMat.color = new Color(0.15f, 0.02f, 0.08f);
            mouth.GetComponent<Renderer>().material = mouthMat;
            Object.DestroyImmediate(mouth.GetComponent<Collider>());

            // 舌
            var tongue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tongue.name = "Tongue";
            tongue.transform.SetParent(root.transform);
            tongue.transform.localPosition = new Vector3(-0.1f, -0.3f, 0.85f);
            tongue.transform.localScale = new Vector3(0.35f, 0.12f, 0.5f);
            var tongueMat = new Material(baseMat);
            tongueMat.color = new Color(1f, 0.4f, 0.6f);
            tongue.GetComponent<Renderer>().material = tongueMat;
            Object.DestroyImmediate(tongue.GetComponent<Collider>());

            // 尻尾（カールした球体チェーン）
            for (int i = 0; i < 6; i++)
            {
                var tailSeg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tailSeg.name = $"Tail_{i}";
                tailSeg.transform.SetParent(root.transform);
                float t = i / 5f;
                tailSeg.transform.localPosition = new Vector3(
                    Mathf.Sin(t * 3f) * 0.3f,
                    -0.3f - t * 0.3f,
                    -0.7f - t * 0.5f
                );
                float ts = 0.25f - t * 0.03f;
                tailSeg.transform.localScale = new Vector3(ts, ts, ts);
                var tailMat = new Material(baseMat);
                tailMat.color = furColor * (1f - t * 0.2f);
                tailMat.SetFloat("_Smoothness", 0.15f);
                tailSeg.GetComponent<Renderer>().material = tailMat;
                Object.DestroyImmediate(tailSeg.GetComponent<Collider>());
            }

            var col3 = root.GetComponent<CapsuleCollider>();
            if (col3 == null) col3 = root.AddComponent<CapsuleCollider>();
            col3.center = new Vector3(0, 0, 0);
            col3.radius = 0.9f;
            col3.height = 2.5f;
        }

        // ─── 共通: 目の生成 ───
        private static void CreateEye(Transform parent, Vector3 pos, float size,
                                       Color irisColor, Material baseMat)
        {
            // 白目
            var eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.name = "Eye_White";
            eyeWhite.transform.SetParent(parent);
            eyeWhite.transform.localPosition = pos;
            eyeWhite.transform.localScale = Vector3.one * size;
            var whiteMat = new Material(baseMat);
            whiteMat.color = Color.white;
            whiteMat.SetFloat("_Smoothness", 0.95f);
            eyeWhite.GetComponent<Renderer>().material = whiteMat;
            Object.DestroyImmediate(eyeWhite.GetComponent<Collider>());

            // 瞳
            var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Eye_Pupil";
            pupil.transform.SetParent(eyeWhite.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.35f);
            pupil.transform.localScale = Vector3.one * 0.6f;
            var pupilMat = new Material(baseMat);
            pupilMat.color = irisColor;
            pupilMat.EnableKeyword("_EMISSION");
            pupilMat.SetColor("_EmissionColor", irisColor * 1.5f);
            pupil.GetComponent<Renderer>().material = pupilMat;
            Object.DestroyImmediate(pupil.GetComponent<Collider>());

            // 黒目
            var center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "Eye_Center";
            center.transform.SetParent(pupil.transform);
            center.transform.localPosition = new Vector3(0, 0, 0.35f);
            center.transform.localScale = Vector3.one * 0.5f;
            var centerMat = new Material(baseMat);
            centerMat.color = Color.black;
            center.GetComponent<Renderer>().material = centerMat;
            Object.DestroyImmediate(center.GetComponent<Collider>());
        }
    }
}
