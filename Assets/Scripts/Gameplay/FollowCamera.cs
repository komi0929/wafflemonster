using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// FPS/TPS切り替え可能カメラ
    /// PlayerControllerの子として動作し、マウスによる上下回転を担当
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private bool _firstPerson = true;

        [Header("Third Person Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _tpsOffset = new Vector3(0f, 8f, -6f);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _lookAheadDistance = 2f;
        [SerializeField] private float _rotationDamping = 3f;

        [Header("Collision")]
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private LayerMask _collisionMask;

        private Vector3 _currentVelocity;

        private void Start()
        {
            // プレイヤーの子になっている場合はFPSモード
            if (transform.parent != null && transform.parent.GetComponent<PlayerController>() != null)
            {
                _firstPerson = true;
                _target = transform.parent;
            }
            else if (_target == null)
            {
                // Playerを自動検出
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            // FPSモード: PlayerControllerがカメラの回転を制御
            if (_firstPerson) return;

            // TPSモード: 従来の追従カメラ
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _tpsOffset;

            // 壁の衝突回避
            Vector3 dirToCamera = desiredPosition - _target.position;
            if (Physics.Raycast(_target.position, dirToCamera.normalized, out RaycastHit hit,
                dirToCamera.magnitude, _collisionMask))
            {
                desiredPosition = hit.point - dirToCamera.normalized * 0.3f;
                float dist = Vector3.Distance(desiredPosition, _target.position);
                if (dist < _minDistance)
                {
                    desiredPosition = _target.position + dirToCamera.normalized * _minDistance;
                }
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                1f / _smoothSpeed
            );

            Vector3 lookTarget = _target.position + _target.forward * _lookAheadDistance + Vector3.up * 1.5f;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationDamping * Time.deltaTime
            );
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
