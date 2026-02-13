using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ファーリーモンスター「Fuzz-nom」
    /// 特徴: 広い範囲索敵（360度）、中速、中食事時間
    /// 仲間を呼ぶ能力：近くのモンスターにもプレイヤー位置を通知
    /// </summary>
    public class FuzzyMonster : MonsterBase
    {
        [Header("Fuzzy Unique")]
        [SerializeField] private float _alertRange = 20f;
        [SerializeField] private float _wobbleIntensity = 0.15f;
        [SerializeField] private float _wobbleSpeed = 5f;

        private bool _hasAlerted = false;

        protected override void Awake()
        {
            base.Awake();

            // ファーリー固有パラメータ
            _patrolSpeed = 2f;
            _chaseSpeed = 4.5f;
            _detectRange = 14f;
            _eatDuration = 4f;
        }

        protected override void Update()
        {
            base.Update();

            // もふもふ揺れアニメーション
            if (_currentState != MonsterAIState.Eat)
            {
                float wobbleX = Mathf.Sin(Time.time * _wobbleSpeed) * _wobbleIntensity;
                float wobbleZ = Mathf.Cos(Time.time * _wobbleSpeed * 0.7f) * _wobbleIntensity;
                transform.localRotation *= Quaternion.Euler(wobbleX, 0, wobbleZ);
            }
        }

        /// <summary>
        /// ファーリーは360度の全方位検知（視線チェックなし）
        /// ただし壁越しは不可
        /// </summary>
        protected override bool CanDetectPlayer()
        {
            if (_playerTransform == null) return false;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            if (distance > _detectRange) return false;

            // 壁チェックのみ（角度制限なし）
            Vector3 dir = (_playerTransform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, _detectRange))
            {
                return hit.transform == _playerTransform;
            }
            return false;
        }

        protected override void UpdateDetect()
        {
            // 仲間に通知
            if (!_hasAlerted)
            {
                AlertNearbyMonsters();
                _hasAlerted = true;
            }

            base.UpdateDetect();
        }

        protected override void UpdatePatrol()
        {
            base.UpdatePatrol();
            _hasAlerted = false;
        }

        /// <summary>
        /// 近くの他モンスターにプレイヤー位置を通知
        /// </summary>
        private void AlertNearbyMonsters()
        {
            MonsterBase[] allMonsters = FindObjectsByType<MonsterBase>(FindObjectsSortMode.None);
            foreach (var monster in allMonsters)
            {
                if (monster == this) continue;
                if (monster.CurrentState == MonsterBase.MonsterAIState.Eat) continue;

                float dist = Vector3.Distance(transform.position, monster.transform.position);
                if (dist < _alertRange)
                {
                    // MonsterBaseのForceChaseを呼び出し
                    monster.ForceChase(_playerTransform);
                }
            }
        }
    }
}
