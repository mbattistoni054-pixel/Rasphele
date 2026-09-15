using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace PatronesAplicados
{
    public class PlayerHealthRefactored : MonoBehaviour//, IObservable
    {
        [Header("Estadsticas de Salud")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("UI y Efectos")]
        public GameObject damagePopupPrefab;

        [Header("Audio")]
        public AudioClip hurtSound;

        // Variables internas
        private bool hasShield = false;
        private float timeSinceLastDamage = 0f;

        // Lista de observadores (Patrn Observer)
        private List<IObserver> observers = new List<IObserver>();

        private Volume volume;
        private Vignette vignette;

        void Start()
        {
            if (PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.baseMaxHealth > 0)
            {
                maxHealth = PlayerStatsRefactored.Instance.GetTotalMaxHealth();
            }

            currentHealth = maxHealth;
            Invoke(nameof(NotifyObservers), 0.1f);
        }

        void Update()
        {
            // Monitoreo de inactividad para activar el escudo (Panal)
            if (PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.shieldStacks > 0)
            {
                if (!hasShield)
                {
                    timeSinceLastDamage += Time.deltaTime;
                    if (timeSinceLastDamage >= 10f)
                    {
                        hasShield = true;
                        Debug.Log(" Escudo de Panal generado! Bloquear el prximo golpe.");
                    }
                }
            }

            float value = Mathf.Clamp01(currentHealth / maxHealth);
            float actualValue = 0.5f - value;

            if (GameManagerRefactored.Instance != null && GameManagerRefactored.Instance.globalVolume != null)
            {
                volume = GameManagerRefactored.Instance.globalVolume.GetComponent<Volume>();
            }
            else if (GameManager.Instance != null && GameManager.Instance.globalVolume != null)
            {
                volume = GameManager.Instance.globalVolume.GetComponent<Volume>();
            }

            if (AudioManager.Instance != null)
            {
                AudioMixer audio = AudioManager.Instance.mainMixer;
                if (audio != null)
                {
                    if (currentHealth < maxHealth / 2)
                    {
                        float newPitch = 0.5f + value;
                        audio.SetFloat("MasterPitch", newPitch);
                    }
                    else
                    {
                        audio.SetFloat("MasterPitch", 1f);
                    }
                }
            }

            if (volume != null && volume.profile.TryGet(out vignette))
            {
                vignette.intensity.value = actualValue * 1.5f;
                vignette.intensity.overrideState = true;
            }
        }

        // --- IMPLEMENTACIN DE ISUBJECT ---

        public void Subscribe(IObserver observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void Unsubscribe(IObserver observer)
        {
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
               // observer.OnNotify(currentHealth, maxHealth);
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("PlayerHealthChanged", currentHealth, maxHealth);
            }
        }

        public void UpdateMaxHealthFromStats()
        {
            if (PlayerStatsRefactored.Instance == null) return;

            float oldMaxHealth = maxHealth;
            maxHealth = PlayerStatsRefactored.Instance.GetTotalMaxHealth();

            float difference = maxHealth - oldMaxHealth;
            if (difference > 0)
            {
                currentHealth += difference;
            }

            if (currentHealth > maxHealth) currentHealth = maxHealth;

            NotifyObservers();
        }

        private void ShowDamagePopup(float damageAmount)
        {
            if (damagePopupPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;
                GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);

                DamagePopup popupScript = popup.GetComponent<DamagePopup>();
                if (popupScript != null)
                {
                    popupScript.Setup(damageAmount, DamageType.Fisico, false, true);
                }
            }
        }

        public void TakeDamage(float amount)
        {
            timeSinceLastDamage = 0f;

            if (hasShield)
            {
                hasShield = false;
                Debug.Log("Escudo de Panal destruido! Golpe bloqueado.");
                ShowDamagePopup(0f);
                return;
            }

            if (PlayerStatsRefactored.Instance != null)
            {
                amount *= PlayerStatsRefactored.Instance.itemDamageTakenMultiplier;
            }

            ShowDamagePopup(amount);
            currentHealth -= amount;
            Debug.Log($"Jugador recibi {amount} de dao! Vida restante: {currentHealth}");

            NotifyObservers();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            NotifyObservers();
            Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
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
                NotifyObservers();
            }
        }

        private void Die()
        {
            Debug.Log("EL JUGADOR HA MUERTO!");

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("PlayerDeath");
            }

            Destroy(gameObject);
        }
    }
}

