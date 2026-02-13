using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// スマホ専用プレイヤーコントローラー
    /// 左ジョイスティック: 移動
    /// 右半面スワイプ: 視点操作
    /// 投擲ボタン: ワッフル発射
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _gravity = -20f;

        [Header("Touch Look")]
        [SerializeField] private float _touchSensitivity = 0.15f;
        [SerializeField] private float _maxPitchAngle = 60f;
        private float _rotationX = 0f;
        private int _lookFingerId = -1;
        private Vector2 _lastLookPos;

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

            // モバイル: カーソルロック不要
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // フレームレート安定化
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            HandleTouchLook();
            HandleMovement();
            HandleThrowCooldown();

            // PC入力フォールバック（エディタデバッグ用）
#if UNITY_EDITOR
            HandleEditorMouseLook();
            if (Input.GetMouseButtonDown(0)) ThrowWaffle();
#endif
        }

        // ─── タッチ視点操作 ───

        private void HandleTouchLook()
        {
            if (Input.touchCount == 0) return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // 画面右半分のみ視点操作（左半分はジョイスティック用）
                if (touch.position.x < Screen.width * 0.35f) continue;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (_lookFingerId == -1)
                        {
                            _lookFingerId = touch.fingerId;
                            _lastLookPos = touch.position;
                        }
                        break;

                    case TouchPhase.Moved:
                        if (touch.fingerId == _lookFingerId)
                        {
                            Vector2 delta = touch.position - _lastLookPos;
                            _lastLookPos = touch.position;

                            // 水平回転（プレイヤー自体）
                            transform.Rotate(Vector3.up * delta.x * _touchSensitivity);

                            // 垂直回転（カメラのみ）
                            _rotationX -= delta.y * _touchSensitivity;
                            _rotationX = Mathf.Clamp(_rotationX, -_maxPitchAngle, _maxPitchAngle);

                            if (_cameraTransform != null)
                            {
                                _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
                            }
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == _lookFingerId)
                        {
                            _lookFingerId = -1;
                        }
                        break;
                }
            }
        }

        // ─── 移動（ジョイスティック）───

        private void HandleMovement()
        {
            float h = 0f, v = 0f;

            // ジョイスティック入力
            if (_joystick != null && _joystick.Magnitude > 0.1f)
            {
                Vector2 joyInput = _joystick.Direction;
                h = joyInput.x;
                v = joyInput.y;
            }

#if UNITY_EDITOR
            // エディタ: キーボードフォールバック
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
            {
                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");
            }
#endif

            if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
            {
                Vector3 moveDir = (transform.forward * v + transform.right * h).normalized;
                _controller.Move(moveDir * _moveSpeed * Time.deltaTime);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
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

        private void HandleThrowCooldown()
        {
            if (!_canThrow)
            {
                _throwTimer -= Time.deltaTime;
                if (_throwTimer <= 0f) _canThrow = true;
            }
        }

        /// <summary>
        /// ワッフル投擲（UIボタンから呼び出し）
        /// </summary>
        public void ThrowWaffle()
        {
            if (!_canThrow) return;

            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing) return;
                if (!GameManager.Instance.UseWaffle()) return;
            }

            _canThrow = false;
            _throwTimer = _throwCooldown;

            if (_wafflePool == null)
            {
                Debug.Log("[Player] ワッフル投擲！（プールなし）");
                return;
            }

            Vector3 spawnPos = _throwPoint != null ? _throwPoint.position : transform.position + Vector3.up * 1.2f;
            GameObject waffle = _wafflePool.Get(spawnPos, Quaternion.identity);

            Vector3 throwDir = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            throwDir = Quaternion.AngleAxis(-_throwUpAngle, transform.right) * throwDir;

            Rigidbody rb = waffle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(throwDir * _throwForce, ForceMode.VelocityChange);
            }

            AudioManager.Instance?.PlaySE(AudioManager.Instance.SeThrow);
        }

#if UNITY_EDITOR
        private void HandleEditorMouseLook()
        {
            if (!Input.GetMouseButton(1)) return; // 右クリックドラッグで視点
            float mouseX = Input.GetAxis("Mouse X") * 2f;
            float mouseY = Input.GetAxis("Mouse Y") * 2f;
            transform.Rotate(Vector3.up * mouseX);
            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -_maxPitchAngle, _maxPitchAngle);
            if (_cameraTransform != null)
                _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        }
#endif
    }
}
