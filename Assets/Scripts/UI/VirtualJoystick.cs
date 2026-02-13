using UnityEngine;
using UnityEngine.EventSystems;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// モバイル用バーチャルジョイスティック
    /// Canvas上に配置し、タッチ入力で方向を取得
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float _handleRange = 50f;
        [SerializeField] private float _deadZone = 0.1f;

        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        private Vector2 _input = Vector2.zero;
        private Canvas _canvas;
        private Camera _uiCamera;

        /// <summary>
        /// 正規化された入力方向 (-1 to 1)
        /// </summary>
        public Vector2 Direction => _input;

        /// <summary>
        /// 入力の大きさ (0 to 1)
        /// </summary>
        public float Magnitude => _input.magnitude;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _uiCamera = _canvas.worldCamera;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                _uiCamera,
                out Vector2 localPoint
            );

            // -1 ~ 1 に正規化
            Vector2 normalized = localPoint / (_background.sizeDelta * 0.5f);
            _input = normalized.magnitude > 1f ? normalized.normalized : normalized;

            // デッドゾーン処理
            if (_input.magnitude < _deadZone)
            {
                _input = Vector2.zero;
            }

            // ハンドル位置更新
            _handle.anchoredPosition = _input * _handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }
    }
}
