using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public static class WeaponUpgradeApplier
    {
        private static readonly Dictionary<UpgradeType, IUpgradeStrategy> Strategies =
            new Dictionary<UpgradeType, IUpgradeStrategy>(32)
            {
                { UpgradeType.Heal, new PlayerHealUpgradeStrategy() },

                { UpgradeType.Explosive,       new WeaponStatUpgradeStrategy((m, v) => m.WithExplosiveRadius(v)) },
                { UpgradeType.Chain,           new WeaponStatUpgradeStrategy((m, v) => m.WithChainBounces(Mathf.RoundToInt(v))) },
                { UpgradeType.Multiple,        new WeaponStatUpgradeStrategy((m, v) => m.WithMultipleShots(Mathf.RoundToInt(v))) },
                { UpgradeType.FireRate,        new WeaponStatUpgradeStrategy((m, v) => m.WithCooldown(v)) },
                { UpgradeType.Range,           new WeaponStatUpgradeStrategy((m, v) => m.WithRange(v)) },
                { UpgradeType.Damage,          new WeaponStatUpgradeStrategy((m, v) => m.WithDamage(v)) },
                { UpgradeType.Bleed,           new WeaponStatUpgradeStrategy((m, v) => m.WithBleedPercent(v)) },
                { UpgradeType.Burn,            new WeaponStatUpgradeStrategy((m, v) => m.WithBurnDamage(v)) },
                { UpgradeType.Poison,          new WeaponStatUpgradeStrategy((m, v) => m.WithPoisonDamage(v)) },
                { UpgradeType.Freeze,          new WeaponStatUpgradeStrategy((m, v) => m.WithFreezePercent(v)) },
                { UpgradeType.Stun,            new WeaponStatUpgradeStrategy((m, v) => m.WithStunChance(v)) },
                { UpgradeType.Crit,            new WeaponStatUpgradeStrategy((m, v) => m.WithCritChance(v)) },
                { UpgradeType.Duration,        new WeaponStatUpgradeStrategy((m, v) => m.WithDuration(v)) },
                { UpgradeType.ElectricStorm,   new WeaponStatUpgradeStrategy((m, v) => m.WithElectricStormChance(v)) },
                { UpgradeType.HeatHeal,        new WeaponStatUpgradeStrategy((m, v) => m.WithHeatHeal(v)) },
                { UpgradeType.RangeMultiplier, new WeaponStatUpgradeStrategy((m, v) => m.WithRangeMultiplier(v)) },
                { UpgradeType.MaxDamageCap,    new WeaponStatUpgradeStrategy((m, v) => m.WithMaxDamageCap(v)) },
                { UpgradeType.RampUpInterval,  new WeaponStatUpgradeStrategy((m, v) => m.WithRampUpInterval(v)) },
                { UpgradeType.ProjectileSpeed, new WeaponStatUpgradeStrategy((m, v) => m.WithProjectileSpeed(v)) },

                { UpgradeType.PlayerSpeed,     new PlayerSpeedEventUpgradeStrategy(GameEventKeys.AddGlobalSpeed) },
                { UpgradeType.PlayerSpeedFlat, new PlayerSpeedEventUpgradeStrategy(GameEventKeys.AddWeaponSpeedFlat) },

                { UpgradeType.CometMode,   new WeaponBehaviorUpgradeStrategy<MeteorWeaponRefactored>((w, _) => w.isCometMode = true) },
                { UpgradeType.ScatterMode, new WeaponBehaviorUpgradeStrategy<MeteorWeaponRefactored>((w, v) => w.scatterAmount = Mathf.Max(2, Mathf.RoundToInt(v))) },
                { UpgradeType.MagicMode,   new WeaponBehaviorUpgradeStrategy<ShurikenWeaponRefactored>((w, _) => w.isMagicMode = true) }
            };


        public static void RegisterStrategy(UpgradeType type, IUpgradeStrategy strategy)
        {
            if (strategy == null) return;
            Strategies[type] = strategy;
        }

        public static IUpgradeStrategy ResolveStrategy(UpgradeType type)
        {
            return Strategies.TryGetValue(type, out IUpgradeStrategy strategy) ? strategy : null;
        }
        public static void ApplyUpgrade(UpgradeData data, WeaponBaseRealRefactored weapon, float valueToApply, bool isRecalculation = false)
        {
            if (data == null) return;

            IUpgradeStrategy strategy = ResolveStrategy(data.type);
            if (strategy != null)
            {
                strategy.Execute(weapon, valueToApply, isRecalculation);
            }
        }

    }
}