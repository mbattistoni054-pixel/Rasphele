using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEngine.Audio; // Necesario para el Action

public class PlayerHealth : MonoBehaviour
{
    [Header("Estadísticas de Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI y Efectos")]
    public GameObject damagePopupPrefab;

    [Header("Audio")]
    public AudioClip hurtSound;

    // Variables internas
    private bool hasShield = false;
    private float timeSinceLastDamage = 0f;


    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public static event HealthChangedHandler OnHealthChanged;

    public static event Action OnPlayerDeath;

    private Volume volume;
    private Vignette vignette;

    void Start()
    {
        // Leemos el total calculado desde PlayerStats (Vida Base + Objetos)
        if (PlayerStats.Instance != null && PlayerStats.Instance.baseMaxHealth > 0)
        {
            maxHealth = PlayerStats.Instance.GetTotalMaxHealth();
        }

        currentHealth = maxHealth;

        // Le avisamos a quien esté escuchando (El HUD) en el segundo 0.1
        Invoke(nameof(BroadcastHealthUpdate), 0.1f);
    }

    void Update()
    {
        // Monitoreo de inactividad para activar el escudo (Panal)
        if (PlayerStats.Instance != null && PlayerStats.Instance.shieldStacks > 0)
        {
            if (!hasShield)
            {
                timeSinceLastDamage += Time.deltaTime;
                if (timeSinceLastDamage >= 10f)
                {
                    hasShield = true;
                    Debug.Log(" ¡Escudo de Panal generado! Bloqueará el próximo golpe.");
                }
            }
        }

        float value = Mathf.Clamp01(currentHealth / maxHealth);

        float actualValue = 0.5f - value;

        volume = GameManager.Instance.globalVolume.GetComponent<Volume>();

        AudioMixer audio = AudioManager.Instance.mainMixer;

        if (volume != null && volume.profile.TryGet(out vignette))
        {

            vignette.intensity.value = actualValue * 1.5f;

             
            vignette.intensity.overrideState = true;
        }

        if (currentHealth < maxHealth / 2)
        {
           // float newPitch = Mathf.Lerp(0.6f, 1f, value);

            float newPitch = 0.5f + value;

            audio.SetFloat("MasterPitch", newPitch);
        }else
        {
            audio.SetFloat("MasterPitch", 1);
        }
      

    }

    public void UpdateMaxHealthFromStats()
    {
        if (PlayerStats.Instance == null) return;

        float oldMaxHealth = maxHealth;
        maxHealth = PlayerStats.Instance.GetTotalMaxHealth();

        // Le regalamos al jugador la vida máxima extra para que no se quede herido
        float difference = maxHealth - oldMaxHealth;
        if (difference > 0)
        {
            currentHealth += difference;
        }

        if (currentHealth > maxHealth) currentHealth = maxHealth;

        BroadcastHealthUpdate();
    }

    private void BroadcastHealthUpdate()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
                // Le decimos que es daño al jugador para que lo pinte de Rojo Peligro
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
            Debug.Log("¡Escudo de Panal destruido! Golpe bloqueado.");
            ShowDamagePopup(0f);
            return;
        }

        if (PlayerStats.Instance != null)
        {
            amount *= PlayerStats.Instance.itemDamageTakenMultiplier;
        }

        ShowDamagePopup(amount);
        currentHealth -= amount;
        Debug.Log($"¡Jugador recibió {amount} de daño! Vida restante: {currentHealth}");

        // Avisamos a la UI
        BroadcastHealthUpdate();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        BroadcastHealthUpdate();
        Debug.Log($"Jugador curado. Vida actual: {currentHealth}");
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.baseMaxHealth += amount;
            UpdateMaxHealthFromStats();
        }
        else
        {
            maxHealth += amount;
            currentHealth += amount;
            BroadcastHealthUpdate();
        }
    }

    private void Die()
    {
        Debug.Log("¡EL JUGADOR HA MUERTO!");


        OnPlayerDeath?.Invoke();

        Destroy(gameObject);
    }
}