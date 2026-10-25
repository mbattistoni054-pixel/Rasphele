using UnityEngine;

namespace PatronesAplicados
{
    public class PlayerView : MonoBehaviour
    {
        [Header("Componentes Visuales")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem footstepParticles;

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

        private PlayerModel _model;
        private float _footstepTimer;
        private bool _wasEmittingFootsteps;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();

            if (footstepParticles != null)
            {
                var em = footstepParticles.emission;
                em.enabled = false;
                footstepParticles.Play();
            }
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

        private void OnDestroy()
        {
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
        }

        private void HandleGroundedChanged(bool isGrounded)
        {
            if (animator != null) animator.SetBool(GroundedHash, isGrounded);
        }

        private void HandleMovingChanged(bool isMoving)
        {
            if (animator != null) animator.SetBool(MovingHash, isMoving);
        }

        private void HandleRunToggled(bool isRunning)
        {
            if (animator != null) animator.SetFloat(WalkHash, isRunning ? 0f : 1f);
        }

        private void HandleJumped()
        {
            if (animator != null) animator.SetTrigger(JumpHash);
            PlaySFX(jumpSound, "Jump");
        }

        private void HandleDashed()
        {
            PlaySFX(dashSound, "Dash");
        }

        private void HandleSlideChanged(bool isSliding)
        {
            if (animator != null) animator.SetBool(SlideHash, isSliding);
            if (isSliding) PlaySFX(slideSound, "Slide");
        }

        private void HandleDashCooldownUpdated(float current, float max)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.DashCooldownChanged, current, max);
            }
        }

        private void PlaySFX(AudioClip clip, string category)
        {
            if (clip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clip, category);
        }

        public void SetPaused(bool isPaused)
        {
            if (animator != null)
            {
                animator.speed = isPaused ? 0f : 1f;
            }

            if (isPaused && footstepParticles != null && _wasEmittingFootsteps)
            {
                var em = footstepParticles.emission;
                em.enabled = false;
                _wasEmittingFootsteps = false;
            }
        }

    }
}