using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Datos Base (Read-Only)")]
    public WeaponData data;

    protected float currentCooldownTimer;

    // --- VARIABLES LOCALES (ENCAPSULAMIENTO) ---
    protected float currentBaseDamage;
    protected float currentBaseCooldown;
    protected int currentMultipleShots;
    protected int currentChainBounces;
    protected float currentExplosiveRadius;
    protected WeaponData.ImpactEffects currentEffects;
    protected float currentRange;

    // Variables Especiales
    protected float currentDuration;
    protected float currentElectricStormChance;
    protected float currentHeatHeal;

    protected DamageType currentDamageType;
    protected float currentRangeMultiplier = 1f;
    protected float currentProjectileSpeed = 20f;

    protected bool attackSuccessful = false;

    // --- GETTERS ---
    public float CurrentBaseDamage => currentBaseDamage;
    public float CurrentBaseCooldown => currentBaseCooldown;
    public int CurrentMultipleShots => currentMultipleShots;
    public int CurrentChainBounces => currentChainBounces;
    public float CurrentExplosiveRadius => currentExplosiveRadius;
    public WeaponData.ImpactEffects CurrentEffects => currentEffects;
    public float CurrentDuration => currentDuration;
    public float CurrentElectricStormChance => currentElectricStormChance;
    public float CurrentHeatHeal => currentHeatHeal;

    public float CurrentRange => currentRange;
    public float CurrentRangeMultiplier => currentRangeMultiplier;
    public float CurrentProjectileSpeed => currentProjectileSpeed;

    public int WeaponID => gameObject.GetInstanceID();
    public DamageType DamageType => currentDamageType;

    protected virtual void Start()
    {
        if (data == null) return;

        currentBaseDamage = data.baseDamage;
        currentBaseCooldown = data.baseCooldown;
        currentMultipleShots = data.multipleShots;
        currentChainBounces = data.chainBounces;
        currentExplosiveRadius = data.explosiveRadius;
        currentEffects = data.baseEffects;
        currentRange = data.baseRange;

        currentDuration = data.baseDuration;
        currentElectricStormChance = data.electricStormChance;

        currentDamageType = data.damageType;
        currentRangeMultiplier = 1f;
        currentProjectileSpeed = 20f;
    }

    protected virtual void Update()
    {
        if (data == null || PlayerStats.Instance == null) return;

        currentCooldownTimer += Time.deltaTime;

        float actualCooldown = currentBaseCooldown * PlayerStats.Instance.globalFireRateMultiplier;

        if (currentCooldownTimer >= actualCooldown)
        {
            attackSuccessful = true;
            Attack();

            if (attackSuccessful)
            {
                if (data.attackSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(data.attackSound, "WeaponAttack");
                }
                currentCooldownTimer = 0f;
            }

            currentCooldownTimer = 0f;
        }
    }

    protected abstract void Attack();

    protected float GetFinalDamage()
    {
        if (data == null || PlayerStats.Instance == null) return 0;
        return currentBaseDamage * PlayerStats.Instance.globalDamageMultiplier;
    }

    protected float GetFinalRange()
    {
        if (data == null || PlayerStats.Instance == null) return 0;
        return currentRange * currentRangeMultiplier * PlayerStats.Instance.globalRangeMultiplier;
    }

    public void SetDamage(float amount) { currentBaseDamage = amount; }
    public void SetCooldown(float amount) { currentBaseCooldown = amount; }
    public void SetMultipleShots(int amount) { currentMultipleShots = amount; }
    public void SetChainBounces(int amount) { currentChainBounces = amount; }
    public void SetExplosiveRadius(float amount) { currentExplosiveRadius = amount; }
    public void SetRange(float amount) { currentRange = amount; }

    // Setters Especiales
    public void SetDuration(float amount) { currentDuration = amount; }
    public void SetElectricStormChance(float amount) { currentElectricStormChance = amount; }
    public void SetHeatHeal(float amount) { currentHeatHeal = amount; }
    public void SetRangeMultiplier(float amount) { currentRangeMultiplier = amount; }
    public void SetProjectileSpeed(float amount) { currentProjectileSpeed = amount; }

    public void SetCritChance(float amount) { currentEffects.critChance = amount; }
    public void SetBleedPercent(float amount) { currentEffects.bleedPercent = amount; }
    public void SetBurnDamage(float amount) { currentEffects.burnDamage = amount; }
    public void SetPoisonDamage(float amount) { currentEffects.poisonDamage = amount; }
    public void SetFreezePercent(float amount) { currentEffects.freezePercent = amount; }
    public void SetStunChance(float amount) { currentEffects.stunChance = amount; }

    public virtual void ResetWeaponStats()
    {
        currentBaseDamage = data.baseDamage;
        currentBaseCooldown = data.baseCooldown;
        currentRange = data.baseRange;
        currentMultipleShots = data.multipleShots;
        currentChainBounces = data.chainBounces;
        currentExplosiveRadius = data.explosiveRadius;

        currentDuration = data.baseDuration;
        currentElectricStormChance = data.electricStormChance;
        currentHeatHeal = 0f; // ¡Faltaba esto! Vuelve a cero

        currentEffects = new WeaponData.ImpactEffects
        {
            bleedPercent = data.baseEffects.bleedPercent,
            burnDamage = data.baseEffects.burnDamage,
            poisonDamage = data.baseEffects.poisonDamage,
            freezePercent = data.baseEffects.freezePercent,
            stunChance = data.baseEffects.stunChance,
            critChance = data.baseEffects.critChance
        };

        currentDamageType = data.damageType;
        currentRangeMultiplier = 1f;
        currentProjectileSpeed = 20f;
    }
}