using System;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public abstract class WeaponBaseRealRefactored : MonoBehaviour, IPausable
    {
        [Header("Datos Base (Read-Only)")]
        public WeaponData data;

        protected WeaponRuntimeStats runtimeStats;
        protected float currentCooldownTimer;
        protected bool attackSuccessful;
        protected bool isPaused;

        private bool _isStatsInitialized;
        private int _cachedWeaponId;
        private Action<PlayerStatsRefactored> _cachedStatsCallback;

        protected float cachedGlobalDamageMult = 1f;
        protected float cachedGlobalFireRateMult = 1f;
        protected float cachedGlobalRangeMult = 1f;

        public int WeaponID => _cachedWeaponId;
        public ref WeaponRuntimeStats Stats => ref runtimeStats;

        public float CurrentBaseDamage { get => runtimeStats.baseDamage; set => runtimeStats.baseDamage = value; }
        public float CurrentBaseCooldown { get => runtimeStats.baseCooldown; set => runtimeStats.baseCooldown = value; }
        public int CurrentMultipleShots { get => runtimeStats.multipleShots; set => runtimeStats.multipleShots = value; }
        public int CurrentChainBounces { get => runtimeStats.chainBounces; set => runtimeStats.chainBounces = value; }
        public float CurrentExplosiveRadius { get => runtimeStats.explosiveRadius; set => runtimeStats.explosiveRadius = value; }
        public WeaponData.ImpactEffects CurrentEffects { get => runtimeStats.effects; set => runtimeStats.effects = value; }
        public float CurrentRange { get => runtimeStats.range; set => runtimeStats.range = value; }
        public float CurrentDuration { get => runtimeStats.duration; set => runtimeStats.duration = value; }
        public float CurrentElectricStormChance { get => runtimeStats.electricStormChance; set => runtimeStats.electricStormChance = value; }
        public float CurrentHeatHeal { get => runtimeStats.heatHeal; set => runtimeStats.heatHeal = value; }
        public float CurrentMaxDamageCap { get => runtimeStats.maxDamageCap; set => runtimeStats.maxDamageCap = value; }
        public float CurrentRampUpInterval { get => runtimeStats.rampUpInterval; set => runtimeStats.rampUpInterval = value; }
        public float CurrentRangeMultiplier { get => runtimeStats.rangeMultiplier; set => runtimeStats.rangeMultiplier = value; }
        public float CurrentProjectileSpeed { get => runtimeStats.projectileSpeed; set => runtimeStats.projectileSpeed = value; }
        public DamageType CurrentDamageType { get => runtimeStats.damageType; set => runtimeStats.damageType = value; }

        protected virtual void Awake()
        {
            _cachedWeaponId = gameObject.GetInstanceID();
            _cachedStatsCallback = OnReceiveStats;
            EnsureInitialized();
        }

        protected virtual void OnEnable()
        {
            isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        protected virtual void Start()
        {
            EnsureInitialized();

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.RequestCurrentStats, _cachedStatsCallback);
                EventManager.Instance.StartListening(GameEventKeys.PlayerStatsUpdated, RequestStatsUpdate);
            }
        }

        protected virtual void OnDestroy()
        {
            PauseManager.Unregister(this);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.PlayerStatsUpdated, RequestStatsUpdate);
            }
        }

        public void EnsureInitialized()
        {
            if (_cachedWeaponId == 0) _cachedWeaponId = gameObject.GetInstanceID();
            if (_cachedStatsCallback == null) _cachedStatsCallback = OnReceiveStats;

            if (!_isStatsInitialized && data != null)
            {
                ResetWeaponStats();
            }
        }

        public void ApplyUpgrade(IWeaponUpgrade upgrade)
        {
            if (upgrade == null) return;
            EnsureInitialized();
            upgrade.Apply(ref runtimeStats);
            runtimeStats.Validate();
        }

        private void RequestStatsUpdate()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.RequestCurrentStats, _cachedStatsCallback);
            }
        }

        private void OnReceiveStats(PlayerStatsRefactored stats)
        {
            if (stats == null) return;
            cachedGlobalDamageMult = stats.globalDamageMultiplier;
            cachedGlobalFireRateMult = stats.globalFireRateMultiplier;
            cachedGlobalRangeMult = stats.globalRangeMultiplier;
        }

        protected virtual void Update()
        {
            if (data == null || isPaused) return;

            currentCooldownTimer += Time.deltaTime;
            float actualCooldown = runtimeStats.baseCooldown * cachedGlobalFireRateMult;

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

        protected float GetFinalDamage()
        {
            return data != null ? runtimeStats.baseDamage * cachedGlobalDamageMult : 0f;
        }

        protected float GetFinalRange()
        {
            return data != null ? runtimeStats.range * runtimeStats.rangeMultiplier * cachedGlobalRangeMult : 0f;
        }

        protected ProjectilePayload CreateProjectilePayload(Transform target, LayerMask enemyLayer)
        {
            return new ProjectilePayload(
                target,
                GetFinalDamage(),
                runtimeStats.projectileSpeed,
                runtimeStats.chainBounces,
                runtimeStats.explosiveRadius,
                enemyLayer,
                runtimeStats.effects,
                WeaponID,
                runtimeStats.damageType
            );
        }

        public virtual void ResetWeaponStats()
        {
            if (data == null) return;
            runtimeStats = WeaponRuntimeStats.FromData(data);
            _isStatsInitialized = true;
        }

        public virtual void OnPause() => isPaused = true;
        public virtual void OnResume() => isPaused = false;
    }
}