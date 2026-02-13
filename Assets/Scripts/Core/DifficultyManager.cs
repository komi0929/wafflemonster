using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// 難易度管理 & スコアランクシステム
    /// クリアタイムに応じたS/A/B/Cランク、ベストタイム記録
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("ランク基準（秒）")]
        [SerializeField] private float _rankSTime = 30f;
        [SerializeField] private float _rankATime = 45f;
        [SerializeField] private float _rankBTime = 60f;

        [Header("難易度スケーリング")]
        [SerializeField] private float _speedMultiplierPerRound = 0.15f;
        [SerializeField] private int _maxDifficultyRound = 5;

        public int CurrentRound { get; private set; } = 1;
        public float BestTime { get; private set; } = -1f;
        public string BestRank { get; private set; } = "";
        public int TotalWafflesThrown { get; set; }
        public int TotalHits { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadBestTime();
        }

        /// <summary>
        /// クリアタイムからランクを算出
        /// </summary>
        public string CalculateRank(float clearTime)
        {
            if (clearTime <= _rankSTime) return "S";
            if (clearTime <= _rankATime) return "A";
            if (clearTime <= _rankBTime) return "B";
            return "C";
        }

        /// <summary>
        /// ゲームクリア時の処理
        /// </summary>
        public void OnGameClear(float clearTime)
        {
            string rank = CalculateRank(clearTime);

            // ベストタイム更新
            if (BestTime < 0f || clearTime < BestTime)
            {
                BestTime = clearTime;
                BestRank = rank;
                SaveBestTime();
            }

            CurrentRound++;
            if (CurrentRound > _maxDifficultyRound)
                CurrentRound = _maxDifficultyRound;

            Debug.Log($"[Difficulty] クリア! タイム: {clearTime:F1}秒 ランク: {rank} ラウンド: {CurrentRound}");
        }

        /// <summary>
        /// 現在の難易度に応じたモンスター速度倍率
        /// </summary>
        public float GetSpeedMultiplier()
        {
            return 1f + _speedMultiplierPerRound * (CurrentRound - 1);
        }

        /// <summary>
        /// 命中率を算出
        /// </summary>
        public float GetAccuracy()
        {
            if (TotalWafflesThrown == 0) return 0f;
            return (float)TotalHits / TotalWafflesThrown * 100f;
        }

        /// <summary>
        /// リセット（タイトルに戻る時）
        /// </summary>
        public void ResetRound()
        {
            CurrentRound = 1;
            TotalWafflesThrown = 0;
            TotalHits = 0;
        }

        // ─── PlayerPrefs ───

        private void SaveBestTime()
        {
            PlayerPrefs.SetFloat("WM_BestTime", BestTime);
            PlayerPrefs.SetString("WM_BestRank", BestRank);
            PlayerPrefs.Save();
        }

        private void LoadBestTime()
        {
            if (PlayerPrefs.HasKey("WM_BestTime"))
            {
                BestTime = PlayerPrefs.GetFloat("WM_BestTime");
                BestRank = PlayerPrefs.GetString("WM_BestRank", "");
            }
        }
    }
}
