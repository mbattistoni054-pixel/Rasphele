using PatronesAplicados.RealImplementation;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PatronesAplicados
{
    public class PlayerHealthRefactored : MonoBehaviour, IDamageable, IHealable, IPausable
    {
        [Header("Estadísticas de Salud")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("UI y Efectos")]
        public GameObject damagePopupPrefab;

        [Header("Shader Graph Interactivo: Escudo (Unlit/Lit Shader Graph)")]
        [Tooltip("Arrastra aquí la Esfera hija del jugador que tiene el Material creado desde tu Shader Graph del Escudo.")]
        public Renderer shieldRenderer;
        [Tooltip("Marca esta casilla para que el escudo arranque activo desde el segundo 0 y puedas mostrarlo en la entrega de Efectos Visuales.")]
        public bool debugForceShieldOnStart = true;
        [Tooltip("Velocidad con la que el escudo aparece o se disuelve al romperse.")]
        public float shieldTransitionSpeed = 3.0f;

        [Header("Audio")]
        public AudioClip hurtSound;

        private bool _hasShield;
        private float _timeSinceLastDamage;
        private bool _isPaused;
        private bool _isDead;

        private Volume _volume;
        private Vignette _vignette;

        private MaterialPropertyBlock _shieldPropBlock;
        private float _currentShieldAlpha;
        private float _targetShieldAlpha;
        private float _currentDissolve;
        private float _currentHitRipple;
        private bool _isDissolvingBreak;

        private static readonly int ShieldAlphaId = Shader.PropertyToID("_ShieldAlpha");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int HitIntensityId = Shader.PropertyToID("_HitIntensity");

        private void Awake()
        {
            _shieldPropBlock = new MaterialPropertyBlock();
            InitializeShieldVisuals();
        }

        private void OnEnable()
        {
            _isDead = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        private void Start()
        {
            maxHealth = PlayerStatsRefactored.Instance.GetTotalMaxHealth();

            currentHealth = maxHealth;

            CachePostProcessingVolume();

            EventManager.Instance.StartListening(GameEventKeys.PlayerStatsUpdated, UpdateMaxHealthFromStats);

            if (debugForceShieldOnStart)
            {
                ActivateHoneycombShield();
            }

            UpdateHealthFeedbackEffects();
            Invoke(nameof(NotifyObservers), 0.1f);
        }

        private void OnDestroy()
        {
            EventManager.Instance.StopListening(GameEventKeys.PlayerStatsUpdated, UpdateMaxHealthFromStats);
        }

        private void Update()
        {
            if (_isPaused) return;

            float dt = Time.deltaTime;

            bool canGenerateShield = debugForceShieldOnStart || (PlayerStatsRefactored.Instance.shieldStacks > 0);

            if (!_hasShield && canGenerateShield)
            {
                _timeSinceLastDamage += dt;
                if (_timeSinceLastDamage >= 10f)
                {
                    ActivateHoneycombShield();
                }
            }

            TickShieldShader(dt);
        }

        private void InitializeShieldVisuals()
        {

            _currentShieldAlpha = 0f;
            _targetShieldAlpha = 0f;
            _currentDissolve = 0f;
            _currentHitRipple = 0f;
            _isDissolvingBreak = false;

            ApplyShieldShaderProperties();
            shieldRenderer.gameObject.SetActive(false);
        }

        [ContextMenu("Probar Activar Escudo")]
        public void ActivateHoneycombShield()
        {
            _hasShield = true;
            _isDissolvingBreak = false;
            _currentDissolve = 0f;
            _currentHitRipple = 0.6f;
            _targetShieldAlpha = 1f;

            shieldRenderer.gameObject.SetActive(true);
            ApplyShieldShaderProperties();

        }

        private void TriggerShieldBreakShaderEffect()
        {
            _hasShield = false;
            _isDissolvingBreak = true;
            _currentHitRipple = 1f;
            _targetShieldAlpha = 0f;
        }

        private void TickShieldShader(float dt)
        {

            bool needsUpdate = false;

            if (_isDissolvingBreak)
            {
                _currentDissolve = Mathf.MoveTowards(_currentDissolve, 1f, shieldTransitionSpeed * 0.8f * dt);
                _currentShieldAlpha = Mathf.MoveTowards(_currentShieldAlpha, 0f, shieldTransitionSpeed * dt);
                needsUpdate = true;

                if (_currentDissolve >= 0.99f && _currentShieldAlpha <= 0.01f)
                {
                    _isDissolvingBreak = false;
                    shieldRenderer.gameObject.SetActive(false);
                    return;
                }
            }
            else if (!Mathf.Approximately(_currentShieldAlpha, _targetShieldAlpha))
            {
                _currentShieldAlpha = Mathf.MoveTowards(_currentShieldAlpha, _targetShieldAlpha, shieldTransitionSpeed * dt);
                needsUpdate = true;
            }

            if (_currentHitRipple > 0f)
            {
                _currentHitRipple = Mathf.MoveTowards(_currentHitRipple, 0f, shieldTransitionSpeed * 1.5f * dt);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                ApplyShieldShaderProperties();
            }
        }

        private void ApplyShieldShaderProperties()
        {
            if (_shieldPropBlock == null) _shieldPropBlock = new MaterialPropertyBlock();

            shieldRenderer.GetPropertyBlock(_shieldPropBlock);
            _shieldPropBlock.SetFloat(ShieldAlphaId, _currentShieldAlpha);
            _shieldPropBlock.SetFloat(DissolveAmountId, _currentDissolve);
            _shieldPropBlock.SetFloat(HitIntensityId, _currentHitRipple);
            shieldRenderer.SetPropertyBlock(_shieldPropBlock);
        }

        private void CachePostProcessingVolume()
        {
           _volume = GameManagerRefactored.Instance.globalVolume.GetComponent<Volume>();
            if (_volume != null && _volume.profile != null)
            {
               _volume.profile.TryGet(out _vignette);
            }
        }

        private void UpdateHealthFeedbackEffects()
        {
            if (maxHealth <= 0f) return;

            float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);
            float vignetteFactor = Mathf.Max(0f, 0.5f - healthRatio);

            if (_vignette == null) CachePostProcessingVolume();

            if (_vignette != null)
            {
                _vignette.intensity.value = vignetteFactor * 1.5f;
                _vignette.intensity.overrideState = true;
            }

            AudioMixer mixer = AudioManager.Instance.mainMixer;
            float targetPitch = (currentHealth < maxHealth * 0.5f) ? (0.5f + healthRatio) : 1f;
            mixer.SetFloat("MasterPitch", targetPitch);

        }

        public void NotifyObservers()
        {
            EventManager.Instance.TriggerEvent(GameEventKeys.PlayerHealthChanged, currentHealth, maxHealth);
        }

        public void UpdateMaxHealthFromStats()
        {
            if (PlayerStatsRefactored.Instance == null) return;

            float oldMaxHealth = maxHealth;
            maxHealth = PlayerStatsRefactored.Instance.GetTotalMaxHealth();

            float difference = maxHealth - oldMaxHealth;
            if (difference > 0f)
            {
                currentHealth += difference;
            }

            if (currentHealth > maxHealth) currentHealth = maxHealth;

            if (!_hasShield && PlayerStatsRefactored.Instance.shieldStacks > 0 && _timeSinceLastDamage <= 0.01f)
            {
                ActivateHoneycombShield();
            }

            UpdateHealthFeedbackEffects();
            NotifyObservers();
        }

        private void ShowDamagePopup(float damageAmount)
        {
            if (damagePopupPrefab == null) return;

            Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;

            VfxPoolManager.Instance.SpawnDamagePopup(damagePopupPrefab, spawnPosition, damageAmount, DamageType.Fisico, false, true);

        }

        public void TakeDamage(float amount, bool isCrit = false, DamageType type = DamageType.Fisico, bool isBleed = false)
        {
            if (_isDead || _isPaused || PauseManager.IsPaused || amount <= 0f) return;

            _timeSinceLastDamage = 0f;

            if (_hasShield)
            {
                TriggerShieldBreakShaderEffect();
                ShowDamagePopup(0f);
                return;
            }

            amount *= PlayerStatsRefactored.Instance.itemDamageTakenMultiplier;

            if (hurtSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hurtSound, "PlayerHurt");
            }

            ShowDamagePopup(amount);
            currentHealth = Mathf.Max(0f, currentHealth - amount);

            UpdateHealthFeedbackEffects();
            NotifyObservers();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead || amount <= 0f || currentHealth >= maxHealth) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            UpdateHealthFeedbackEffects();
            NotifyObservers();
        }

        public void IncreaseMaxHealth(float amount)
        {

            PlayerStatsRefactored.Instance.baseMaxHealth += amount;
            UpdateMaxHealthFromStats();

        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            EventManager.Instance.TriggerEvent(GameEventKeys.PlayerDeath);

            Destroy(gameObject);
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}