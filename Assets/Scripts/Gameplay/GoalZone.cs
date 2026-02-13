using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ゴール地点 — プレイヤー到達でGameClear
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class GoalZone : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private ParticleSystem _goalEffect;
        [SerializeField] private Light _goalLight;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseMin = 1f;
        [SerializeField] private float _pulseMax = 3f;

        private void Start()
        {
            // コライダーをトリガーに
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            // ゴールの光がパルスする
            if (_goalLight != null)
            {
                float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                _goalLight.intensity = Mathf.Lerp(_pulseMin, _pulseMax, t);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                Debug.Log("[GoalZone] ゴール到達！");

                if (_goalEffect != null)
                {
                    _goalEffect.Play();
                }

                GameManager.Instance?.ReachGoal();
            }
        }
    }
}
