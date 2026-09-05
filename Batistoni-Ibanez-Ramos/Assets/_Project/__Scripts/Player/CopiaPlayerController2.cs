using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CopiaPlayerController2 : MonoBehaviour
{
    public delegate void DashCooldownChangedHandler(float currentCooldown, float maxCooldown);
    public static event DashCooldownChangedHandler OnDashCooldownChanged;

    public enum PlayerState { Walking, Airborne, Sliding, WallRiding, Dashing }

    [Header("Estado Actual")]
    public PlayerState currentState = PlayerState.Walking;

    private PlayerControls _controls;
    private PlayerMovement _movement;

    [Header("Referencias")]
    private Rigidbody rb;
    private CapsuleCollider col;
    [SerializeField] Animator animator;
    public Transform cam;

    [Header("Ajustes de Velocidad")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    private float currentSpeed;
    private bool isRunning = true;
    public float turnSmoothTime = 0.1f;

    [Header("Ajustes de Inclinación (Anti-Spiderman)")]
    [Tooltip("Ángulo máximo que el jugador puede subir caminando o corriendo. Paredes más empinadas lo bloquearán.")]
    public float maxWalkableAngle = 45f;

    [Header("Ajustes de Salto")]
    public float jumpForce = 7f;
    public float fallMultiplier = 5f;
    public float lowJumpMultiplier = 4f;
    public float airControl = 5f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    private bool isGrounded;
    private float jumpCooldown;
    private int additionalJumpsLeft;

    [Header("Ajustes de Deslizamiento")]
    public float minSlideSpeed = 4f;
    public float slideBoost = 5f;
    public float slideDrag = 15f;
    public float slopeAcceleration = 10f;
    public float slideHeight = 1f;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 slideDirection;

    [Header("Ajustes de Dash")]
    public float dashSpeed = 50f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2f;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private Vector3 dashDirection;
    private int currentDashesAvailable = 1;

    [Header("Ajustes de Wall Jump")]
    public LayerMask wallMask;
    public float wallCheckDistance = 0.8f;
    public float wallSlideSpeed = -2f;
    public float wallJumpUpForce = 8f;
    public float wallJumpSideForce = 8f;
    private RaycastHit wallHit;

    [Header("Input System")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference slideAction;
    public InputActionReference dashAction;
    public InputActionReference walkToggleAction;
    public InputActionReference togglePause;

    [Header("Canvas & Timers")]
    [SerializeField] Canvas CanvasPause;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject controlsMenu;
    public float inputBufferTime = 0.2f;
    public float coyoteTime = 0.15f;
    private float coyoteTimer;
    private float regenTimer;
    public float knockbackTimer = 0f;

    [Header("Audio y VFX")]
    public AudioClip footstepSound;
    public float baseFootstepInterval = 0.4f;
    private float footstepTimer;
    public AudioClip dashSound;
    public AudioClip slideSound;
    public AudioClip jumpSound;
    public ParticleSystem footstepParticles;

    private Vector3 moveDirection;

    // En lugar de usar la velocidad base, esto calcula tu velocidad FINAL con objetos.
    private float FinalSpeed
    {
        get
        {
            if (PlayerStats.Instance != null)
                return PlayerStats.Instance.GetTotalSpeed(currentSpeed);

            return currentSpeed; // Si por algún motivo no hay stats, usa la normal
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        if (animator == null) animator = GetComponent<Animator>();
        rb.freezeRotation = true;

        originalHeight = col.height;
        originalCenter = col.center;
        currentSpeed = runSpeed;

        _controls = new PlayerControls(moveAction, jumpAction, slideAction, dashAction, walkToggleAction, togglePause, inputBufferTime);
        _movement = new PlayerMovement(rb, col, transform);
    }

    void Start()
    {
        if (footstepParticles != null)
        {
            var em = footstepParticles.emission;
            em.enabled = false;
            footstepParticles.Play();
        }
    }

    private void OnEnable() => _controls?.EnableKeys();
    private void OnDisable() => _controls?.DisableKeys();

    void Update()
    {
        if (jumpCooldown > 0) jumpCooldown -= Time.deltaTime;

        _controls?.ListenKeys(Time.deltaTime);

        int maxDashesAllowed = 1 + ((PlayerStats.Instance != null) ? PlayerStats.Instance.itemExtraDashes : 0);
        if (currentDashesAvailable < maxDashesAllowed)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                currentDashesAvailable++;
                dashCooldownTimer = (currentDashesAvailable < maxDashesAllowed) ? dashCooldown : 0f;
            }
        }
        else dashCooldownTimer = 0f;

        if (PlayerStats.Instance != null && PlayerStats.Instance.itemRegenMoving > 0f)
        {
            if (_movement.GetHorizontalSpeed() > 0.5f && moveDirection.magnitude > 0.1f && isGrounded)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= 1f)
                {
                    regenTimer = 0f;
                    GetComponent<PlayerHealth>()?.Heal(PlayerStats.Instance.itemRegenMoving);
                }
            }
        }

        OnDashCooldownChanged?.Invoke(dashCooldownTimer, dashCooldown);

        CheckGrounded();
        CheckWall();

        bool isMovingOnGround = isGrounded && currentState == PlayerState.Walking && moveDirection.magnitude > 0.1f;

        if (footstepParticles != null)
        {
            var em = footstepParticles.emission;
            em.enabled = isMovingOnGround;
        }

        if (isMovingOnGround)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (footstepSound != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(footstepSound, "Footsteps");
                footstepTimer = isRunning ? baseFootstepInterval * 0.6f : baseFootstepInterval;
            }
        }
        else footstepTimer = 0f;

        if (knockbackTimer <= 0f)
        {
            switch (currentState)
            {
                case PlayerState.Walking: HandleWalkingInput(); break;
                case PlayerState.Airborne: HandleAirborneInput(); break;
                case PlayerState.Sliding: HandleSlidingInput(); break;
                case PlayerState.WallRiding: HandleWallRidingInput(); break;
                case PlayerState.Dashing: HandleDashingInput(); break;
            }
        }

        if (_controls.WasPausePressed()) TogglePause();
        if (animator != null) animator.SetBool("Grounded", isGrounded);
    }

    private void FixedUpdate()
    {

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            // Le aplicamos la gravedad extra para que caiga bien y no flote como papel
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;

            return; // Cortamos la función aquí para que el script no sobreescriba el empujón
        }

        switch (currentState)
        {
            case PlayerState.Walking: HandleWalkingPhysics(); break;
            case PlayerState.Airborne: HandleAirbornePhysics(); break;
            case PlayerState.Sliding: HandleSlidingPhysics(); break;
            case PlayerState.WallRiding: HandleWallRidingPhysics(); break;
            case PlayerState.Dashing: HandleDashingPhysics(); break;
        }
    }

    private void CheckGrounded()
    {
        bool sphereGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isGrounded = sphereGrounded;

        if (!isGrounded && jumpCooldown <= 0f)
        {
            if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 1.0f, groundMask))
            {
                isGrounded = true;
            }
        }

        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (isGrounded && currentState == PlayerState.Airborne && _movement.GetVelocity().y <= 0 && jumpCooldown <= 0f)
        {
            currentState = PlayerState.Walking;
            additionalJumpsLeft = (PlayerStats.Instance != null) ? PlayerStats.Instance.itemExtraJumps : 0;
        }
        else if (!isGrounded && currentState == PlayerState.Walking && coyoteTimer <= 0f)
        {
            currentState = PlayerState.Airborne;
        }
    }

    private void CheckWall()
    {
        if (currentState == PlayerState.Airborne && jumpCooldown <= 0f)
        {
            if (Physics.Raycast(transform.position, transform.forward, out wallHit, wallCheckDistance, wallMask))
            {
                StartWallRiding();
            }
        }
    }

    private void SnapToGround()
    {
        if (jumpCooldown > 0) return;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 1.0f, groundMask))
        {
            _movement.ApplyForce(Vector3.down * 200f, ForceMode.Acceleration);
        }
    }

    private RaycastHit slopeHit;
    private bool OnSlope()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, 1.0f, groundMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // Usamos la nueva variable también aquí para mayor consistencia
            return angle < maxWalkableAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;

    private void PreventSteepWallClimbing()
    {
        if (moveDirection.magnitude < 0.1f) return;

        // Lanzamos una esfera virtual (SphereCast) desde la cintura hacia adelante
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, col.radius * 0.9f, moveDirection, out RaycastHit hit, 0.5f, groundMask))
        {
            float wallAngle = Vector3.Angle(Vector3.up, hit.normal);

            // Si la pared es más empinada que tu máximo permitido (ej. 80 grados > 45)
            if (wallAngle > maxWalkableAngle)
            {
                // Proyectamos el vector de movimiento sobre el plano de la pared.
                // Esto anula la fuerza penetrante y hace que resbales suavemente hacia los lados.
                Vector3 projected = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                moveDirection = new Vector3(projected.x, 0f, projected.z).normalized;
            }
        }
    }

    private void HandleWalkingInput()
    {
        if (_controls.WasWalkToggled())
        {
            isRunning = !isRunning;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            if (animator != null) animator.SetFloat("Walk", isRunning ? 0 : 1);
        }

        moveDirection = _movement.UpdateRotation(_controls.GetMoveInput(), cam.eulerAngles.y, turnSmoothTime);

        // Bloqueo Anti-Spiderman
        PreventSteepWallClimbing();

        if (animator != null) animator.SetBool("Moving", moveDirection.magnitude >= 0.1f);

        if (_controls.dashBuffer > 0f && currentDashesAvailable > 0)
        {
            _controls.dashBuffer = 0f;
            StartDash();
            return;
        }

        if (_controls.slideBuffer > 0f && isGrounded && _movement.GetHorizontalSpeed() >= minSlideSpeed)
        {
            _controls.slideBuffer = 0f;
            StartSlide();
            return;
        }

        if (_controls.jumpBuffer > 0f && (coyoteTimer > 0f || isGrounded))
        {
            _controls.jumpBuffer = 0f; coyoteTimer = 0f; jumpCooldown = 0.2f;
            currentState = PlayerState.Airborne;

            _movement.ExecuteJump(jumpForce, false);
            if (animator != null) animator.SetTrigger("Jump");
            if (jumpSound != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(jumpSound, "Jump");
        }
    }

    private void HandleWalkingPhysics()
    {
        if (OnSlope())
        {
            Vector3 slopeDirection = GetSlopeMoveDirection(moveDirection);
            // --- CAMBIO: Usamos FinalSpeed en lugar de currentSpeed ---
            _movement.SetVelocity(slopeDirection * FinalSpeed);
            if (moveDirection == Vector3.zero) _movement.SetVelocity(Vector3.zero);
        }
        else
        {
            // --- CAMBIO: Usamos FinalSpeed en lugar de currentSpeed ---
            _movement.SetVelocity(new Vector3(moveDirection.x * FinalSpeed, _movement.GetVelocity().y, moveDirection.z * FinalSpeed));
        }
        SnapToGround();
    }

    private void HandleAirborneInput()
    {
        moveDirection = _movement.UpdateRotation(_controls.GetMoveInput(), cam.eulerAngles.y, turnSmoothTime);

        // Bloqueo Anti-Spiderman en el aire (evita escalar saltando hacia la pared)
        PreventSteepWallClimbing();

        if (_controls.dashBuffer > 0f && currentDashesAvailable > 0)
        {
            _controls.dashBuffer = 0f; StartDash(); return;
        }

        if (_controls.jumpBuffer > 0f && additionalJumpsLeft > 0 && jumpCooldown <= 0f)
        {
            _controls.jumpBuffer = 0f; jumpCooldown = 0.2f; additionalJumpsLeft--;
            _movement.ExecuteJump(jumpForce, true);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    private void HandleAirbornePhysics()
    {
        Vector3 currentHorizontal = new Vector3(_movement.GetVelocity().x, 0, _movement.GetVelocity().z);
        // --- CAMBIO: Usamos FinalSpeed en lugar de currentSpeed ---
        Vector3 targetHorizontal = moveDirection * FinalSpeed;

        Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, targetHorizontal, airControl * Time.fixedDeltaTime);
        _movement.SetVelocity(new Vector3(newHorizontal.x, _movement.GetVelocity().y, newHorizontal.z));

        if (_movement.GetVelocity().y < 0)
            _movement.SetVelocity(_movement.GetVelocity() + Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
        else if (_movement.GetVelocity().y > 0 && !_controls.IsJumpPressed())
            _movement.SetVelocity(_movement.GetVelocity() + Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
    }

    private void HandleSlidingInput()
    {
        if (_controls.jumpBuffer > 0f && (coyoteTimer > 0f || isGrounded))
        {
            _controls.jumpBuffer = 0f; coyoteTimer = 0f; jumpCooldown = 0.2f;
            StopSlide();
            currentState = PlayerState.Airborne;
            _movement.ExecuteJump(jumpForce, false);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    private void HandleSlidingPhysics()
    {
        float currentSlideSpeed = _movement.GetHorizontalSpeed();

        if (OnSlope())
        {
            Vector3 slopeDir = GetSlopeMoveDirection(slideDirection);
            currentSlideSpeed += (slopeDir.y < -0.1f) ? slopeAcceleration * Time.fixedDeltaTime : -slideDrag * Time.fixedDeltaTime;
            currentSlideSpeed = Mathf.Clamp(currentSlideSpeed, 0, 25f);
            _movement.SetVelocity(slopeDir * currentSlideSpeed);
        }
        else
        {
            currentSlideSpeed -= slideDrag * Time.fixedDeltaTime;
            _movement.SetVelocity(new Vector3(slideDirection.x * currentSlideSpeed, _movement.GetVelocity().y, slideDirection.z * currentSlideSpeed));
        }

        SnapToGround();
        if (currentSlideSpeed < minSlideSpeed || !_controls.IsSlidePressed()) StopSlide();
    }

    private void StartSlide()
    {
        currentState = PlayerState.Sliding;
        _movement.StartSlideHitbox(slideHeight, originalHeight, originalCenter);
        slideDirection = transform.forward;
        _movement.ApplyForce(slideDirection * slideBoost, ForceMode.Impulse);
        if (animator != null) animator.SetBool("Slide", true);
        if (slideSound != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(slideSound, "Slide");
    }

    private void StopSlide()
    {
        currentState = PlayerState.Walking;
        _movement.StopSlideHitbox(originalHeight, originalCenter);
        if (animator != null) animator.SetBool("Slide", false);
    }

    private void HandleDashingInput()
    {
        dashTimeLeft -= Time.deltaTime;
        if (dashTimeLeft <= 0f) StopDash();
    }

    private void HandleDashingPhysics() => _movement.SetVelocity(dashDirection * dashSpeed);

    private void StartDash()
    {
        if (currentState == PlayerState.Sliding) StopSlide();
        currentState = PlayerState.Dashing;
        dashTimeLeft = dashDuration;
        currentDashesAvailable--;
        if (dashCooldownTimer <= 0f) dashCooldownTimer = dashCooldown;
        dashDirection = transform.forward;
        if (dashSound != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(dashSound, "Dash");
    }

    private void StopDash()
    {
        currentState = isGrounded ? PlayerState.Walking : PlayerState.Airborne;
        // --- CAMBIO: Usamos FinalSpeed en lugar de currentSpeed ---
        _movement.SetVelocity(_movement.GetVelocity().normalized * FinalSpeed);
    }

    private void HandleWallRidingInput()
    {
        if (_controls.jumpBuffer > 0f)
        {
            _controls.jumpBuffer = 0f; jumpCooldown = 0.2f;
            StopWallRiding();
            Vector3 jumpDir = (transform.forward * wallJumpSideForce) + (Vector3.up * wallJumpUpForce);
            _movement.SetVelocity(Vector3.zero);
            _movement.ApplyForce(jumpDir, ForceMode.Impulse);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    private void HandleWallRidingPhysics()
    {
        if (!Physics.Raycast(transform.position, -transform.forward, out RaycastHit backHit, wallCheckDistance + 0.2f, wallMask))
        {
            StopWallRiding(); return;
        }

        _movement.SetVelocity(new Vector3(0f, wallSlideSpeed, 0f));
        _movement.ApplyForce(-transform.forward * 10f, ForceMode.Force);

        if (isGrounded)
        {
            StopWallRiding();
            currentState = PlayerState.Walking;
        }
    }

    private void StartWallRiding()
    {
        currentState = PlayerState.WallRiding;
        _movement.SetVelocity(Vector3.zero);
        transform.rotation = Quaternion.LookRotation(wallHit.normal);
    }

    private void StopWallRiding() => currentState = PlayerState.Airborne;

    private void TogglePause()
    {
        if (CanvasPause != null)
        {
            if (!CanvasPause.gameObject.activeSelf) CanvasPause.gameObject.SetActive(true);
            // else if ((controlsMenu == null || !controlsMenu.activeSelf) && (optionsMenu == null || !optionsMenu.activeSelf))
            //     CanvasPause.gameObject.SetActive(false);

            else if (CanvasPause.gameObject.activeSelf) CanvasPause.gameObject.SetActive(false);
        }
    }

    public void ApplyKnockback(Vector3 force, float stunDuration = 0.4f)
    {
        knockbackTimer = stunDuration;
        currentState = PlayerState.Airborne;
        jumpCooldown = stunDuration; // Evita que el SnapToGround lo pegue al piso

        rb.linearVelocity = Vector3.zero; // Frenamos inercia previa
        rb.AddForce(force, ForceMode.Impulse); // Lo hacemos volar
    }
}