using UnityEngine;

public class PlayerMovement
{
    private Rigidbody _rb;
    private CapsuleCollider _col;
    private Transform _transform;
    private float _turnSmoothVelocity;

    public PlayerMovement(Rigidbody rb, CapsuleCollider col, Transform transform)
    {
        _rb = rb;
        _col = col;
        _transform = transform;
    }

    public float GetHorizontalSpeed()
    {
        return new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
    }

    public Vector3 GetVelocity() => _rb.linearVelocity;

    public void SetVelocity(Vector3 velocity)
    {
        _rb.linearVelocity = velocity;
    }

    public void ApplyForce(Vector3 force, ForceMode mode)
    {
        _rb.AddForce(force, mode);
    }

    public Vector3 UpdateRotation(Vector2 moveInput, float camEulerY, float turnSmoothTime)
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + camEulerY;

            if (float.IsNaN(_turnSmoothVelocity)) _turnSmoothVelocity = 0f;
            float safeSmoothTime = Mathf.Max(0.01f, turnSmoothTime);

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

    public void StartSlideHitbox(float slideHeight, float originalHeight, Vector3 originalCenter)
    {
        _col.height = slideHeight;
        _col.center = new Vector3(originalCenter.x, originalCenter.y - (originalHeight - slideHeight) / 2f, originalCenter.z);
    }

    public void StopSlideHitbox(float originalHeight, Vector3 originalCenter)
    {
        _col.height = originalHeight;
        _col.center = originalCenter;
    }
}