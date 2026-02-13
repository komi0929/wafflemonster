using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// タイトル画面
    /// ゲームロゴ、スタートボタン、操作説明、ベストタイム
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bestTimeText;
        [SerializeField] private TextMeshProUGUI _controlsText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private float _pulseTimer;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
            }

            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            UpdateBestTime();
            ShowTitle();
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            // タイトルテキストパルス
            if (_titleText != null)
            {
                _pulseTimer += Time.unscaledDeltaTime;
                float scale = 1f + Mathf.Sin(_pulseTimer * 1.5f) * 0.03f;
                _titleText.transform.localScale = Vector3.one * scale;
            }
        }

        private void ShowTitle()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;
        }

        private void OnStartClicked()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f;
            GameManager.Instance?.StartGame();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Title)
            {
                UpdateBestTime();
                ShowTitle();
            }
        }

        private void UpdateBestTime()
        {
            if (_bestTimeText == null) return;

            if (DifficultyManager.Instance != null && DifficultyManager.Instance.BestTime > 0f)
            {
                _bestTimeText.text = $"BEST: {DifficultyManager.Instance.BestTime:F1}s " +
                                     $"[{DifficultyManager.Instance.BestRank}]";
                _bestTimeText.gameObject.SetActive(true);
            }
            else
            {
                _bestTimeText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
    }
}
