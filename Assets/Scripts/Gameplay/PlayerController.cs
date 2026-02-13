using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// プレイヤー移動 + ワッフル投擲コントローラー
    /// PC: WASD移動、マウス視点操作、左クリック投擲
    /// モバイル: ジョイスティック移動、ボタン投擲
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _jumpForce = 8f;

        [Header("Mouse Look")]
        [SerializeField] private float _mouseSensitivity = 2f;
        private float _rotationX = 0f;

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
        private bool _useKeyboardInput = true;

        public bool IsMoving { get; private set; }

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

            // PCではマウスカーソルをロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // GameManagerがなくてもプレイ可能にする（デバッグ用）
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            HandleMouseLook();
            HandleMovement();
            HandleThrowCooldown();
            HandleKeyboardThrow();

            // ESCでカーソル解除
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // クリックでカーソル再ロック
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void HandleMouseLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

            // 水平回転（プレイヤー自体を回転）
            transform.Rotate(Vector3.up * mouseX);

            // 垂直回転（カメラのみ）
            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -60f, 60f);

            if (_cameraTransform != null)
            {
                _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            // キーボード入力
            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S

            // ジョイスティック入力（モバイル）がある場合はそちらを優先
            if (_joystick != null && _joystick.Magnitude > 0.1f)
            {
                Vector2 joyInput = _joystick.Direction;
                h = joyInput.x;
                v = joyInput.y;
            }

            Vector3 moveDir = Vector3.zero;

            if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
            {
                // プレイヤーローカル座標で移動
                moveDir = (transform.forward * v + transform.right * h).normalized;
                _controller.Move(moveDir * _moveSpeed * Time.deltaTime);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }

            // ジャンプ
            if (_controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                _velocity.y = _jumpForce;
            }

            // 重力
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleKeyboardThrow()
        {
            // 左クリックでワッフル投擲
            if (Input.GetMouseButtonDown(0))
            {
                ThrowWaffle();
            }
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
        /// ワッフル投擲（UIボタンまたはキーボードから呼び出し）
        /// </summary>
        public void ThrowWaffle()
        {
            if (!_canThrow) return;

            // GameManagerが無い場合はスキップ
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing) return;
                if (!GameManager.Instance.UseWaffle()) return;
            }

            // クールダウン開始
            _canThrow = false;
            _throwTimer = _throwCooldown;

            // ワッフルプールがない場合はエフェクトだけ
            if (_wafflePool == null)
            {
                Debug.Log("[Player] ワッフル投擲！（プールなし - エフェクトのみ）");
                return;
            }

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

        private void OnDestroy()
        {
            // カーソルを元に戻す
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
