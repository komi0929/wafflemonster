using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// 提灯ライト — 揺れるポイントライト
    /// 香港/台湾の裏路地雰囲気
    /// </summary>
    public class Lantern : MonoBehaviour
    {
        [Header("Light")]
        [SerializeField] private Light _light;
        [SerializeField] private float _baseIntensity = 1.5f;
        [SerializeField] private float _intensityVariation = 0.3f;

        [Header("Swing")]
        [SerializeField] private float _swingAngle = 5f;
        [SerializeField] private float _swingSpeed = 1.5f;

        [Header("Color")]
        [SerializeField] private Color _warmColor = new Color(1f, 0.7f, 0.3f);

        private Vector3 _originalRotation;

        private void Start()
        {
            _originalRotation = transform.localEulerAngles;

            if (_light != null)
            {
                _light.color = _warmColor;
                _light.intensity = _baseIntensity;
            }
        }

        private void Update()
        {
            // 揺れ
            float swing = Mathf.Sin(Time.time * _swingSpeed) * _swingAngle;
            transform.localEulerAngles = _originalRotation + new Vector3(0, 0, swing);

            // 明るさ揺らぎ
            if (_light != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * 3f, 0f);
                _light.intensity = _baseIntensity + (noise - 0.5f) * _intensityVariation * 2f;
            }
        }
    }
}
