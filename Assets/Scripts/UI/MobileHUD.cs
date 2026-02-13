using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// モバイルHUD — タイマー、ワッフル残数表示
    /// </summary>
    public class MobileHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _waffleCountText;
        [SerializeField] private Image _timerFill;
        [SerializeField] private Image _dangerOverlay;

        [Header("Settings")]
        [SerializeField] private float _dangerThreshold = 15f;
        [SerializeField] private Color _normalTimeColor = Color.white;
        [SerializeField] private Color _dangerTimeColor = Color.red;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimeUpdated.AddListener(UpdateTimer);
                GameManager.Instance.OnWaffleCountChanged.AddListener(UpdateWaffleCount);
                GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);

                UpdateWaffleCount(GameManager.Instance.WaffleCount);
            }

            if (_dangerOverlay != null)
            {
                _dangerOverlay.gameObject.SetActive(false);
            }
        }

        private void UpdateTimer(float time)
        {
            if (_timerText != null)
            {
                _timerText.text = Mathf.CeilToInt(time).ToString();

                // 残り時間少ない時は赤く
                _timerText.color = time <= _dangerThreshold ? _dangerTimeColor : _normalTimeColor;
            }

            if (_timerFill != null)
            {
                _timerFill.fillAmount = time / GameManager.Instance.TimeLimit;
            }

            // 残り少ない時のダンジャー演出
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

        private void OnGameStateChanged(GameState state)
        {
            // Playing時のみ表示
            gameObject.SetActive(state == GameState.Playing || state == GameState.Ready);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimeUpdated.RemoveListener(UpdateTimer);
                GameManager.Instance.OnWaffleCountChanged.RemoveListener(UpdateWaffleCount);
                GameManager.Instance.OnStateChanged.RemoveListener(OnGameStateChanged);
            }
        }
    }
}
