using UnityEngine;
using System.Collections.Generic;

public struct UpgradeOption
{
    public UpgradeData data;
    public WeaponBase weapon;
}

public class WeaponUpgradeProfile
{
    public Dictionary<UpgradeData, int> levels = new Dictionary<UpgradeData, int>();
    public List<Sprite> firingIcons = new List<Sprite>();
    public List<Sprite> impactIcons = new List<Sprite>();
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Base de Datos")]
    public List<UpgradeData> allAvailableUpgrades;


    private Dictionary<WeaponBase, WeaponUpgradeProfile> weaponProfiles = new Dictionary<WeaponBase, WeaponUpgradeProfile>();

    public Dictionary<WeaponBase, int> weaponPurgeCounts = new Dictionary<WeaponBase, int>();
    public Dictionary<WeaponBase, UpgradeData> blacklistedUpgrades = new Dictionary<WeaponBase, UpgradeData>();

    //public List<UpgradeData> choosenUpgrades = new List<UpgradeData>();
    public Dictionary<UpgradeData, int> choosenUpgrades = new Dictionary<UpgradeData, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public WeaponUpgradeProfile GetProfile(WeaponBase weapon)
    {
        if (!weaponProfiles.ContainsKey(weapon))
        {
            weaponProfiles[weapon] = new WeaponUpgradeProfile();
        }
        return weaponProfiles[weapon];
    }

    public List<UpgradeOption> GetGlobalRandomUpgrades(int amount, WeaponBase[] activeWeapons)
    {
        List<UpgradeOption> validOptions = new List<UpgradeOption>();
        bool healRolled = false;

        foreach (WeaponBase weapon in activeWeapons)
        {
            WeaponUpgradeProfile profile = GetProfile(weapon);

            foreach (var upgrade in allAvailableUpgrades)
            {
                if (blacklistedUpgrades.ContainsKey(weapon) && blacklistedUpgrades[weapon] == upgrade)
                {
                    continue;
                }

                if (upgrade.type == UpgradeType.Heal)
                {
                    if (!healRolled)
                    {
                        if (Random.Range(0f, 100f) <= 15f)
                            validOptions.Add(new UpgradeOption { data = upgrade, weapon = weapon });

                        healRolled = true;
                    }
                    continue;
                }

                if (upgrade.exclusiveWeapon != null && weapon.data != null)
                {
                    if (upgrade.exclusiveWeapon != weapon.data) continue;
                }

                int currentLevel = profile.levels.ContainsKey(upgrade) ? profile.levels[upgrade] : 0;
                if (currentLevel >= upgrade.levelValues.Length) continue;

                if (currentLevel == 0)
                {

                    if (upgrade.category == UpgradeCategory.Firing && profile.firingIcons.Count >= weapon.data.maxFiringUpgrades) continue;
                    if (upgrade.category == UpgradeCategory.Impact && profile.impactIcons.Count >= weapon.data.maxImpactUpgrades) continue;
                }

                validOptions.Add(new UpgradeOption { data = upgrade, weapon = weapon });
            }
        }

        List<UpgradeOption> chosen = new List<UpgradeOption>();
        for (int i = 0; i < amount; i++)
        {
            if (validOptions.Count == 0) break;

            int rand = Random.Range(0, validOptions.Count);
            chosen.Add(validOptions[rand]);
            validOptions.RemoveAt(rand);
        }

        return chosen;
    }

    public void ApplyUpgrade(UpgradeData upgrade, WeaponBase weapon)
    {
        if (upgrade.type == UpgradeType.Heal)
        {
            PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null) playerHealth.Heal(upgrade.levelValues[0]);
            return;
        }

        WeaponUpgradeProfile profile = GetProfile(weapon);

        if (!profile.levels.ContainsKey(upgrade))
        {
            profile.levels[upgrade] = 0;
            if (upgrade.category == UpgradeCategory.Firing) profile.firingIcons.Add(upgrade.icon);
            else if (upgrade.category == UpgradeCategory.Impact) profile.impactIcons.Add(upgrade.icon);
            choosenUpgrades.Add(upgrade, 0);
        }

        int newLevel = profile.levels[upgrade] + 1;
        profile.levels[upgrade] = newLevel;

        choosenUpgrades[upgrade] += 1;

        float valueToApply = upgrade.levelValues[newLevel - 1];

        switch (upgrade.type)
        {
            case UpgradeType.Explosive: weapon.SetExplosiveRadius(weapon.CurrentExplosiveRadius + valueToApply); break;
            case UpgradeType.Chain: weapon.SetChainBounces(weapon.CurrentChainBounces + (int)valueToApply); break;
            case UpgradeType.Multiple: weapon.SetMultipleShots(weapon.CurrentMultipleShots + (int)valueToApply); break;
            case UpgradeType.FireRate: weapon.SetCooldown(valueToApply); break; 
            case UpgradeType.Range: weapon.SetRange(weapon.CurrentRange + valueToApply); break;
            case UpgradeType.Damage: weapon.SetDamage(weapon.CurrentBaseDamage + valueToApply); break;

            case UpgradeType.Bleed: weapon.SetBleedPercent(weapon.CurrentEffects.bleedPercent + valueToApply); break;
            case UpgradeType.Burn: weapon.SetBurnDamage(weapon.CurrentEffects.burnDamage + valueToApply); break;
            case UpgradeType.Poison: weapon.SetPoisonDamage(weapon.CurrentEffects.poisonDamage + valueToApply); break;
            case UpgradeType.Freeze: weapon.SetFreezePercent(weapon.CurrentEffects.freezePercent + valueToApply); break;
            case UpgradeType.Stun: weapon.SetStunChance(weapon.CurrentEffects.stunChance + valueToApply); break;
            case UpgradeType.Crit: weapon.SetCritChance(weapon.CurrentEffects.critChance + valueToApply); break;

            case UpgradeType.Duration: weapon.SetDuration(weapon.CurrentDuration + valueToApply); break;
            case UpgradeType.ElectricStorm: weapon.SetElectricStormChance(weapon.CurrentElectricStormChance + valueToApply); break;

            case UpgradeType.HeatHeal:
                weapon.SetHeatHeal(weapon.CurrentHeatHeal + valueToApply);
                Debug.Log($" [UPGRADE MANAGER] Se inyect� +{valueToApply} de curaci�n al arma {weapon.gameObject.name}. Total: {weapon.CurrentHeatHeal}");
                break;

            case UpgradeType.PlayerSpeed:
                if (PlayerStats.Instance != null) PlayerStats.Instance.AddGlobalSpeed(valueToApply); break;
            case UpgradeType.PlayerSpeedFlat:
                if (PlayerStats.Instance != null) PlayerStats.Instance.AddWeaponSpeedFlat(valueToApply); break;

            case UpgradeType.CometMode: if (weapon is MeteorWeapon meteorCometa) meteorCometa.isCometMode = true; break;
            case UpgradeType.ScatterMode: if (weapon is MeteorWeapon meteorDisperso) meteorDisperso.scatterAmount = (int)valueToApply; break;

            case UpgradeType.RangeMultiplier: weapon.SetRangeMultiplier(valueToApply); break; // Reemplaza
            case UpgradeType.MaxDamageCap: if (weapon is GalvanicCoreWeapon gcCap1) gcCap1.maxDamageCap = valueToApply; break; // Reemplaza
            case UpgradeType.RampUpInterval: if (weapon is GalvanicCoreWeapon gcSpd1) gcSpd1.rampUpInterval = valueToApply; break; // Reemplaza

            case UpgradeType.MagicMode: if (weapon is ShurikenWeapon shuriMagico) shuriMagico.isMagicMode = true; break;
            case UpgradeType.ProjectileSpeed: weapon.SetProjectileSpeed(weapon.CurrentProjectileSpeed + valueToApply); break;
        }
    }

    public int GetPurgeCost(WeaponBase weapon)
    {
        if (!weaponPurgeCounts.ContainsKey(weapon)) weaponPurgeCounts[weapon] = 0;
        return 200 * (weaponPurgeCounts[weapon] + 1);
    }

    public void PurgeUpgrade(WeaponBase weapon, UpgradeData upgradeToPurge)
    {
        WeaponUpgradeProfile profile = GetProfile(weapon);

        if (profile.levels.ContainsKey(upgradeToPurge))
        {
            if (!weaponPurgeCounts.ContainsKey(weapon)) weaponPurgeCounts[weapon] = 0;
            weaponPurgeCounts[weapon]++;

            blacklistedUpgrades[weapon] = upgradeToPurge;

            profile.levels.Remove(upgradeToPurge);
            profile.firingIcons.RemoveAll(icon => icon == upgradeToPurge.icon);
            profile.impactIcons.RemoveAll(icon => icon == upgradeToPurge.icon);

            RecalculateWeaponStats(weapon, profile);
        }
    }

    private void RecalculateWeaponStats(WeaponBase weapon, WeaponUpgradeProfile profile)
    {
        weapon.ResetWeaponStats();

        foreach (var kvp in profile.levels)
        {
            UpgradeData uData = kvp.Key;
            int levelReached = kvp.Value;

            for (int i = 0; i < levelReached; i++)
            {
                float val = uData.levelValues[i];

                switch (uData.type)
                {
                    case UpgradeType.Explosive: weapon.SetExplosiveRadius(weapon.CurrentExplosiveRadius + val); break;
                    case UpgradeType.Chain: weapon.SetChainBounces(weapon.CurrentChainBounces + (int)val); break;
                    case UpgradeType.Multiple: weapon.SetMultipleShots(weapon.CurrentMultipleShots + (int)val); break;
                    case UpgradeType.FireRate: weapon.SetCooldown(val); break;
                    case UpgradeType.Range: weapon.SetRange(weapon.CurrentRange + val); break;
                    case UpgradeType.Damage: weapon.SetDamage(weapon.CurrentBaseDamage + val); break;

                    case UpgradeType.Bleed: weapon.SetBleedPercent(weapon.CurrentEffects.bleedPercent + val); break;
                    case UpgradeType.Burn: weapon.SetBurnDamage(weapon.CurrentEffects.burnDamage + val); break;
                    case UpgradeType.Poison: weapon.SetPoisonDamage(weapon.CurrentEffects.poisonDamage + val); break;
                    case UpgradeType.Freeze: weapon.SetFreezePercent(weapon.CurrentEffects.freezePercent + val); break;
                    case UpgradeType.Stun: weapon.SetStunChance(weapon.CurrentEffects.stunChance + val); break;
                    case UpgradeType.Crit: weapon.SetCritChance(weapon.CurrentEffects.critChance + val); break;

                    case UpgradeType.Duration: weapon.SetDuration(weapon.CurrentDuration + val); break;
                    case UpgradeType.ElectricStorm: weapon.SetElectricStormChance(weapon.CurrentElectricStormChance + val); break;
                    case UpgradeType.HeatHeal: weapon.SetHeatHeal(weapon.CurrentHeatHeal + val); break;

                    case UpgradeType.CometMode: if (weapon is MeteorWeapon meteorCometa) meteorCometa.isCometMode = true; break;
                    case UpgradeType.ScatterMode: if (weapon is MeteorWeapon meteorDisperso) meteorDisperso.scatterAmount = (int)val; break;

                    case UpgradeType.RangeMultiplier: weapon.SetRangeMultiplier(val); break;
                    case UpgradeType.MaxDamageCap: if (weapon is GalvanicCoreWeapon gcCap2) gcCap2.maxDamageCap = val; break;
                    case UpgradeType.RampUpInterval: if (weapon is GalvanicCoreWeapon gcSpd2) gcSpd2.rampUpInterval = val; break;

                    case UpgradeType.MagicMode: if (weapon is ShurikenWeapon shuriMagico2) shuriMagico2.isMagicMode = true; break;
                    case UpgradeType.ProjectileSpeed: weapon.SetProjectileSpeed(weapon.CurrentProjectileSpeed + val); break;
                }
            }
        }
    }
}