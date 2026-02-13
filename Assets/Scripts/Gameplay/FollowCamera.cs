using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// 3人称追従カメラ（Cinemachineなしの軽量版）
    /// プレイヤーの背後上方から追従
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Camera Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 8f, -6f);
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _lookAheadDistance = 2f;
        [SerializeField] private float _rotationDamping = 3f;

        [Header("Collision")]
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private LayerMask _collisionMask;

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (_target == null) return;

            // 目標位置：プレイヤーの後方上方
            Vector3 desiredPosition = _target.position + _offset;

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

            // スムーズ追従
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                1f / _smoothSpeed
            );

            // プレイヤーの少し前方を見る
            Vector3 lookTarget = _target.position + _target.forward * _lookAheadDistance + Vector3.up * 1.5f;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _rotationDamping * Time.deltaTime
            );
        }

        /// <summary>
        /// ターゲットを設定
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
