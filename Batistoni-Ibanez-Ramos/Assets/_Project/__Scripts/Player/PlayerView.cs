using UnityEngine;

namespace PatronesAplicados
{
    public class PlayerView : MonoBehaviour
    {
        [Header("Componentes Visuales")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem footstepParticles;
        [SerializeField] private ParticleSystem grabXp;
        [SerializeField] private Renderer[] renderers;

        [Header("Partículas de Salto")]
        [Tooltip("Arrastra aquí tu PS_JumpDust.")]
        [SerializeField] private ParticleSystem jumpDustParticles;

        [Header("Post-Proceso Fullscreen Shader Graph (Solo Dash)")]
        [Tooltip("Arrastra aquí tu material 'Shader Graphs_Dash' (el mismo que pusiste en PC_Renderer -> Pass Material).")]
        [SerializeField] private Material dashFullscreenMaterial;
        [Tooltip("Tiempo que el efecto se mantiene al 100% mientras dura el Dash (0.2s).")]
        [SerializeField] private float dashHoldDuration = 0.2f;
        [Tooltip("Tiempo que tarda el efecto en volver suavemente a la cámara normal al terminar el Dash (0.1s).")]
        [SerializeField] private float dashReturnDuration = 0.1f;

        [Header("Audio")]
        public AudioClip footstepSound;
        public float baseFootstepInterval = 0.4f;
        public AudioClip dashSound;
        public AudioClip slideSound;
        public AudioClip jumpSound;

        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int MovingHash = Animator.StringToHash("Moving");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int SlideHash = Animator.StringToHash("Slide");

        private static readonly int DashIntensityId = Shader.PropertyToID("_DashIntensity");

        private PlayerModel _model;
        private float _footstepTimer;
        private bool _wasEmittingFootsteps;

        private float _dashHoldTimer;
        private float _currentDashEffectWeight;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();

            if (footstepParticles != null)
            {
                var em = footstepParticles.emission;
                em.enabled = false;
                footstepParticles.Play();
            }

            SetDashShaderIntensity(0f);
        }

        public void Initialize(PlayerModel model)
        {
            _model = model;

            _model.OnGroundedChanged += HandleGroundedChanged;
            _model.OnMovingChanged += HandleMovingChanged;
            _model.OnRunToggled += HandleRunToggled;
            _model.OnJumped += HandleJumped;
            _model.OnDashed += HandleDashed;
            _model.OnSlideChanged += HandleSlideChanged;
            _model.OnDashCooldownUpdated += HandleDashCooldownUpdated;
        }

        private void OnDisable()
        {
            _dashHoldTimer = 0f;
            _currentDashEffectWeight = 0f;
            SetDashShaderIntensity(0f);
        }

        private void OnDestroy()
        {
            SetDashShaderIntensity(0f);

            if (_model == null) return;

            _model.OnGroundedChanged -= HandleGroundedChanged;
            _model.OnMovingChanged -= HandleMovingChanged;
            _model.OnRunToggled -= HandleRunToggled;
            _model.OnJumped -= HandleJumped;
            _model.OnDashed -= HandleDashed;
            _model.OnSlideChanged -= HandleSlideChanged;
            _model.OnDashCooldownUpdated -= HandleDashCooldownUpdated;
        }

        public void TickView(float deltaTime)
        {
            if (_model == null) return;

            bool isMovingOnGround = _model.IsGrounded && _model.CurrentState == PlayerState.Walking && _model.IsMoving;

            if (footstepParticles != null && _wasEmittingFootsteps != isMovingOnGround)
            {
                var em = footstepParticles.emission;
                em.enabled = isMovingOnGround;
                _wasEmittingFootsteps = isMovingOnGround;
            }

            if (isMovingOnGround)
            {
                _footstepTimer -= deltaTime;
                if (_footstepTimer <= 0f)
                {
                    PlaySFX(footstepSound, "Footsteps");
                    _footstepTimer = _model.IsRunning ? baseFootstepInterval * 0.6f : baseFootstepInterval;
                }
            }
            else
            {
                _footstepTimer = 0f;
            }

            TickDashPostProcess(deltaTime);
        }

        private void TickDashPostProcess(float deltaTime)
        {
            if (_dashHoldTimer > 0f)
            {
                _dashHoldTimer -= deltaTime;
                _currentDashEffectWeight = 1f;
                SetDashShaderIntensity(1f);
                return;
            }

            if (_currentDashEffectWeight > 0f)
            {
                float returnSpeed = dashReturnDuration > 0.001f ? (1f / dashReturnDuration) : 10f;
                _currentDashEffectWeight = Mathf.MoveTowards(_currentDashEffectWeight, 0f, returnSpeed * deltaTime);
                SetDashShaderIntensity(_currentDashEffectWeight);
            }
        }

        private void SetDashShaderIntensity(float value)
        {
            if (dashFullscreenMaterial != null && dashFullscreenMaterial.HasProperty(DashIntensityId))
            {
                dashFullscreenMaterial.SetFloat(DashIntensityId, value);
            }
        }

        private void HandleGroundedChanged(bool isGrounded)
        {
            animator.SetBool(GroundedHash, isGrounded);
        }

        private void HandleMovingChanged(bool isMoving)
        {
            animator.SetBool(MovingHash, isMoving);
        }

        private void HandleRunToggled(bool isRunning)
        {
            animator.SetFloat(WalkHash, isRunning ? 0f : 1f);
        }

        private void HandleJumped()
        {
            animator.SetTrigger(JumpHash);
            PlaySFX(jumpSound, "Jump");

            if (jumpDustParticles != null)
            {
                jumpDustParticles.Clear();
                jumpDustParticles.Play();
            }
        }

        private void HandleDashed()
        {
            PlaySFX(dashSound, "Dash");

            _dashHoldTimer = (_model != null && _model.Config != null && _model.Config.dashDuration > 0f)
                ? _model.Config.dashDuration
                : dashHoldDuration;

            _currentDashEffectWeight = 1f;
            SetDashShaderIntensity(1f);
        }

        private void HandleSlideChanged(bool isSliding)
        {
            animator.SetBool(SlideHash, isSliding);
            if (isSliding) PlaySFX(slideSound, "Slide");
        }

        private void HandleDashCooldownUpdated(float current, float max)
        {
            EventManager.Instance.TriggerEvent(GameEventKeys.DashCooldownChanged, current, max);
        }

        private void PlaySFX(AudioClip clip, string category)
        {
            if (clip != null)
                AudioManager.Instance.PlaySFX(clip, category);
        }

        public void SetPaused(bool isPaused)
        {

            animator.speed = isPaused ? 0f : 1f;

            if (isPaused)
            {
                if (footstepParticles != null && _wasEmittingFootsteps)
                {
                    var em = footstepParticles.emission;
                    em.enabled = false;
                    _wasEmittingFootsteps = false;
                }
                if (jumpDustParticles != null && jumpDustParticles.isPlaying)
                {
                    jumpDustParticles.Pause();
                }
            }
            else
            {
                if (jumpDustParticles != null && jumpDustParticles.isPaused)
                {
                    jumpDustParticles.Play();
                }
            }
        }

        float xpCount;
        float actualXp;
        public void GrabXp()
        {
            grabXp.Emit(Random.Range(4, 9));

            if (xpCount <= 1)
            {
                xpCount += 0.1f;
            }
            else
            {
                xpCount = 1;
            }
        }

        private void Update()
        {
            if (xpCount > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (actualXp <= xpCount)
                    {
                        actualXp += Time.deltaTime / 5;
                    }
                    else
                    {
                        actualXp -= Time.deltaTime / 10;
                        xpCount -= Time.deltaTime / 10;
                    }
                    renderer.material.SetFloat("_xpValue", actualXp);
                }
            }
            else
            {
               xpCount = 0;
               actualXp = 0;
            }
        }
    }

}