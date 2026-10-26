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

        [Header("Audio")]
        public AudioClip hurtSound;

        private bool _hasShield;
        private float _timeSinceLastDamage;
        private bool _isPaused;

        private Volume _volume;
        private Vignette _vignette;

        private void OnEnable()
        {
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        private void Start()
        {
            if (PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.baseMaxHealth > 0f)
            {
                maxHealth = PlayerStatsRefactored.Instance.GetTotalMaxHealth();
            }

            currentHealth = maxHealth;

            CachePostProcessingVolume();

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening(GameEventKeys.PlayerStatsUpdated, UpdateMaxHealthFromStats);
            }

            UpdateHealthFeedbackEffects();
            Invoke(nameof(NotifyObservers), 0.1f);
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.PlayerStatsUpdated, UpdateMaxHealthFromStats);
            }
        }

        private void Update()
        {
            if (_isPaused) return;

            if (!_hasShield && PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.shieldStacks > 0)
            {
                _timeSinceLastDamage += Time.deltaTime;
                if (_timeSinceLastDamage >= 10f)
                {
                    _hasShield = true;
                    Debug.Log("¡Escudo de Panal generado! Bloqueará el próximo golpe.");
                }
            }
        }

        private void CachePostProcessingVolume()
        {
            if (GameManagerRefactored.Instance != null && GameManagerRefactored.Instance.globalVolume != null)
            {
                _volume = GameManagerRefactored.Instance.globalVolume.GetComponent<Volume>();
                if (_volume != null && _volume.profile != null)
                {
                    _volume.profile.TryGet(out _vignette);
                }
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

            if (AudioManager.Instance != null && AudioManager.Instance.mainMixer != null)
            {
                AudioMixer mixer = AudioManager.Instance.mainMixer;
                float targetPitch = (currentHealth < maxHealth * 0.5f) ? (0.5f + healthRatio) : 1f;
                mixer.SetFloat("MasterPitch", targetPitch);
            }
        }

        public void NotifyObservers()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.PlayerHealthChanged, currentHealth, maxHealth);
            }
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

            UpdateHealthFeedbackEffects();
            NotifyObservers();
        }

        private void ShowDamagePopup(float damageAmount)
        {
            if (damagePopupPrefab == null) return;

            Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;

            if (VfxPoolManager.Instance != null)
            {
                VfxPoolManager.Instance.SpawnDamagePopup(damagePopupPrefab, spawnPosition, damageAmount, DamageType.Fisico, false, true);
            }
            else
            {
                GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
                if (popup.TryGetComponent(out DamagePopup popupScript))
                {
                    popupScript.Setup(damageAmount, DamageType.Fisico, false, true);
                }
            }
        }

        public void TakeDamage(float amount, bool isCrit = false, DamageType type = DamageType.Fisico, bool isBleed = false)
        {
            _timeSinceLastDamage = 0f;

            if (_hasShield)
            {
                _hasShield = false;
                Debug.Log("¡Escudo de Panal destruido! Golpe bloqueado.");
                ShowDamagePopup(0f);
                return;
            }

            if (PlayerStatsRefactored.Instance != null)
            {
                amount *= PlayerStatsRefactored.Instance.itemDamageTakenMultiplier;
            }

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
            if (amount <= 0f || currentHealth >= maxHealth) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            UpdateHealthFeedbackEffects();
            NotifyObservers();
        }

        public void IncreaseMaxHealth(float amount)
        {
            if (PlayerStatsRefactored.Instance != null)
            {
                PlayerStatsRefactored.Instance.baseMaxHealth += amount;
                UpdateMaxHealthFromStats();
            }
            else
            {
                maxHealth += amount;
                currentHealth += amount;
                UpdateHealthFeedbackEffects();
                NotifyObservers();
            }
        }

        private void Die()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.PlayerDeath);
            }

            Destroy(gameObject);
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}