using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// WeaponBuilder Real: Encapsula TODAS las modificaciones que UpgradeManager le hace a las armas.
    /// Sustituye la necesidad de tener 20 setters en WeaponBase.
    /// </summary>
    public class WeaponBuilder
    {
        private WeaponBaseRealRefactored weapon;

        public WeaponBuilder(WeaponBaseRealRefactored weapon)
        {
            this.weapon = weapon;
        }

        // --- ESTADSTICAS PRINCIPALES ---
        public WeaponBuilder AddDamage(float amount) { weapon.CurrentBaseDamage += amount; return this; }
        public WeaponBuilder SetCooldown(float amount) { weapon.CurrentBaseCooldown = amount; return this; }
        public WeaponBuilder AddRange(float amount) { weapon.CurrentRange += amount; return this; }
        public WeaponBuilder AddMultipleShots(int amount) { weapon.CurrentMultipleShots += amount; return this; }
        public WeaponBuilder AddChainBounces(int amount) { weapon.CurrentChainBounces += amount; return this; }
        public WeaponBuilder AddExplosiveRadius(float amount) { weapon.CurrentExplosiveRadius += amount; return this; }
        
        public WeaponBuilder AddDuration(float amount) { weapon.CurrentDuration += amount; return this; }
        public WeaponBuilder AddElectricStormChance(float amount) { weapon.CurrentElectricStormChance += amount; return this; }
        public WeaponBuilder AddHeatHeal(float amount) { weapon.CurrentHeatHeal += amount; return this; }
        
        // Modificadores Especficos
        public WeaponBuilder SetMaxDamageCap(float amount) { weapon.CurrentMaxDamageCap = amount; return this; }
        public WeaponBuilder SetRampUpInterval(float amount) { weapon.CurrentRampUpInterval = amount; return this; }

        public WeaponBuilder SetRangeMultiplier(float amount) { weapon.CurrentRangeMultiplier = amount; return this; }
        public WeaponBuilder AddProjectileSpeed(float amount) { weapon.CurrentProjectileSpeed += amount; return this; }

        // --- EFECTOS DE ESTADO (Struct Modification) ---
        public WeaponBuilder AddBleedPercent(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.bleedPercent += amount; weapon.CurrentEffects = eff; 
            return this; 
        }
        
        public WeaponBuilder AddBurnDamage(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.burnDamage += amount; weapon.CurrentEffects = eff; 
            return this; 
        }

        public WeaponBuilder AddPoisonDamage(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.poisonDamage += amount; weapon.CurrentEffects = eff; 
            return this; 
        }

        public WeaponBuilder AddFreezePercent(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.freezePercent += amount; weapon.CurrentEffects = eff; 
            return this; 
        }

        public WeaponBuilder AddStunChance(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.stunChance += amount; weapon.CurrentEffects = eff; 
            return this; 
        }

        public WeaponBuilder AddCritChance(float amount) 
        { 
            var eff = weapon.CurrentEffects; eff.critChance += amount; weapon.CurrentEffects = eff; 
            return this; 
        }

        /// <summary>
        /// Finaliza la construccin. El UpgradeManager llamara a esto despus de inyectar los stats.
        /// </summary>
        public WeaponBaseRealRefactored Build()
        {
            // Validaciones bsicas
            if (weapon.CurrentBaseCooldown < 0.1f) weapon.CurrentBaseCooldown = 0.1f;
            if (weapon.CurrentBaseDamage < 1f) weapon.CurrentBaseDamage = 1f;

            return weapon;
        }
    }
}
