using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// バブルワッフル球 — モンスターに命中でEatステート誘発
    /// ObjectPool対応: 一定時間後に自動返却
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class WaffleBall : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 5f;
        [SerializeField] private ParticleSystem _hitEffect;

        private ObjectPool _pool;
        private float _timer;
        private Rigidbody _rb;
        private TrailRenderer _trail;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _trail = GetComponent<TrailRenderer>();
        }

        private void OnEnable()
        {
            _timer = _lifeTime;
            if (_trail != null) _trail.Clear();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// ObjectPoolへの参照を設定（生成時に呼び出し）
        /// </summary>
        public void SetPool(ObjectPool pool)
        {
            _pool = pool;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // モンスターと衝突
            MonsterBase monster = collision.gameObject.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.OnHitByWaffle(transform.position);
                AudioManager.Instance?.PlaySE(AudioManager.Instance.SeHit);

                // ヒットエフェクト
                if (_hitEffect != null)
                {
                    var effect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
                    Destroy(effect.gameObject, 2f);
                }

                ReturnToPool();
                return;
            }

            // 地面や壁に当たった場合 — 2秒後に消える
            _timer = Mathf.Min(_timer, 2f);
        }

        private void ReturnToPool()
        {
            if (_pool != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _pool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
