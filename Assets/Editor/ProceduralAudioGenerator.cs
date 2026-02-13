using UnityEngine;
using UnityEditor;
using System.IO;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// プロシージャル8bit風SE生成
    /// メニュー: Soyya > アセット生成 > SE/BGMを生成
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        private const string AUDIO_DIR = "Assets/Audio";
        private const int SAMPLE_RATE = 44100;

        [MenuItem("Soyya/アセット生成/SEを生成")]
        public static void GenerateAll()
        {
            EnsureDirectory(AUDIO_DIR);
            GenerateThrowSE();
            GenerateHitSE();
            GeneratePickupSE();
            GenerateCaughtSE();
            GenerateGoalSE();
            GenerateCountdownBeep();
            GenerateMonsterPakuSE();
            AssetDatabase.Refresh();
            Debug.Log("[AudioGen] 全SE生成完了");
        }

        // ─── 投擲音（シュッ） ───
        private static void GenerateThrowSE()
        {
            float duration = 0.15f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(800f, 2000f, t);
                float noise = (Random.value - 0.5f) * 0.3f;
                float env = (1f - t) * (1f - t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f + noise) * env;
            }

            SaveAudioClip(data, "SE_Throw");
        }

        // ─── 命中音（パコッ） ───
        private static void GenerateHitSE()
        {
            float duration = 0.25f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Exp(-t * 15f);
                float freq1 = 300f * Mathf.Pow(2f, -t * 2f);
                float freq2 = 150f;
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.6f
                          + Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.3f
                          + (Random.value - 0.5f) * 0.2f) * env;
            }

            SaveAudioClip(data, "SE_Hit");
        }

        // ─── ピックアップ音（キラキラ上昇） ───
        private static void GeneratePickupSE()
        {
            float duration = 0.3f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(600f, 1800f, t * t);
                float env = Mathf.Sin(t * Mathf.PI);
                float arp = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
                float harmonics = Mathf.Sin(4f * Mathf.PI * freq * t) * 0.2f;
                data[i] = (arp + harmonics) * env * 0.7f;
            }

            SaveAudioClip(data, "SE_Pickup");
        }

        // ─── 被捕獲音（ブー低音） ───
        private static void GenerateCaughtSE()
        {
            float duration = 0.5f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = 1f - t;
                float freq = Mathf.Lerp(200f, 80f, t);
                // 矩形波
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.3f;
                float wobble = Mathf.Sin(2f * Mathf.PI * 5f * t) * 0.3f;
                data[i] = (square + wobble * Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t)) * env * 0.6f;
            }

            SaveAudioClip(data, "SE_Caught");
        }

        // ─── ゴール音（ファンファーレ上昇） ───
        private static void GenerateGoalSE()
        {
            float duration = 0.8f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            float[] notes = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
            float noteLen = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int noteIdx = Mathf.Min((int)(t / noteLen), notes.Length - 1);
                float noteT = (t - noteIdx * noteLen) / noteLen;
                float env = Mathf.Sin(noteT * Mathf.PI) * (1f - (float)noteIdx / notes.Length * 0.3f);
                float freq = notes[noteIdx];
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f
                          + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.15f
                          + Mathf.Sin(6f * Mathf.PI * freq * t) * 0.08f) * env * 0.8f;
            }

            SaveAudioClip(data, "SE_Goal");
        }

        // ─── カウントダウンビープ ───
        private static void GenerateCountdownBeep()
        {
            float duration = 0.1f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = 1f - t;
                data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * env * 0.5f;
            }

            SaveAudioClip(data, "SE_Countdown");
        }

        // ─── モンスター「パク」音 ───
        private static void GenerateMonsterPakuSE()
        {
            float duration = 0.3f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Exp(-t * 8f);
                // 「パク」: 短いパルス + 下降音
                float freq = Mathf.Lerp(500f, 200f, t);
                float pulse = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                data[i] = pulse * env * 0.4f * (t < 0.05f ? 2f : 1f);
            }

            SaveAudioClip(data, "SE_Paku");
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

        private static void SaveAudioClip(float[] data, string name)
        {
            string path = $"{AUDIO_DIR}/{name}.wav";

            // WAVファイル書き出し
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                int subChunk2Size = data.Length * 2; // 16-bit PCM

                // WAVヘッダー
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + subChunk2Size);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);           // chunk size
                writer.Write((short)1);     // PCM
                writer.Write((short)1);     // mono
                writer.Write(SAMPLE_RATE);
                writer.Write(SAMPLE_RATE * 2); // byte rate
                writer.Write((short)2);     // block align
                writer.Write((short)16);    // bits per sample

                // data chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(subChunk2Size);

                for (int i = 0; i < data.Length; i++)
                {
                    short val = (short)(Mathf.Clamp(data[i], -1f, 1f) * 32767f);
                    writer.Write(val);
                }
            }

            Debug.Log($"[AudioGen] {name} 生成完了: {path}");
        }
    }
}
