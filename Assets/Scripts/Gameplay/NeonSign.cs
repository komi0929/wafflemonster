using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ネオンサイン — Emissionパルスアニメーション
    /// 建物の壁に配置して裏路地の雰囲気を演出
    /// </summary>
    public class NeonSign : MonoBehaviour
    {
        [Header("Neon Colors")]
        [SerializeField] private Color _neonColor = new Color(1f, 0.2f, 0.8f); // ピンク
        [SerializeField] private float _emissionIntensity = 3f;

        [Header("Animation")]
        [SerializeField] private float _pulseSpeed = 1.5f;
        [SerializeField] private float _pulseMin = 0.5f;
        [SerializeField] private float _flickerChance = 0.02f;
        [SerializeField] private float _flickerDuration = 0.1f;

        [Header("Light")]
        [SerializeField] private Light _neonLight;

        private Renderer _renderer;
        private Material _material;
        private float _flickerTimer = 0f;
        private bool _isFlickering = false;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _material = _renderer.material;
                _material.EnableKeyword("_EMISSION");
            }
        }

        private void Update()
        {
            float intensity;

            // フリッカー（ちらつき）判定
            if (!_isFlickering && Random.value < _flickerChance)
            {
                _isFlickering = true;
                _flickerTimer = _flickerDuration;
            }

            if (_isFlickering)
            {
                _flickerTimer -= Time.deltaTime;
                intensity = Random.value > 0.5f ? _emissionIntensity : 0f;
                if (_flickerTimer <= 0f)
                {
                    _isFlickering = false;
                }
            }
            else
            {
                // 通常パルス
                float pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                intensity = Mathf.Lerp(_pulseMin, 1f, pulse) * _emissionIntensity;
            }

            // Emission更新
            if (_material != null)
            {
                Color emColor = _neonColor * intensity;
                _material.SetColor(EmissionColor, emColor);
            }

            // 付随するライトも同期
            if (_neonLight != null)
            {
                _neonLight.intensity = intensity * 0.5f;
                _neonLight.color = _neonColor;
            }
        }
    }
}
