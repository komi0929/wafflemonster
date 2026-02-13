using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ロボットモンスター「Mecha-paku」
    /// 特徴: 高速直線追跡、狭い検知範囲、短い食事時間
    /// 機械的な動き（カクカク回転）
    /// </summary>
    public class MechaMonster : MonsterBase
    {
        [Header("Mecha Unique")]
        [SerializeField] private float _chargeSpeed = 7f;
        [SerializeField] private float _rotationStepAngle = 45f;
        [SerializeField] private Light _eyeLight;

        private bool _isCharging = false;

        protected override void Awake()
        {
            base.Awake();

            // ロボット固有パラメータ
            _patrolSpeed = 2.5f;
            _chaseSpeed = 6f;
            _detectRange = 10f;
            _eatDuration = 3f;
            _catchDistance = 1.8f;
        }

        protected override void Start()
        {
            base.Start();
            SetEyeColor(Color.green);
        }

        protected override void UpdateChase()
        {
            base.UpdateChase();

            // 追跡時は目を赤く
            SetEyeColor(Color.red);

            // 近距離で突進加速
            if (_playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist < _detectRange * 0.5f && !_isCharging)
                {
                    _isCharging = true;
                    _agent.speed = _chargeSpeed;
                }
            }
        }

        protected override void UpdatePatrol()
        {
            base.UpdatePatrol();
            _isCharging = false;
            _agent.speed = _patrolSpeed;
            SetEyeColor(Color.green);
        }

        public override void OnHitByWaffle(Vector3 hitPosition)
        {
            _isCharging = false;
            SetEyeColor(Color.yellow);
            base.OnHitByWaffle(hitPosition);
        }

        private void SetEyeColor(Color color)
        {
            if (_eyeLight != null)
            {
                _eyeLight.color = color;
            }
        }
    }
}
