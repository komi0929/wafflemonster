using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// HUD — タイマー、ワッフル残数、コンボ、カウントダウン表示
    /// </summary>
    public class MobileHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _waffleCountText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private Image _timerFill;
        [SerializeField] private Image _dangerOverlay;

        [Header("Settings")]
        [SerializeField] private float _dangerThreshold = 15f;
        [SerializeField] private Color _normalTimeColor = Color.white;
        [SerializeField] private Color _dangerTimeColor = Color.red;

        private float _comboDisplayTimer;
        private float _countdownDisplayTimer;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimeUpdated.AddListener(UpdateTimer);
                GameManager.Instance.OnWaffleCountChanged.AddListener(UpdateWaffleCount);
                GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
                GameManager.Instance.OnComboChanged.AddListener(OnComboChanged);
                GameManager.Instance.OnCountdown.AddListener(OnCountdown);

                UpdateWaffleCount(GameManager.Instance.WaffleCount);
            }

            if (_dangerOverlay != null) _dangerOverlay.gameObject.SetActive(false);
            if (_comboText != null) _comboText.gameObject.SetActive(false);
            if (_countdownText != null) _countdownText.gameObject.SetActive(false);
        }

        private void Update()
        {
            // コンボ表示タイマー
            if (_comboText != null && _comboText.gameObject.activeSelf)
            {
                _comboDisplayTimer -= Time.deltaTime;
                if (_comboDisplayTimer <= 0f)
                {
                    _comboText.gameObject.SetActive(false);
                    GameManager.Instance?.ResetCombo();
                }
                else
                {
                    // パルスアニメーション
                    float scale = 1f + Mathf.Sin(Time.time * 8f) * 0.1f;
                    _comboText.transform.localScale = Vector3.one * scale;
                }
            }

            // カウントダウン表示タイマー
            if (_countdownText != null && _countdownText.gameObject.activeSelf)
            {
                _countdownDisplayTimer -= Time.deltaTime;
                if (_countdownDisplayTimer <= 0f)
                {
                    _countdownText.gameObject.SetActive(false);
                }
                else
                {
                    float scale = 1f + (1f - _countdownDisplayTimer) * 0.3f;
                    _countdownText.transform.localScale = Vector3.one * scale;
                    Color c = _countdownText.color;
                    c.a = _countdownDisplayTimer;
                    _countdownText.color = c;
                }
            }
        }

        private void UpdateTimer(float time)
        {
            if (_timerText != null)
            {
                int t = Mathf.CeilToInt(time);
                _timerText.text = t.ToString();
                _timerText.color = time <= _dangerThreshold ? _dangerTimeColor : _normalTimeColor;
            }

            if (_timerFill != null)
            {
                _timerFill.fillAmount = time / GameManager.Instance.TimeLimit;
            }

            if (_dangerOverlay != null)
            {
                bool isDanger = time <= _dangerThreshold;
                _dangerOverlay.gameObject.SetActive(isDanger);
                if (isDanger)
                {
                    float alpha = Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.15f;
                    Color c = _dangerOverlay.color;
                    c.a = alpha;
                    _dangerOverlay.color = c;
                }
            }
        }

        private void UpdateWaffleCount(int count)
        {
            if (_waffleCountText != null)
            {
                _waffleCountText.text = $"×{count}";
            }
        }

        private void OnComboChanged(int combo)
        {
            if (_comboText == null) return;

            if (combo >= 2)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"{combo} COMBO!";
                _comboDisplayTimer = 3f; // 3秒後にリセット
                _comboText.transform.localScale = Vector3.one * 1.5f;

                // コンボに応じた色
                _comboText.color = combo switch
                {
                    >= 5 => new Color(1f, 0.3f, 0.8f),
                    >= 3 => new Color(1f, 0.85f, 0f),
                    _ => new Color(0f, 0.9f, 1f)
                };
            }
        }

        private void OnCountdown(int count)
        {
            if (_countdownText == null) return;

            if (count > 0)
            {
                _countdownText.gameObject.SetActive(true);
                _countdownText.text = count.ToString();
                _countdownText.color = new Color(1f, 1f, 1f, 1f);
                _countdownDisplayTimer = 0.9f;
                _countdownText.transform.localScale = Vector3.one * 2f;
            }
            else
            {
                _countdownText.gameObject.SetActive(true);
                _countdownText.text = "GO!";
                _countdownText.color = new Color(0f, 1f, 0.5f, 1f);
                _countdownDisplayTimer = 0.8f;
                _countdownText.transform.localScale = Vector3.one * 2.5f;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Playing || state == GameState.Ready);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimeUpdated.RemoveListener(UpdateTimer);
                GameManager.Instance.OnWaffleCountChanged.RemoveListener(UpdateWaffleCount);
                GameManager.Instance.OnStateChanged.RemoveListener(OnGameStateChanged);
                GameManager.Instance.OnComboChanged.RemoveListener(OnComboChanged);
                GameManager.Instance.OnCountdown.RemoveListener(OnCountdown);
            }
        }
    }
}
