using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// リザルト画面（GameClear / GameOver）
    /// ランク表示、ベストタイム更新、リトライ/タイトル遷移
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _gameClearPanel;
        [SerializeField] private TextMeshProUGUI _clearTimeText;
        [SerializeField] private TextMeshProUGUI _clearWaffleText;
        [SerializeField] private TextMeshProUGUI _clearRankText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _titleButton;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 2f;

        private bool _isShowing = false;
        private float _targetAlpha = 0f;

        private void Start()
        {
            gameObject.SetActive(false);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(OnRetry);
            }
            if (_titleButton != null)
            {
                _titleButton.onClick.AddListener(OnTitle);
            }
        }

        private void Update()
        {
            if (!_isShowing) return;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(
                    _canvasGroup.alpha,
                    _targetAlpha,
                    _fadeSpeed * Time.unscaledDeltaTime
                );
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                Show(false);
            }
            else if (state == GameState.GameClear)
            {
                Show(true);
            }
        }

        private void Show(bool isClear)
        {
            gameObject.SetActive(true);
            _isShowing = true;
            _targetAlpha = 1f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_gameOverPanel != null) _gameOverPanel.SetActive(!isClear);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(isClear);

            if (isClear && GameManager.Instance != null)
            {
                float elapsedTime = GameManager.Instance.TimeLimit - GameManager.Instance.RemainingTime;

                string rank = "C";
                if (DifficultyManager.Instance != null)
                {
                    rank = DifficultyManager.Instance.CalculateRank(elapsedTime);
                    DifficultyManager.Instance.OnGameClear(elapsedTime);
                }

                if (_clearTimeText != null)
                {
                    _clearTimeText.text = $"クリアタイム: {elapsedTime:F1}秒";
                }
                if (_clearRankText != null)
                {
                    _clearRankText.text = $"RANK {rank}";
                    _clearRankText.color = rank switch
                    {
                        "S" => new Color(1f, 0.85f, 0f),
                        "A" => new Color(0f, 1f, 0.5f),
                        "B" => new Color(0f, 0.7f, 1f),
                        _ => new Color(0.6f, 0.6f, 0.6f)
                    };
                }
                if (_clearWaffleText != null)
                {
                    _clearWaffleText.text = $"残りワッフル: ×{GameManager.Instance.WaffleCount}";
                }
            }
        }

        private void OnRetry()
        {
            gameObject.SetActive(false);
            _isShowing = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            GameManager.Instance?.StartGame();
        }

        private void OnTitle()
        {
            gameObject.SetActive(false);
            _isShowing = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Title);
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
