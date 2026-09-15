using UnityEngine;
using System.Collections.Generic;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public struct UpgradeOptionRefactored
    {
        public UpgradeData data;
        public WeaponBaseRealRefactored weapon;
    }

    public class UpgradeManagerRefactored : MonoBehaviour
    {
        [Header("Base de Datos")]
        public List<UpgradeData> allAvailableUpgrades;

        private Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile> weaponProfiles = new Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile>();
        public Dictionary<WeaponBaseRealRefactored, int> weaponPurgeCounts = new Dictionary<WeaponBaseRealRefactored, int>();
        public Dictionary<WeaponBaseRealRefactored, UpgradeData> blacklistedUpgrades = new Dictionary<WeaponBaseRealRefactored, UpgradeData>();

        public Dictionary<UpgradeData, int> choosenUpgrades = new Dictionary<UpgradeData, int>();

        private void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", OnRequestUpgradeProfile);
                EventManager.Instance.StartListening<int, WeaponBaseRealRefactored[], System.Action<List<UpgradeOptionRefactored>>>("RequestGlobalRandomUpgrades", OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StartListening<UpgradeData, WeaponBaseRealRefactored>("ApplyUpgrade", ApplyUpgrade);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, System.Action<int>>("RequestPurgeCost", OnRequestPurgeCost);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, UpgradeData>("PurgeUpgrade", PurgeUpgrade);
                EventManager.Instance.StartListening<System.Action<List<UpgradeData>>>("RequestAllAvailableUpgrades", OnRequestAllAvailableUpgrades);
                EventManager.Instance.StartListening<System.Action<Dictionary<UpgradeData, int>>>("RequestChosenUpgrades", OnRequestChosenUpgrades);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", OnRequestUpgradeProfile);
                EventManager.Instance.StopListening<int, WeaponBaseRealRefactored[], System.Action<List<UpgradeOptionRefactored>>>("RequestGlobalRandomUpgrades", OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StopListening<UpgradeData, WeaponBaseRealRefactored>("ApplyUpgrade", ApplyUpgrade);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, System.Action<int>>("RequestPurgeCost", OnRequestPurgeCost);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, UpgradeData>("PurgeUpgrade", PurgeUpgrade);
                EventManager.Instance.StopListening<System.Action<List<UpgradeData>>>("RequestAllAvailableUpgrades", OnRequestAllAvailableUpgrades);
                EventManager.Instance.StopListening<System.Action<Dictionary<UpgradeData, int>>>("RequestChosenUpgrades", OnRequestChosenUpgrades);
            }
        }

        private void OnRequestUpgradeProfile(WeaponBaseRealRefactored weapon, System.Action<WeaponUpgradeProfile> callback)
        {
            callback?.Invoke(GetProfile(weapon));
        }

        private void OnRequestGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons, System.Action<List<UpgradeOptionRefactored>> callback)
        {

            callback?.Invoke(GetGlobalRandomUpgrades(amount, activeWeapons));
        }

        private void OnRequestPurgeCost(WeaponBaseRealRefactored weapon, System.Action<int> callback)
        {
            callback?.Invoke(GetPurgeCost(weapon));
        }

        private void OnRequestAllAvailableUpgrades(System.Action<List<UpgradeData>> callback)
        {
            callback?.Invoke(allAvailableUpgrades);
        }

        private void OnRequestChosenUpgrades(System.Action<Dictionary<UpgradeData, int>> callback)
        {
            callback?.Invoke(choosenUpgrades);
        }

        public WeaponUpgradeProfile GetProfile(WeaponBaseRealRefactored weapon)
        {
            if (!weaponProfiles.ContainsKey(weapon))
            {
                weaponProfiles[weapon] = new WeaponUpgradeProfile();
            }
            return weaponProfiles[weapon];
        }

        public List<UpgradeOptionRefactored> GetGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons)
        {

            List<UpgradeOptionRefactored> validOptions = new List<UpgradeOptionRefactored>();
            bool healRolled = false;

            foreach (WeaponBaseRealRefactored weapon in activeWeapons)
            {
                WeaponUpgradeProfile profile = GetProfile(weapon);


                foreach (var upgrade in allAvailableUpgrades)
                {
                    if (blacklistedUpgrades.ContainsKey(weapon) && blacklistedUpgrades[weapon] == upgrade) 
                    {

                        continue;
                    }

                    if (upgrade.levelValues != null && profile.levels.ContainsKey(upgrade) && profile.levels[upgrade] >= upgrade.levelValues.Length) 
                    {

                        continue;
                    }

                    if (upgrade.type == UpgradeType.Heal)
                    {
                        if (!healRolled)
                        {
                            if (Random.Range(0f, 100f) <= 15f)
                            {

                                validOptions.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
                            }
                            healRolled = true;
                        }
                        continue;
                    }

                    if (upgrade.exclusiveWeapon != null && weapon.data != null)
                    {
                        if (upgrade.exclusiveWeapon != weapon.data) continue;
                    }

                    int currentLevel = profile.levels.ContainsKey(upgrade) ? profile.levels[upgrade] : 0;
                    if (upgrade.levelValues != null && currentLevel >= upgrade.levelValues.Length) 
                    {

                        continue;
                    }

                    if (currentLevel == 0)
                    {
                        if (upgrade.category == UpgradeCategory.Firing && profile.firingIcons.Count >= weapon.data.maxFiringUpgrades) 
                        {

                            continue;
                        }
                        if (upgrade.category == UpgradeCategory.Impact && profile.impactIcons.Count >= weapon.data.maxImpactUpgrades) 
                        {

                            continue;
                        }
                    }


                    validOptions.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
                }
            }


            List<UpgradeOptionRefactored> chosen = new List<UpgradeOptionRefactored>();
            for (int i = 0; i < amount; i++)
            {
                if (validOptions.Count == 0) break;
                int rand = Random.Range(0, validOptions.Count);
                chosen.Add(validOptions[rand]);
                validOptions.RemoveAt(rand);
            }


            return chosen;
        }

        public void ApplyUpgrade(UpgradeData data, WeaponBaseRealRefactored weapon)
        {

            
            if (!choosenUpgrades.ContainsKey(data)) choosenUpgrades[data] = 0;
            choosenUpgrades[data]++;


            WeaponUpgradeProfile profile = GetProfile(weapon);

            if (!profile.levels.ContainsKey(data))
            {
                profile.levels[data] = 0;


                if (data.category == UpgradeCategory.Firing) profile.firingIcons.Add(data.icon);
                else if (data.category == UpgradeCategory.Impact) profile.impactIcons.Add(data.icon);
            }

            int currentLevel = profile.levels[data];
            float valueToApply = 0f;
            if (data.levelValues != null && currentLevel < data.levelValues.Length)
            {
                valueToApply = data.levelValues[currentLevel];
            }
            else
            {

            }
            
            profile.levels[data]++;



            switch (data.type)
            {
                case UpgradeType.Explosive: weapon.CurrentExplosiveRadius += valueToApply; break;
                case UpgradeType.Chain: weapon.CurrentChainBounces += (int)valueToApply; break;
                case UpgradeType.Multiple: weapon.CurrentMultipleShots += (int)valueToApply; break;
                case UpgradeType.FireRate: weapon.CurrentBaseCooldown = valueToApply; break;
                case UpgradeType.Range: weapon.CurrentRange += valueToApply; break;
                case UpgradeType.Damage: 
                    weapon.CurrentBaseDamage += valueToApply; 

                    break;

                case UpgradeType.Bleed: { var e = weapon.CurrentEffects; e.bleedPercent += valueToApply; weapon.CurrentEffects = e; break; }
                case UpgradeType.Burn: { var e = weapon.CurrentEffects; e.burnDamage += valueToApply; weapon.CurrentEffects = e; break; }
                case UpgradeType.Poison: { var e = weapon.CurrentEffects; e.poisonDamage += valueToApply; weapon.CurrentEffects = e; break; }
                case UpgradeType.Freeze: { var e = weapon.CurrentEffects; e.freezePercent += valueToApply; weapon.CurrentEffects = e; break; }
                case UpgradeType.Stun: { var e = weapon.CurrentEffects; e.stunChance += valueToApply; weapon.CurrentEffects = e; break; }
                case UpgradeType.Crit: { var e = weapon.CurrentEffects; e.critChance += valueToApply; weapon.CurrentEffects = e; break; }

                case UpgradeType.Duration: weapon.CurrentDuration += valueToApply; break;
                case UpgradeType.ElectricStorm: weapon.CurrentElectricStormChance += valueToApply; break;
                case UpgradeType.HeatHeal: weapon.CurrentHeatHeal += valueToApply; break;

                case UpgradeType.PlayerSpeed:
                case UpgradeType.PlayerSpeedFlat:

                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerEvent(data.type == UpgradeType.PlayerSpeed ? "AddGlobalSpeed" : "AddWeaponSpeedFlat", valueToApply);
                    }
                    break;

                case UpgradeType.CometMode:
                case UpgradeType.ScatterMode:

                    break;

                case UpgradeType.RangeMultiplier: weapon.CurrentRangeMultiplier = valueToApply; break;
                case UpgradeType.MaxDamageCap: weapon.CurrentMaxDamageCap = valueToApply; break;
                case UpgradeType.RampUpInterval: weapon.CurrentRampUpInterval = valueToApply; break;
                case UpgradeType.MagicMode: 
                    if (weapon is ShurikenWeaponRefactored shuriMagico) shuriMagico.isMagicMode = true; 
                    break;
                case UpgradeType.ProjectileSpeed: weapon.CurrentProjectileSpeed += valueToApply; break;
            }
        }

        public int GetPurgeCost(WeaponBaseRealRefactored weapon)
        {
            if (!weaponPurgeCounts.ContainsKey(weapon)) weaponPurgeCounts[weapon] = 0;
            return 200 * (weaponPurgeCounts[weapon] + 1);
        }

        public void PurgeUpgrade(WeaponBaseRealRefactored weapon, UpgradeData upgradeToPurge)
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

        private void RecalculateWeaponStats(WeaponBaseRealRefactored weapon, WeaponUpgradeProfile profile)
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
                        case UpgradeType.Explosive: weapon.CurrentExplosiveRadius += val; break;
                        case UpgradeType.Chain: weapon.CurrentChainBounces += (int)val; break;
                        case UpgradeType.Multiple: weapon.CurrentMultipleShots += (int)val; break;
                        case UpgradeType.FireRate: weapon.CurrentBaseCooldown = val; break;
                        case UpgradeType.Range: weapon.CurrentRange += val; break;
                        case UpgradeType.Damage: weapon.CurrentBaseDamage += val; break;

                        case UpgradeType.Bleed: { var e = weapon.CurrentEffects; e.bleedPercent += val; weapon.CurrentEffects = e; break; }
                        case UpgradeType.Burn: { var e = weapon.CurrentEffects; e.burnDamage += val; weapon.CurrentEffects = e; break; }
                        case UpgradeType.Poison: { var e = weapon.CurrentEffects; e.poisonDamage += val; weapon.CurrentEffects = e; break; }
                        case UpgradeType.Freeze: { var e = weapon.CurrentEffects; e.freezePercent += val; weapon.CurrentEffects = e; break; }
                        case UpgradeType.Stun: { var e = weapon.CurrentEffects; e.stunChance += val; weapon.CurrentEffects = e; break; }
                        case UpgradeType.Crit: { var e = weapon.CurrentEffects; e.critChance += val; weapon.CurrentEffects = e; break; }

                        case UpgradeType.Duration: weapon.CurrentDuration += val; break;
                        case UpgradeType.ElectricStorm: weapon.CurrentElectricStormChance += val; break;
                        case UpgradeType.HeatHeal: weapon.CurrentHeatHeal += val; break;

                        case UpgradeType.RangeMultiplier: weapon.CurrentRangeMultiplier = val; break;
                        case UpgradeType.MaxDamageCap: weapon.CurrentMaxDamageCap = val; break;
                        case UpgradeType.RampUpInterval: weapon.CurrentRampUpInterval = val; break;

                        case UpgradeType.MagicMode: 
                            if (weapon is ShurikenWeaponRefactored shuriMagico2) shuriMagico2.isMagicMode = true; 
                            break;
                        case UpgradeType.ProjectileSpeed: weapon.CurrentProjectileSpeed += val; break;
                    }
                }
            }
        }
    }
}
