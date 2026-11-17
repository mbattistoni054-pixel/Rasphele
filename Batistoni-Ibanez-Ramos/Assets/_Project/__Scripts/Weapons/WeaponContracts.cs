using System;

namespace PatronesAplicados.RealImplementation
{
    [Serializable]
    public struct WeaponRuntimeStats
    {
        public float baseDamage;
        public float baseCooldown;
        public float range;
        public float rangeMultiplier;
        public float projectileSpeed;
        public int multipleShots;
        public int chainBounces;
        public float explosiveRadius;
        public float duration;
        public float electricStormChance;
        public float heatHeal;
        public float maxDamageCap;
        public float rampUpInterval;
        public DamageType damageType;
        public WeaponData.ImpactEffects effects;

        public static WeaponRuntimeStats FromData(WeaponData data)
        {
            if (data == null) return default;

            return new WeaponRuntimeStats
            {
                baseDamage = data.baseDamage,
                baseCooldown = data.baseCooldown,
                range = data.baseRange,
                rangeMultiplier = 1f,
                projectileSpeed = data.baseProjectileSpeed > 0f ? data.baseProjectileSpeed : 20f,
                multipleShots = data.multipleShots,
                chainBounces = data.chainBounces,
                explosiveRadius = data.explosiveRadius,
                duration = data.baseDuration,
                electricStormChance = data.electricStormChance,
                heatHeal = 0f,
                maxDamageCap = 200f,
                rampUpInterval = 1f,
                damageType = data.damageType,
                effects = data.baseEffects
            };
        }

        public void Validate()
        {
            if (baseCooldown < 0.1f) baseCooldown = 0.1f;
            if (baseDamage < 1f) baseDamage = 1f;
            if (rangeMultiplier < 0.1f) rangeMultiplier = 0.1f;
            if (projectileSpeed < 1f) projectileSpeed = 1f;
        }
    }
}