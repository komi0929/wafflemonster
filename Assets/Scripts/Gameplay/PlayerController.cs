using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// プレイヤー移動 + ワッフル投擲コントローラー
    /// タッチ操作: 左ジョイスティック移動、右タップ投擲
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _gravity = -20f;

        [Header("Throw")]
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _throwUpAngle = 25f;
        [SerializeField] private Transform _throwPoint;
        [SerializeField] private ObjectPool _wafflePool;

        [Header("References")]
        [SerializeField] private VirtualJoystick _joystick;
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _canThrow = true;
        private float _throwCooldown = 0.3f;
        private float _throwTimer;

        public bool IsMoving => _joystick != null && _joystick.Magnitude > 0.1f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            HandleMovement();
            HandleThrowCooldown();
        }

        private void HandleMovement()
        {
            if (_joystick == null) return;

            Vector2 input = _joystick.Direction;

            if (input.magnitude > 0.1f)
            {
                // カメラ基準の移動方向を計算
                Vector3 camForward = _cameraTransform.forward;
                Vector3 camRight = _cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;
                _controller.Move(moveDir * _moveSpeed * Time.deltaTime);

                // 移動方向を向く
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // 重力
            if (_controller.isGrounded)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleThrowCooldown()
        {
            if (!_canThrow)
            {
                _throwTimer -= Time.deltaTime;
                if (_throwTimer <= 0f)
                {
                    _canThrow = true;
                }
            }
        }

        /// <summary>
        /// ワッフル投擲（UIボタンから呼び出し）
        /// </summary>
        public void ThrowWaffle()
        {
            if (!_canThrow) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            if (!GameManager.Instance.UseWaffle()) return;

            // クールダウン開始
            _canThrow = false;
            _throwTimer = _throwCooldown;

            // ワッフル球生成
            Vector3 spawnPos = _throwPoint != null ? _throwPoint.position : transform.position + Vector3.up * 1.2f;
            GameObject waffle = _wafflePool.Get(spawnPos, Quaternion.identity);

            // 投擲方向: プレイヤーの前方 + 上向きの角度
            Vector3 throwDir = transform.forward;
            throwDir = Quaternion.AngleAxis(-_throwUpAngle, transform.right) * throwDir;

            Rigidbody rb = waffle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(throwDir * _throwForce, ForceMode.VelocityChange);
            }

            // SE再生
            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeThrow);
        }
    }
}
