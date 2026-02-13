using UnityEngine;
using UnityEngine.Events;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ゲーム全体の制御 — シングルトン
    /// 状態遷移、タイマー、勝敗判定を管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private float _timeLimit = 90f;
        [SerializeField] private int _initialWaffleCount = 10;

        [Header("Events")]
        public UnityEvent<GameState> OnStateChanged;
        public UnityEvent<float> OnTimeUpdated;
        public UnityEvent<int> OnWaffleCountChanged;

        public GameState CurrentState { get; private set; } = GameState.Title;
        public float RemainingTime { get; private set; }
        public int WaffleCount { get; private set; }
        public float TimeLimit => _timeLimit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            WaffleCount = _initialWaffleCount;
            RemainingTime = _timeLimit;
            ChangeState(GameState.Ready);
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            RemainingTime -= Time.deltaTime;
            OnTimeUpdated?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                ChangeState(GameState.GameOver);
            }
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Ready:
                    StartCoroutine(ReadyCountdown());
                    break;
                case GameState.Playing:
                    break;
                case GameState.GameOver:
                    OnGameOver();
                    break;
                case GameState.GameClear:
                    OnGameClear();
                    break;
            }
        }

        private System.Collections.IEnumerator ReadyCountdown()
        {
            // 3-2-1 カウントダウン
            yield return new WaitForSeconds(0.5f);
            ChangeState(GameState.Playing);
        }

        public void StartGame()
        {
            RemainingTime = _timeLimit;
            WaffleCount = _initialWaffleCount;
            OnWaffleCountChanged?.Invoke(WaffleCount);
            ChangeState(GameState.Ready);
        }

        /// <summary>
        /// ワッフルを消費。足りなければfalse
        /// </summary>
        public bool UseWaffle()
        {
            if (WaffleCount <= 0) return false;
            WaffleCount--;
            OnWaffleCountChanged?.Invoke(WaffleCount);
            return true;
        }

        /// <summary>
        /// マップ上のワッフル拾得
        /// </summary>
        public void AddWaffle(int count = 1)
        {
            WaffleCount += count;
            OnWaffleCountChanged?.Invoke(WaffleCount);
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SePickup);
        }

        /// <summary>
        /// ゴール到達
        /// </summary>
        public void ReachGoal()
        {
            if (CurrentState != GameState.Playing) return;
            ChangeState(GameState.GameClear);
        }

        /// <summary>
        /// モンスターに捕まった
        /// </summary>
        public void PlayerCaught()
        {
            if (CurrentState != GameState.Playing) return;
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeCaught);
            ChangeState(GameState.GameOver);
        }

        private void OnGameOver()
        {
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲームオーバー");
        }

        private void OnGameClear()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeGoal);
            Debug.Log("[GameManager] ゲームクリア！");
        }
    }
}
