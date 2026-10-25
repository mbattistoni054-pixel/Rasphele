using System;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    [Serializable]
    public class WeaponStatModifier : IWeaponUpgrade
    {
        [Header("Modificadores Aditivos de Estadísticas")]
        public float addDamage;
        public float addRange;
        public int addMultipleShots;
        public int addChainBounces;
        public float addExplosiveRadius;
        public float addDuration;
        public float addElectricStormChance;
        public float addHeatHeal;
        public float addProjectileSpeed;

        [Header("Modificadores de Reemplazo (Opcionales)")]
        public bool overrideCooldown;
        public float newCooldown = 1f;

        public bool overrideMaxDamageCap;
        public float newMaxDamageCap = 200f;

        public bool overrideRampUpInterval;
        public float newRampUpInterval = 1f;

        public bool overrideRangeMultiplier;
        public float newRangeMultiplier = 1f;

        [Header("Modificadores de Efectos de Impacto")]
        public WeaponData.ImpactEffects addEffects;


        public void Apply(ref WeaponRuntimeStats stats)
        {
            stats.baseDamage += addDamage;
            if (overrideCooldown) stats.baseCooldown = newCooldown;

            stats.range += addRange;
            stats.multipleShots += addMultipleShots;
            stats.chainBounces += addChainBounces;
            stats.explosiveRadius += addExplosiveRadius;
            stats.duration += addDuration;
            stats.electricStormChance += addElectricStormChance;
            stats.heatHeal += addHeatHeal;
            stats.projectileSpeed += addProjectileSpeed;

            if (overrideMaxDamageCap) stats.maxDamageCap = newMaxDamageCap;
            if (overrideRampUpInterval) stats.rampUpInterval = newRampUpInterval;
            if (overrideRangeMultiplier) stats.rangeMultiplier = newRangeMultiplier;

            stats.effects.bleedPercent += addEffects.bleedPercent;
            stats.effects.burnDamage += addEffects.burnDamage;
            stats.effects.poisonDamage += addEffects.poisonDamage;
            stats.effects.freezePercent += addEffects.freezePercent;
            stats.effects.stunChance += addEffects.stunChance;
            stats.effects.critChance += addEffects.critChance;

            stats.Validate();
        }

        public WeaponStatModifier WithDamage(float amount) { addDamage += amount; return this; }
        public WeaponStatModifier WithCooldown(float amount) { overrideCooldown = true; newCooldown = amount; return this; }
        public WeaponStatModifier WithRange(float amount) { addRange += amount; return this; }
        public WeaponStatModifier WithMultipleShots(int amount) { addMultipleShots += amount; return this; }
        public WeaponStatModifier WithChainBounces(int amount) { addChainBounces += amount; return this; }
        public WeaponStatModifier WithExplosiveRadius(float amount) { addExplosiveRadius += amount; return this; }
        public WeaponStatModifier WithDuration(float amount) { addDuration += amount; return this; }
        public WeaponStatModifier WithElectricStormChance(float amount) { addElectricStormChance += amount; return this; }
        public WeaponStatModifier WithHeatHeal(float amount) { addHeatHeal += amount; return this; }
        public WeaponStatModifier WithMaxDamageCap(float amount) { overrideMaxDamageCap = true; newMaxDamageCap = amount; return this; }
        public WeaponStatModifier WithRampUpInterval(float amount) { overrideRampUpInterval = true; newRampUpInterval = amount; return this; }
        public WeaponStatModifier WithRangeMultiplier(float amount) { overrideRangeMultiplier = true; newRangeMultiplier = amount; return this; }
        public WeaponStatModifier WithProjectileSpeed(float amount) { addProjectileSpeed += amount; return this; }
        public WeaponStatModifier WithBleedPercent(float amount) { addEffects.bleedPercent += amount; return this; }
        public WeaponStatModifier WithBurnDamage(float amount) { addEffects.burnDamage += amount; return this; }
        public WeaponStatModifier WithPoisonDamage(float amount) { addEffects.poisonDamage += amount; return this; }
        public WeaponStatModifier WithFreezePercent(float amount) { addEffects.freezePercent += amount; return this; }
        public WeaponStatModifier WithStunChance(float amount) { addEffects.stunChance += amount; return this; }
        public WeaponStatModifier WithCritChance(float amount) { addEffects.critChance += amount; return this; }
    }
}