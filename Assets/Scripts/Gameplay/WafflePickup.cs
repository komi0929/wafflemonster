using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// マップ上のワッフル補充アイテム
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class WafflePickup : MonoBehaviour
    {
        [SerializeField] private int _waffleAmount = 3;
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private float _bobAmplitude = 0.2f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private ParticleSystem _pickupEffect;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void Update()
        {
            // 回転 + 上下浮遊
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
            float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobAmplitude;
            transform.position = _startPos + Vector3.up * yOffset;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                GameManager.Instance?.AddWaffle(_waffleAmount);

                if (_pickupEffect != null)
                {
                    var effect = Instantiate(_pickupEffect, transform.position, Quaternion.identity);
                    Destroy(effect.gameObject, 2f);
                }

                Destroy(gameObject);
            }
        }
    }
}
