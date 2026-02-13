using UnityEngine;
using UnityEditor;
using System.IO;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// プロシージャルテクスチャ生成
    /// メニュー: Soyya > アセット生成 > テクスチャを生成
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        private const string TEX_DIR = "Assets/Textures";

        [MenuItem("Soyya/アセット生成/テクスチャを生成")]
        public static void GenerateAll()
        {
            EnsureDirectory(TEX_DIR);
            GenerateWaffleTexture();
            GenerateWaffleNormal();
            GenerateBuildingWindowTexture();
            GenerateWetAsphaltNormal();
            GenerateNeonSignTextures();
            GenerateGroundTexture();
            AssetDatabase.Refresh();
            Debug.Log("[TextureGen] 全テクスチャ生成完了");
        }

        // ─── ワッフル球 アルベド ───
        public static Texture2D GenerateWaffleTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.85f, 0.65f, 0.35f);
            Color darkColor = new Color(0.6f, 0.4f, 0.2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // ワッフル格子パターン
                    float gridX = Mathf.Abs(Mathf.Sin(u * Mathf.PI * 12f));
                    float gridY = Mathf.Abs(Mathf.Sin(v * Mathf.PI * 12f));
                    float grid = Mathf.Min(gridX, gridY);
                    grid = Mathf.Pow(grid, 0.3f);

                    // ノイズで自然な焼き色
                    float noise = Mathf.PerlinNoise(u * 8f + 42f, v * 8f + 17f) * 0.15f;
                    float bakeNoise = Mathf.PerlinNoise(u * 3f, v * 3f) * 0.1f;

                    Color c = Color.Lerp(darkColor, baseColor, grid);
                    c -= new Color(noise, noise * 0.8f, noise * 0.5f, 0f);
                    c -= new Color(bakeNoise, bakeNoise, bakeNoise, 0f);
                    c.a = 1f;

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return SaveTexture(tex, "T_Waffle_Albedo");
        }

        // ─── ワッフル球 ノーマルマップ ───
        public static Texture2D GenerateWaffleNormal()
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // 格子のバンプ
                    float bumpX = Mathf.Cos(u * Mathf.PI * 12f) * 0.5f;
                    float bumpY = Mathf.Cos(v * Mathf.PI * 12f) * 0.5f;

                    // ノイズディテール
                    float noiseX = (Mathf.PerlinNoise(u * 20f + 0.5f, v * 20f) - 0.5f) * 0.2f;
                    float noiseY = (Mathf.PerlinNoise(u * 20f, v * 20f + 0.5f) - 0.5f) * 0.2f;

                    float nx = Mathf.Clamp01((bumpX + noiseX) * 0.5f + 0.5f);
                    float ny = Mathf.Clamp01((bumpY + noiseY) * 0.5f + 0.5f);

                    tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
                }
            }

            tex.Apply();
            return SaveTexture(tex, "T_Waffle_Normal");
        }

        // ─── ビル壁面（窓グリッド）───
        public static Texture2D GenerateBuildingWindowTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color wallColor = new Color(0.08f, 0.06f, 0.1f);
            Color windowDark = new Color(0.02f, 0.02f, 0.04f);

            Color[] windowColors = {
                new Color(0.1f, 0.3f, 0.6f, 1f),   // 青い窓
                new Color(0.6f, 0.3f, 0.1f, 1f),   // オレンジ窓
                new Color(0.05f, 0.5f, 0.4f, 1f),  // ティール窓
                new Color(0.5f, 0.1f, 0.4f, 1f),   // マゼンタ窓
            };

            int gridX = 8;
            int gridY = 12;
            int windowW = size / gridX;
            int windowH = size / gridY;
            int margin = 4;

            // 壁面ベース
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.03f;
                    tex.SetPixel(x, y, wallColor + new Color(n, n, n, 0f));
                }

            // 窓
            System.Random rng = new System.Random(42);
            for (int gy = 0; gy < gridY; gy++)
            {
                for (int gx = 0; gx < gridX; gx++)
                {
                    bool lit = rng.NextDouble() > 0.35;
                    Color wc = lit ? windowColors[rng.Next(windowColors.Length)] : windowDark;
                    float brightness = lit ? (float)(rng.NextDouble() * 0.4 + 0.3) : 0.05f;
                    wc *= brightness;
                    wc.a = 1f;

                    int sx = gx * windowW + margin;
                    int sy = gy * windowH + margin;
                    int ex = (gx + 1) * windowW - margin;
                    int ey = (gy + 1) * windowH - margin;

                    for (int y = sy; y < ey && y < size; y++)
                        for (int x = sx; x < ex && x < size; x++)
                            tex.SetPixel(x, y, wc);
                }
            }

            tex.Apply();
            return SaveTexture(tex, "T_Building_Windows");
        }

        // ─── 濡れたアスファルト ノーマルマップ ───
        public static Texture2D GenerateWetAsphaltNormal()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // マルチスケールノイズ
                    float n1 = Mathf.PerlinNoise(u * 30f, v * 30f) * 0.5f;
                    float n2 = Mathf.PerlinNoise(u * 60f + 100f, v * 60f + 100f) * 0.3f;
                    float n3 = Mathf.PerlinNoise(u * 120f + 200f, v * 120f + 200f) * 0.2f;

                    float n = n1 + n2 + n3;

                    float nx = Mathf.Clamp01((Mathf.PerlinNoise(u * 40f + 0.5f, v * 40f) - 0.5f) * 0.3f + 0.5f);
                    float ny = Mathf.Clamp01((Mathf.PerlinNoise(u * 40f, v * 40f + 0.5f) - 0.5f) * 0.3f + 0.5f);

                    tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
                }
            }

            tex.Apply();
            return SaveTexture(tex, "T_WetAsphalt_Normal");
        }

        // ─── ネオン看板テクスチャ群 ───
        public static void GenerateNeonSignTextures()
        {
            string[] signs = { "拉麺", "BAR", "茶", "酒場", "OPEN" };
            Color[] colors = {
                new Color(1f, 0.2f, 0.6f),
                new Color(0f, 0.9f, 1f),
                new Color(1f, 0.8f, 0f),
                new Color(0.6f, 0.2f, 1f),
                new Color(1f, 0.4f, 0.1f),
            };

            for (int i = 0; i < signs.Length; i++)
            {
                int w = 256, h = 128;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

                Color bg = new Color(0.01f, 0.01f, 0.02f, 0.9f);
                Color neon = colors[i % colors.Length];

                // 背景
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        tex.SetPixel(x, y, bg);

                // ネオン枠
                int border = 3;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        bool isBorder = x < border || x >= w - border || y < border || y >= h - border;
                        if (isBorder)
                        {
                            float glow = 1f;
                            tex.SetPixel(x, y, neon * glow);
                        }
                    }
                }

                // 中央に擬似文字（光のブロック）
                float charWidth = w * 0.6f;
                float charHeight = h * 0.5f;
                int cx = (int)(w * 0.2f);
                int cy = (int)(h * 0.25f);

                // 各文字をブロックで表現
                int numBlocks = signs[i].Length;
                float blockW = charWidth / numBlocks;

                for (int bi = 0; bi < numBlocks; bi++)
                {
                    int bx = cx + (int)(bi * blockW);
                    int bw = (int)(blockW * 0.7f);

                    for (int y = cy; y < cy + (int)charHeight && y < h; y++)
                    {
                        for (int x = bx; x < bx + bw && x < w; x++)
                        {
                            // 文字形状をハッシュで擬似生成
                            int hash = (signs[i][bi] * 31 + y / 8) & 0xFF;
                            int col = (x - bx) * 8 / bw;
                            int row = (y - cy) * 8 / (int)charHeight;
                            bool filled = ((hash >> (col % 8)) & 1) == 1 || ((hash >> (row % 8)) & 1) == 1;

                            if (filled)
                            {
                                float dist = Mathf.Min(
                                    Mathf.Min(x - bx, bx + bw - x),
                                    Mathf.Min(y - cy, cy + (int)charHeight - y)
                                ) / 5f;
                                float glow = Mathf.Clamp01(dist);
                                tex.SetPixel(x, y, neon * (0.5f + glow * 0.5f));
                            }
                        }
                    }
                }

                tex.Apply();
                SaveTexture(tex, $"T_NeonSign_{i}");
            }
        }

        // ─── 地面テクスチャ（アスファルト）───
        public static Texture2D GenerateGroundTexture()
        {
            int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float n1 = Mathf.PerlinNoise(u * 20f, v * 20f) * 0.08f;
                    float n2 = Mathf.PerlinNoise(u * 50f + 50f, v * 50f + 50f) * 0.04f;
                    float n3 = Mathf.PerlinNoise(u * 100f + 100f, v * 100f + 100f) * 0.02f;

                    float val = 0.06f + n1 + n2 + n3;

                    // 水たまりの反射スポット
                    float puddle = Mathf.PerlinNoise(u * 5f + 7f, v * 5f + 3f);
                    if (puddle > 0.65f)
                    {
                        val *= 0.5f; // 暗い水たまり
                        float reflect = (puddle - 0.65f) * 3f;
                        val += reflect * 0.05f;
                    }

                    tex.SetPixel(x, y, new Color(val, val * 0.95f, val * 1.1f, 1f));
                }
            }

            tex.Apply();
            return SaveTexture(tex, "T_Ground_Albedo");
        }

        // ─── ユーティリティ ───
        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private static Texture2D SaveTexture(Texture2D tex, string name)
        {
            string path = $"{TEX_DIR}/{name}.png";
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);

            // ノーマルマップの設定
            if (name.Contains("Normal"))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"[TextureGen] {name} 生成完了: {path}");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
