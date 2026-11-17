using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public static class WeaponUpgradeApplier
    {
        private static Transform _cachedPlayerTransform;
        private static PlayerHealthRefactored _cachedPlayerHealth;

        public static void ApplyStatModification(UpgradeData data, WeaponBaseRealRefactored weapon, float valueToApply, bool isRecalculation = false)
        {
            if (data == null) return;

            if (data.type == UpgradeType.Heal)
            {
                if (!isRecalculation)
                {
                    HealPlayer(valueToApply);
                }
                return;
            }

            if (weapon == null) return;

            weapon.EnsureInitialized();
            ref WeaponRuntimeStats stats = ref weapon.Stats;

            switch (data.type)
            {
                case UpgradeType.Explosive:
                    stats.explosiveRadius += valueToApply;
                    break;
                case UpgradeType.Chain:
                    stats.chainBounces += Mathf.RoundToInt(valueToApply);
                    break;
                case UpgradeType.Multiple:
                    stats.multipleShots += Mathf.RoundToInt(valueToApply);
                    break;
                case UpgradeType.FireRate:
                    stats.baseCooldown = valueToApply;
                    break;
                case UpgradeType.Range:
                    stats.range += valueToApply;
                    break;
                case UpgradeType.Damage:
                    stats.baseDamage += valueToApply;
                    break;
                case UpgradeType.Bleed:
                    stats.effects.bleedPercent += valueToApply;
                    break;
                case UpgradeType.Burn:
                    stats.effects.burnDamage += valueToApply;
                    break;
                case UpgradeType.Poison:
                    stats.effects.poisonDamage += valueToApply;
                    break;
                case UpgradeType.Freeze:
                    stats.effects.freezePercent += valueToApply;
                    break;
                case UpgradeType.Stun:
                    stats.effects.stunChance += valueToApply;
                    break;
                case UpgradeType.Crit:
                    stats.effects.critChance += valueToApply;
                    break;
                case UpgradeType.Duration:
                    stats.duration += valueToApply;
                    break;
                case UpgradeType.ElectricStorm:
                    stats.electricStormChance += valueToApply;
                    break;
                case UpgradeType.HeatHeal:
                    stats.heatHeal += valueToApply;
                    break;
                case UpgradeType.RangeMultiplier:
                    stats.rangeMultiplier = valueToApply;
                    break;
                case UpgradeType.MaxDamageCap:
                    stats.maxDamageCap = valueToApply;
                    break;
                case UpgradeType.RampUpInterval:
                    stats.rampUpInterval = valueToApply;
                    break;
                case UpgradeType.ProjectileSpeed:
                    stats.projectileSpeed += valueToApply;
                    break;

                case UpgradeType.PlayerSpeed:
                case UpgradeType.PlayerSpeedFlat:
                    if (!isRecalculation && EventManager.Instance != null)
                    {
                        string evt = data.type == UpgradeType.PlayerSpeed
                            ? GameEventKeys.AddGlobalSpeed
                            : GameEventKeys.AddWeaponSpeedFlat;
                        EventManager.Instance.TriggerEvent(evt, valueToApply);
                    }
                    break;

                case UpgradeType.CometMode:
                    if (weapon is MeteorWeaponRefactored cometMeteor)
                    {
                        cometMeteor.isCometMode = true;
                    }
                    break;

                case UpgradeType.ScatterMode:
                    if (weapon is MeteorWeaponRefactored scatterMeteor)
                    {
                        scatterMeteor.scatterAmount = Mathf.Max(2, Mathf.RoundToInt(valueToApply));
                    }
                    break;

                case UpgradeType.MagicMode:
                    if (weapon is ShurikenWeaponRefactored shuriMagico)
                    {
                        shuriMagico.isMagicMode = true;
                    }
                    break;
            }

            stats.Validate();
        }

        private static void HealPlayer(float amount)
        {
            Transform currentTransform = GameManagerRefactored.Instance != null
                ? GameManagerRefactored.Instance.Player
                : null;

            if (currentTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) currentTransform = p.transform;
            }

            if (currentTransform == null) return;

            if (_cachedPlayerTransform != currentTransform || _cachedPlayerHealth == null)
            {
                _cachedPlayerTransform = currentTransform;
                currentTransform.TryGetComponent(out _cachedPlayerHealth);
            }

            if (_cachedPlayerHealth != null)
            {
                _cachedPlayerHealth.Heal(amount);
            }
        }
    }
}