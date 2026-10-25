using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PatronesAplicados
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerView))]
    public class PlayerControllerRefactored : MonoBehaviour, IKnockbackable, IRootable, IPausable
    {
        [Header("Configuración (Modelo SO)")]
        [SerializeField] private PlayerConfigSO config;

        [Header("Estado Actual (Solo Lectura)")]
        public PlayerState currentState = PlayerState.Walking;

        [Header("Referencias")]
        public Transform cam;
        public Transform groundCheck;

        [Header("Input System")]
        public InputActionReference moveAction;
        public InputActionReference jumpAction;
        public InputActionReference slideAction;
        public InputActionReference dashAction;
        public InputActionReference walkToggleAction;
        public InputActionReference togglePause;

        [Header("Canvas & Menús")]
        [SerializeField] private Canvas CanvasPause;
        [SerializeField] private GameObject optionsMenu;
        [SerializeField] private GameObject controlsMenu;

        private Rigidbody _rb;
        private PlayerModel _model;
        private PlayerView _view;
        private PlayerControls _controls;
        private PlayerMovement _movement;
        private PlayerHealthRefactored _health;

        private Dictionary<PlayerState, IPlayerState> _states;
        private IPlayerState _activeState;

        private bool _isPaused;
        private Vector3 _savedVelocity;
        private Vector3 _savedAngularVelocity;
        private bool _wasKinematicBeforePause;

        public Transform CameraTransform => cam;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            _rb.freezeRotation = true;

            _view = GetComponent<PlayerView>();
            _health = GetComponent<PlayerHealthRefactored>();

            _model = new PlayerModel(config);
            _controls = new PlayerControls(moveAction, jumpAction, slideAction, dashAction, walkToggleAction, togglePause, config.inputBufferTime);
            _movement = new PlayerMovement(_rb, col, transform, groundCheck, config);

            _view.Initialize(_model);

            InitializeStateMachine();

            if (GameManagerRefactored.Instance != null)
            {
                GameManagerRefactored.Instance.RegisterPlayer(transform);
            }
        }

        private void InitializeStateMachine()
        {
            _states = new Dictionary<PlayerState, IPlayerState>(5)
            {
                { PlayerState.Walking, new WalkingState(this, _model, _movement, _controls) },
                { PlayerState.Airborne, new AirborneState(this, _model, _movement, _controls) },
                { PlayerState.Sliding, new SlidingState(this, _model, _movement, _controls) },
                { PlayerState.WallRiding, new WallRidingState(this, _model, _movement, _controls) },
                { PlayerState.Dashing, new DashingState(this, _model, _movement, _controls) }
            };

            _activeState = _states[PlayerState.Walking];
            _activeState.Enter();
        }

        public void TransitionToState(PlayerState nextStateType)
        {
            if (_model.CurrentState == nextStateType && _activeState != null) return;

            _activeState?.Exit();
            _model.CurrentState = nextStateType;
            currentState = nextStateType;
            _activeState = _states[nextStateType];
            _activeState.Enter();
        }

        private void OnEnable()
        {
            _controls?.EnableKeys();
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            _controls?.DisableKeys();
            PauseManager.Unregister(this);
        }

        private void Update()
        {
            if (_controls.WasPausePressed())
            {
                TogglePause();
            }

            if (_isPaused) return;

            float dt = Time.deltaTime;

            if (_model.JumpCooldown > 0f) _model.JumpCooldown -= dt;

            _controls.ListenKeys(dt);
            UpdateDashCooldown(dt);
            UpdateMovementRegen(dt);
            UpdateEnvironmentTransitions(dt);

            if (_model.KnockbackTimer <= 0f)
            {
                _activeState?.HandleInput(dt);
            }

            _view.TickView(dt);
        }

        private void FixedUpdate()
        {
            if (_isPaused) return;

            float fdt = Time.fixedDeltaTime;

            if (_model.KnockbackTimer > 0f)
            {
                _model.KnockbackTimer -= fdt;
                Vector3 vel = _movement.GetVelocity();
                if (vel.y < 0f)
                {
                    _movement.SetVelocity(vel + Vector3.up * (Physics.gravity.y * (config.fallMultiplier - 1f) * fdt));
                }
                return;
            }

            _activeState?.UpdatePhysics(fdt);
        }

        private void UpdateDashCooldown(float dt)
        {
            int maxDashesAllowed = 1 + (PlayerStatsRefactored.Instance != null ? PlayerStatsRefactored.Instance.itemExtraDashes : 0);
            if (_model.CurrentDashesAvailable < maxDashesAllowed)
            {
                float nextTimer = _model.DashCooldownTimer - dt;
                if (nextTimer <= 0f)
                {
                    _model.CurrentDashesAvailable++;
                    nextTimer = (_model.CurrentDashesAvailable < maxDashesAllowed) ? config.dashCooldown : 0f;
                }
                _model.SetDashCooldown(nextTimer);
            }
            else
            {
                _model.SetDashCooldown(0f);
            }
        }

        private void UpdateMovementRegen(float dt)
        {
            if (_health == null || PlayerStatsRefactored.Instance == null || PlayerStatsRefactored.Instance.itemRegenMoving <= 0f)
                return;

            if (_movement.GetHorizontalSpeed() > 0.5f && _model.IsMoving && _model.IsGrounded)
            {
                _model.RegenTimer += dt;
                if (_model.RegenTimer >= 1f)
                {
                    _model.RegenTimer = 0f;
                    _health.Heal(PlayerStatsRefactored.Instance.itemRegenMoving);
                }
            }
        }

        private void UpdateEnvironmentTransitions(float dt)
        {
            _model.IsGrounded = _movement.CheckGrounded(_model.JumpCooldown);

            if (_model.IsGrounded) _model.CoyoteTimer = config.coyoteTime;
            else _model.CoyoteTimer -= dt;

            if (_model.IsGrounded && _model.CurrentState == PlayerState.Airborne && _movement.GetVelocity().y <= 0f && _model.JumpCooldown <= 0f)
            {
                _model.AdditionalJumpsLeft = PlayerStatsRefactored.Instance != null ? PlayerStatsRefactored.Instance.itemExtraJumps : 0;
                TransitionToState(PlayerState.Walking);
            }
            else if (!_model.IsGrounded && _model.CurrentState == PlayerState.Walking && _model.CoyoteTimer <= 0f)
            {
                TransitionToState(PlayerState.Airborne);
            }

            if (_model.CurrentState == PlayerState.Airborne && _model.JumpCooldown <= 0f)
            {
                if (_movement.CheckWallForward(out RaycastHit wallHit))
                {
                    _model.LastWallHit = wallHit;
                    TransitionToState(PlayerState.WallRiding);
                }
            }
        }

        private void TogglePause()
        {
            if (CanvasPause == null) return;

            bool willActivate = !CanvasPause.gameObject.activeSelf;
            if (!willActivate && ((controlsMenu != null && controlsMenu.activeSelf) || (optionsMenu != null && optionsMenu.activeSelf)))
            {
                return;
            }

            CanvasPause.gameObject.SetActive(willActivate);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(willActivate ? GameEventKeys.PauseRequested : GameEventKeys.ResumeRequested);
            }
        }

        public void ApplyKnockback(Vector3 force, float stunDuration = 0.4f)
        {
            _model.KnockbackTimer = stunDuration;
            _model.JumpCooldown = stunDuration;
            TransitionToState(PlayerState.Airborne);

            _movement.SetVelocity(Vector3.zero);
            _movement.ApplyForce(force, ForceMode.Impulse);
        }

        public void ApplyRoot(float duration)
        {
            _model.KnockbackTimer = Mathf.Max(_model.KnockbackTimer, duration);
            _movement.SetVelocity(new Vector3(0f, _movement.GetVelocity().y, 0f));
        }

        public void OnPause()
        {
            if (_isPaused) return;
            _isPaused = true;

            _view?.SetPaused(true);

            if (_rb != null)
            {
                _wasKinematicBeforePause = _rb.isKinematic;
                if (!_rb.isKinematic)
                {
                    _savedVelocity = _rb.linearVelocity;
                    _savedAngularVelocity = _rb.angularVelocity;
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.isKinematic = true;
                }
            }
        }

        public void OnResume()
        {
            if (!_isPaused) return;
            _isPaused = false;

            _view?.SetPaused(false);

            if (_rb != null)
            {
                _rb.isKinematic = _wasKinematicBeforePause;
                if (!_rb.isKinematic)
                {
                    _rb.linearVelocity = _savedVelocity;
                    _rb.angularVelocity = _savedAngularVelocity;
                }
            }
        }

    }
}