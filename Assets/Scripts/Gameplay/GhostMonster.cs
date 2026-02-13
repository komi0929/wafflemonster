using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ゴーストモンスター「Whi-chan」
    /// 特徴: ゆっくり移動、広い検知範囲、長い食事時間
    /// ふわふわ浮遊アニメーション付き
    /// </summary>
    public class GhostMonster : MonsterBase
    {
        [Header("Ghost Unique")]
        [SerializeField] private float _floatAmplitude = 0.3f;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _transparentAlpha = 0.6f;

        private Vector3 _basePosition;
        private Material _material;

        protected override void Awake()
        {
            base.Awake();

            // ゴースト固有パラメータ
            _patrolSpeed = 1.5f;
            _chaseSpeed = 3f;
            _detectRange = 15f;
            _eatDuration = 5f;
        }

        protected override void Start()
        {
            base.Start();
            _basePosition = transform.position;

            // 半透明マテリアル
            if (_bodyRenderer != null)
            {
                _material = _bodyRenderer.material;
                SetAlpha(_transparentAlpha);
            }
        }

        protected override void Update()
        {
            base.Update();

            // ふわふわ浮遊
            if (_currentState != MonsterAIState.Eat)
            {
                float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
                Vector3 pos = transform.position;
                pos.y = _agent.nextPosition.y + yOffset + 0.5f;
                transform.position = pos;
            }
        }

        /// <summary>
        /// ゴーストは壁を透過して検知可能（レイキャスト無視）
        /// </summary>
        protected override bool CanDetectPlayer()
        {
            if (_playerTransform == null) return false;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            return distance <= _detectRange;
        }

        private void SetAlpha(float alpha)
        {
            if (_material == null) return;
            Color c = _material.color;
            c.a = alpha;
            _material.color = c;
        }
    }
}
