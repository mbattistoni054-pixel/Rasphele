using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public static class WeaponUpgradeApplier
    {
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

            switch (data.type)
            {
                case UpgradeType.Explosive:
                    weapon.CurrentExplosiveRadius += valueToApply;
                    break;
                case UpgradeType.Chain:
                    weapon.CurrentChainBounces += Mathf.RoundToInt(valueToApply);
                    break;
                case UpgradeType.Multiple:
                    weapon.CurrentMultipleShots += Mathf.RoundToInt(valueToApply);
                    break;
                case UpgradeType.FireRate:
                    weapon.CurrentBaseCooldown = valueToApply;
                    break;
                case UpgradeType.Range:
                    weapon.CurrentRange += valueToApply;
                    break;
                case UpgradeType.Damage:
                    weapon.CurrentBaseDamage += valueToApply;
                    break;

                case UpgradeType.Bleed:
                    {
                        var e = weapon.CurrentEffects;
                        e.bleedPercent += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }
                case UpgradeType.Burn:
                    {
                        var e = weapon.CurrentEffects;
                        e.burnDamage += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }
                case UpgradeType.Poison:
                    {
                        var e = weapon.CurrentEffects;
                        e.poisonDamage += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }
                case UpgradeType.Freeze:
                    {
                        var e = weapon.CurrentEffects;
                        e.freezePercent += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }
                case UpgradeType.Stun:
                    {
                        var e = weapon.CurrentEffects;
                        e.stunChance += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }
                case UpgradeType.Crit:
                    {
                        var e = weapon.CurrentEffects;
                        e.critChance += valueToApply;
                        weapon.CurrentEffects = e;
                        break;
                    }

                case UpgradeType.Duration:
                    weapon.CurrentDuration += valueToApply;
                    break;
                case UpgradeType.ElectricStorm:
                    weapon.CurrentElectricStormChance += valueToApply;
                    break;
                case UpgradeType.HeatHeal:
                    weapon.CurrentHeatHeal += valueToApply;
                    break;

                case UpgradeType.PlayerSpeed:
                case UpgradeType.PlayerSpeedFlat:
                    if (!isRecalculation && EventManager.Instance != null)
                    {
                        string evt = data.type == UpgradeType.PlayerSpeed ? "AddGlobalSpeed" : "AddWeaponSpeedFlat";
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

                case UpgradeType.RangeMultiplier:
                    weapon.CurrentRangeMultiplier = valueToApply;
                    break;
                case UpgradeType.MaxDamageCap:
                    weapon.CurrentMaxDamageCap = valueToApply;
                    break;
                case UpgradeType.RampUpInterval:
                    weapon.CurrentRampUpInterval = valueToApply;
                    break;

                case UpgradeType.MagicMode:
                    if (weapon is ShurikenWeaponRefactored shuriMagico)
                    {
                        shuriMagico.isMagicMode = true;
                    }
                    break;

                case UpgradeType.ProjectileSpeed:
                    weapon.CurrentProjectileSpeed += valueToApply;
                    break;
            }
        }

        private static void HealPlayer(float amount)
        {
            Transform playerTransform = GameManagerRefactored.Instance != null ? GameManagerRefactored.Instance.Player : null;
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }

            if (playerTransform != null)
            {
                PlayerHealthRefactored hpRef = playerTransform.GetComponent<PlayerHealthRefactored>();
                if (hpRef != null)
                {
                    hpRef.Heal(amount);
                    return;
                }

            }
        }
    }
}