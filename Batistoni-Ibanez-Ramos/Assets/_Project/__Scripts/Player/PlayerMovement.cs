using UnityEngine;

namespace PatronesAplicados
{
    public class PlayerMovement
    {
        private readonly Rigidbody _rb;
        private readonly CapsuleCollider _col;
        private readonly Transform _transform;
        private readonly Transform _groundCheck;
        private readonly PlayerConfigSO _config;

        private readonly float _originalHeight;
        private readonly Vector3 _originalCenter;

        private float _turnSmoothVelocity;
        private RaycastHit _slopeHit;

        public Transform Transform => _transform;

        public PlayerMovement(Rigidbody rb, CapsuleCollider col, Transform transform, Transform groundCheck, PlayerConfigSO config)
        {
            _rb = rb;
            _col = col;
            _transform = transform;
            _groundCheck = groundCheck;
            _config = config;

            _originalHeight = _col.height;
            _originalCenter = _col.center;
        }

        public float GetHorizontalSpeed()
        {
            Vector3 vel = _rb.linearVelocity;
            return new Vector3(vel.x, 0f, vel.z).magnitude;
        }

        public Vector3 GetVelocity() => _rb.linearVelocity;

        public void SetVelocity(Vector3 velocity) => _rb.linearVelocity = velocity;

        public void ApplyForce(Vector3 force, ForceMode mode) => _rb.AddForce(force, mode);

        public bool CheckGrounded(float jumpCooldown)
        {
            bool isGrounded = Physics.CheckSphere(_groundCheck.position, _config.groundDistance, _config.groundMask);

            if (!isGrounded && jumpCooldown <= 0f)
            {
                isGrounded = Physics.Raycast(_groundCheck.position, Vector3.down, 1.0f, _config.groundMask);
            }
            return isGrounded;
        }

        public bool CheckWallForward(out RaycastHit wallHit)
        {
            return Physics.Raycast(_transform.position, _transform.forward, out wallHit, _config.wallCheckDistance, _config.wallMask);
        }

        public bool CheckWallBackward()
        {
            return Physics.Raycast(_transform.position, -_transform.forward, _config.wallCheckDistance + 0.2f, _config.wallMask);
        }

        public void SnapToGround(float jumpCooldown)
        {
            if (jumpCooldown > 0f) return;
            if (Physics.Raycast(_groundCheck.position, Vector3.down, 1.0f, _config.groundMask))
            {
                ApplyForce(Vector3.down * 200f, ForceMode.Acceleration);
            }
        }

        public bool OnSlope()
        {
            if (Physics.Raycast(_groundCheck.position, Vector3.down, out _slopeHit, 1.0f, _config.groundMask))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                return angle < _config.maxWalkableAngle && angle != 0f;
            }
            return false;
        }

        public Vector3 GetSlopeMoveDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
        }

        public Vector3 PreventSteepWallClimbing(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude < 0.01f) return moveDirection;

            Vector3 origin = _transform.position + Vector3.up * 0.5f;
            if (Physics.SphereCast(origin, _col.radius * 0.9f, moveDirection, out RaycastHit hit, 0.5f, _config.groundMask))
            {
                float wallAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (wallAngle > _config.maxWalkableAngle)
                {
                    Vector3 projected = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                    return new Vector3(projected.x, 0f, projected.z).normalized;
                }
            }
            return moveDirection;
        }

        public Vector3 UpdateRotation(Vector2 moveInput, float camEulerY)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camEulerY;

                if (float.IsNaN(_turnSmoothVelocity)) _turnSmoothVelocity = 0f;
                float safeSmoothTime = Mathf.Max(0.01f, _config.turnSmoothTime);

                float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, safeSmoothTime);

                if (!float.IsNaN(angle))
                {
                    _transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }
            }
            return Vector3.zero;
        }

        public void ExecuteJump(float jumpForce, bool isAirJump = false)
        {
            if (!isAirJump)
            {
                _transform.position += Vector3.up * 0.05f;
            }

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        public void StartSlideHitbox()
        {
            _col.height = _config.slideHeight;
            _col.center = new Vector3(_originalCenter.x, _originalCenter.y - (_originalHeight - _config.slideHeight) / 2f, _originalCenter.z);
        }

        public void StopSlideHitbox()
        {
            _col.height = _originalHeight;
            _col.center = _originalCenter;
        }
    }
}