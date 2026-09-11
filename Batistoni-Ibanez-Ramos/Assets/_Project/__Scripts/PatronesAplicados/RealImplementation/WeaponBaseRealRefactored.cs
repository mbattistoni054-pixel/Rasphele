using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// WeaponBaseRealRefactored: Adaptacin real de tu WeaponBase.
    /// Exponemos las variables como Propiedades con setter pblico para que el Builder
    /// pueda modificarlas, manteniendo la lectura pblica.
    /// </summary>
    public abstract class WeaponBaseRealRefactored : MonoBehaviour
    {
        [Header("Datos Base (Read-Only)")]
        public WeaponData data;

        protected float currentCooldownTimer;
        protected bool attackSuccessful = false;

        // --- PROPIEDADES (Para el Builder) ---
        public float CurrentBaseDamage { get; set; }
        public float CurrentBaseCooldown { get; set; }
        public int CurrentMultipleShots { get; set; }
        public int CurrentChainBounces { get; set; }
        public float CurrentExplosiveRadius { get; set; }
        public WeaponData.ImpactEffects CurrentEffects { get; set; }
        public float CurrentRange { get; set; }
        
        public float CurrentDuration { get; set; }
        public float CurrentElectricStormChance { get; set; }
        public float CurrentHeatHeal { get; set; }
        
        // Atributos Especficos (Ej. Ncleo Galvnico)
        public float CurrentMaxDamageCap { get; set; } = 200f;
        public float CurrentRampUpInterval { get; set; } = 1f;

        public float CurrentRangeMultiplier { get; set; } = 1f;
        public float CurrentProjectileSpeed { get; set; } = 20f;

        public DamageType CurrentDamageType { get; set; }
        public int WeaponID => gameObject.GetInstanceID();

        // --- STATS CACHE (Desacoplamiento de PlayerStats) ---
        protected float cachedGlobalDamageMult = 1f;
        protected float cachedGlobalFireRateMult = 1f;
        protected float cachedGlobalRangeMult = 1f;

        protected virtual void Start()
        {
            if (data != null) ResetWeaponStats();

            // DESACOPLAMIENTO: Pedimos los stats iniciales y nos suscribimos a los cambios
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent<System.Action<PatronesAplicados.PlayerStatsRefactored>>("RequestCurrentStats", OnReceiveStats);
                EventManager.Instance.StartListening("PlayerStatsUpdated", RequestStatsUpdate);
            }
        }

        protected virtual void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening("PlayerStatsUpdated", RequestStatsUpdate);
            }
        }

        private void RequestStatsUpdate()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent<System.Action<PatronesAplicados.PlayerStatsRefactored>>("RequestCurrentStats", OnReceiveStats);
            }
        }

        private void OnReceiveStats(PatronesAplicados.PlayerStatsRefactored stats)
        {
            if (stats == null) return;
            cachedGlobalDamageMult = stats.globalDamageMultiplier;
            cachedGlobalFireRateMult = stats.globalFireRateMultiplier;
            cachedGlobalRangeMult = stats.globalRangeMultiplier;
        }

        protected virtual void Update()
        {
            if (data == null) return;

            currentCooldownTimer += Time.deltaTime;

            float actualCooldown = CurrentBaseCooldown * cachedGlobalFireRateMult;

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
            }
        }

        protected abstract void Attack();

        // Mtodos de utilidad para calcular el valor final en el momento del disparo
        protected float GetFinalDamage()
        {
            if (data == null) return 0;
            return CurrentBaseDamage * cachedGlobalDamageMult;
        }

        protected float GetFinalRange()
        {
            if (data == null) return 0;
            return CurrentRange * CurrentRangeMultiplier * cachedGlobalRangeMult;
        }

        public virtual void ResetWeaponStats()
        {
            CurrentBaseDamage = data.baseDamage;
            CurrentBaseCooldown = data.baseCooldown;
            CurrentRange = data.baseRange;
            CurrentMultipleShots = data.multipleShots;
            CurrentChainBounces = data.chainBounces;
            CurrentExplosiveRadius = data.explosiveRadius;

            CurrentDuration = data.baseDuration;
            CurrentElectricStormChance = data.electricStormChance;
            CurrentHeatHeal = 0f;

            CurrentEffects = new WeaponData.ImpactEffects
            {
                bleedPercent = data.baseEffects.bleedPercent,
                burnDamage = data.baseEffects.burnDamage,
                poisonDamage = data.baseEffects.poisonDamage,
                freezePercent = data.baseEffects.freezePercent,
                stunChance = data.baseEffects.stunChance,
                critChance = data.baseEffects.critChance
            };

            CurrentDamageType = data.damageType;
            CurrentRangeMultiplier = 1f;
            CurrentProjectileSpeed = 20f;
        }
    }
}
