using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// モンスターAI基底クラス
    /// ステートマシン: Patrol → Detect → Chase → Eat → Patrol
    /// NavMeshAgent使用
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterBase : MonoBehaviour
    {
        public enum MonsterAIState
        {
            Patrol,     // パトロール中
            Detect,     // プレイヤー発見
            Chase,      // 追跡中
            Eat,        // ワッフル食べ中（無力化）
            Return      // パトロール復帰中
        }

        [Header("AI Settings")]
        [SerializeField] protected float _detectRange = 12f;
        [SerializeField] protected float _chaseSpeed = 4f;
        [SerializeField] protected float _patrolSpeed = 2f;
        [SerializeField] protected float _eatDuration = 4f;
        [SerializeField] protected float _catchDistance = 1.5f;
        [SerializeField] protected float _losePlayerDistance = 18f;

        [Header("Patrol")]
        [SerializeField] protected Transform[] _patrolPoints;
        [SerializeField] protected float _patrolWaitTime = 1.5f;

        [Header("Visual")]
        [SerializeField] protected Renderer _bodyRenderer;
        [SerializeField] protected Color _normalColor = Color.white;
        [SerializeField] protected Color _chaseColor = Color.red;
        [SerializeField] protected Color _eatColor = Color.yellow;
        [SerializeField] protected ParticleSystem _eatParticle;

        protected NavMeshAgent _agent;
        protected Transform _playerTransform;
        protected MonsterVisualAnimator _visualAnimator;
        protected MonsterAIState _currentState = MonsterAIState.Patrol;
        protected int _currentPatrolIndex = 0;
        protected bool _isWaiting = false;

        public MonsterAIState CurrentState => _currentState;

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _visualAnimator = GetComponent<MonsterVisualAnimator>();
            if (_visualAnimator == null)
                _visualAnimator = gameObject.AddComponent<MonsterVisualAnimator>();
        }

        protected virtual void Start()
        {
            // プレイヤーを探す
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            _agent.speed = _patrolSpeed;
            SetState(MonsterAIState.Patrol);
        }

        protected virtual void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                _agent.isStopped = true;
                return;
            }

            _agent.isStopped = false;

            switch (_currentState)
            {
                case MonsterAIState.Patrol:
                    UpdatePatrol();
                    break;
                case MonsterAIState.Detect:
                    UpdateDetect();
                    break;
                case MonsterAIState.Chase:
                    UpdateChase();
                    break;
                case MonsterAIState.Eat:
                    // コルーチンが管理
                    break;
                case MonsterAIState.Return:
                    UpdateReturn();
                    break;
            }
        }

        #region State Updates

        protected virtual void UpdatePatrol()
        {
            // プレイヤー検知チェック
            if (CanDetectPlayer())
            {
                SetState(MonsterAIState.Detect);
                return;
            }

            // パトロールポイントへ移動
            if (_patrolPoints == null || _patrolPoints.Length == 0) return;
            if (_isWaiting) return;

            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                StartCoroutine(PatrolWait());
            }
        }

        protected virtual void UpdateDetect()
        {
            // 発見演出後に追跡へ
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeMonsterDetect);
            _visualAnimator?.ShowAlert(1f);
            SetState(MonsterAIState.Chase);
        }

        protected virtual void UpdateChase()
        {
            if (_playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            // プレイヤーに接触 → ゲームオーバー
            if (distance < _catchDistance)
            {
                GameManager.Instance?.PlayerCaught();
                _agent.isStopped = true;
                return;
            }

            // プレイヤーを見失った → パトロール復帰
            if (distance > _losePlayerDistance)
            {
                SetState(MonsterAIState.Return);
                return;
            }

            // 追跡
            _agent.SetDestination(_playerTransform.position);
        }

        protected virtual void UpdateReturn()
        {
            // 最寄りのパトロールポイントへ復帰
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                SetState(MonsterAIState.Patrol);
            }

            // 復帰中にプレイヤーを再検知
            if (CanDetectPlayer())
            {
                SetState(MonsterAIState.Chase);
            }
        }

        #endregion

        #region State Transitions

        protected void SetState(MonsterAIState newState)
        {
            _currentState = newState;

            switch (newState)
            {
                case MonsterAIState.Patrol:
                    _agent.speed = _patrolSpeed;
                    SetColor(_normalColor);
                    MoveToNextPatrolPoint();
                    break;

                case MonsterAIState.Detect:
                    _agent.isStopped = true;
                    break;

                case MonsterAIState.Chase:
                    _agent.speed = _chaseSpeed;
                    _agent.isStopped = false;
                    SetColor(_chaseColor);
                    break;

                case MonsterAIState.Eat:
                    _agent.isStopped = true;
                    SetColor(_eatColor);
                    break;

                case MonsterAIState.Return:
                    _agent.speed = _patrolSpeed;
                    _agent.isStopped = false;
                    SetColor(_normalColor);
                    MoveToNearestPatrolPoint();
                    break;
            }
        }

        #endregion

        #region Detection

        protected virtual bool CanDetectPlayer()
        {
            if (_playerTransform == null) return false;
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            if (distance > _detectRange) return false;

            // 視線チェック（レイキャスト）
            Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out RaycastHit hit, _detectRange))
            {
                if (hit.transform == _playerTransform)
                    return true;
            }

            return false;
        }

        #endregion

        #region Waffle Hit

        /// <summary>
        /// ワッフル球が命中した時の処理
        /// </summary>
        public virtual void OnHitByWaffle(Vector3 hitPosition)
        {
            if (_currentState == MonsterAIState.Eat) return; // 既に食事中

            StopAllCoroutines();
            StartCoroutine(EatCoroutine(hitPosition));
        }

        protected virtual IEnumerator EatCoroutine(Vector3 wafflePosition)
        {
            SetState(MonsterAIState.Eat);
            _visualAnimator?.ShowPaku(1.5f);

            // ワッフルの位置を向く
            Vector3 lookDir = (wafflePosition - transform.position);
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // 食べエフェクト再生
            if (_eatParticle != null)
            {
                _eatParticle.Play();
            }
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeMonsterEat);

            // 食事モーション（スケールパルス）
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            while (elapsed < _eatDuration)
            {
                float pulse = 1f + 0.1f * Mathf.Sin(elapsed * 8f);
                transform.localScale = originalScale * pulse;
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale;

            if (_eatParticle != null)
            {
                _eatParticle.Stop();
            }

            // パトロールに復帰
            SetState(MonsterAIState.Return);
        }

        #endregion

        #region Patrol Helpers

        private void MoveToNextPatrolPoint()
        {
            if (_patrolPoints == null || _patrolPoints.Length == 0) return;
            _agent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }

        private void MoveToNearestPatrolPoint()
        {
            if (_patrolPoints == null || _patrolPoints.Length == 0) return;

            float minDist = float.MaxValue;
            int nearest = 0;
            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                float dist = Vector3.Distance(transform.position, _patrolPoints[i].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = i;
                }
            }
            _currentPatrolIndex = nearest;
            _agent.SetDestination(_patrolPoints[nearest].position);
        }

        private IEnumerator PatrolWait()
        {
            _isWaiting = true;
            yield return new WaitForSeconds(_patrolWaitTime);
            _isWaiting = false;
            MoveToNextPatrolPoint();
        }

        #endregion

        /// <summary>
        /// 外部から強制追跡を指示（FuzzyMonsterの仲間呼び用）
        /// </summary>
        public void ForceChase(Transform target)
        {
            if (_currentState == MonsterAIState.Eat) return;
            _playerTransform = target;
            SetState(MonsterAIState.Chase);
        }

        protected void SetColor(Color color)
        {
            if (_bodyRenderer != null)
            {
                _bodyRenderer.material.color = color;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 検知範囲（シーンビュー用）
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _catchDistance);
        }
    }
}
