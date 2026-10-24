using System;
using System.Collections.Generic;
using UnityEngine;
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

        private readonly Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile> weaponProfiles = new Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile>();
        public Dictionary<WeaponBaseRealRefactored, int> weaponPurgeCounts = new Dictionary<WeaponBaseRealRefactored, int>();
        public Dictionary<WeaponBaseRealRefactored, UpgradeData> blacklistedUpgrades = new Dictionary<WeaponBaseRealRefactored, UpgradeData>();
        public Dictionary<UpgradeData, int> choosenUpgrades = new Dictionary<UpgradeData, int>();

        private void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", OnRequestUpgradeProfile);
                EventManager.Instance.StartListening<int, WeaponBaseRealRefactored[], Action<List<UpgradeOptionRefactored>>>("RequestGlobalRandomUpgrades", OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StartListening<UpgradeData, WeaponBaseRealRefactored>("ApplyUpgrade", ApplyUpgrade);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, Action<int>>("RequestPurgeCost", OnRequestPurgeCost);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, UpgradeData>("PurgeUpgrade", PurgeUpgrade);
                EventManager.Instance.StartListening<Action<List<UpgradeData>>>("RequestAllAvailableUpgrades", OnRequestAllAvailableUpgrades);
                EventManager.Instance.StartListening<Action<Dictionary<UpgradeData, int>>>("RequestChosenUpgrades", OnRequestChosenUpgrades);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", OnRequestUpgradeProfile);
                EventManager.Instance.StopListening<int, WeaponBaseRealRefactored[], Action<List<UpgradeOptionRefactored>>>("RequestGlobalRandomUpgrades", OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StopListening<UpgradeData, WeaponBaseRealRefactored>("ApplyUpgrade", ApplyUpgrade);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, Action<int>>("RequestPurgeCost", OnRequestPurgeCost);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, UpgradeData>("PurgeUpgrade", PurgeUpgrade);
                EventManager.Instance.StopListening<Action<List<UpgradeData>>>("RequestAllAvailableUpgrades", OnRequestAllAvailableUpgrades);
                EventManager.Instance.StopListening<Action<Dictionary<UpgradeData, int>>>("RequestChosenUpgrades", OnRequestChosenUpgrades);
            }
        }

        private void OnRequestUpgradeProfile(WeaponBaseRealRefactored weapon, Action<WeaponUpgradeProfile> callback) => callback?.Invoke(GetProfile(weapon));
        private void OnRequestGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons, Action<List<UpgradeOptionRefactored>> callback) => callback?.Invoke(GetGlobalRandomUpgrades(amount, activeWeapons));
        private void OnRequestPurgeCost(WeaponBaseRealRefactored weapon, Action<int> callback) => callback?.Invoke(GetPurgeCost(weapon));
        private void OnRequestAllAvailableUpgrades(Action<List<UpgradeData>> callback) => callback?.Invoke(allAvailableUpgrades);
        private void OnRequestChosenUpgrades(Action<Dictionary<UpgradeData, int>> callback) => callback?.Invoke(choosenUpgrades);

        public WeaponUpgradeProfile GetProfile(WeaponBaseRealRefactored weapon)
        {
            if (!weaponProfiles.TryGetValue(weapon, out WeaponUpgradeProfile profile))
            {
                profile = new WeaponUpgradeProfile();
                weaponProfiles[weapon] = profile;
            }
            return profile;
        }

        public List<UpgradeOptionRefactored> GetGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons)
        {
            List<UpgradeOptionRefactored> validOptions = new List<UpgradeOptionRefactored>();
            if (activeWeapons == null || allAvailableUpgrades == null) return validOptions;

            bool healRolled = false;

            foreach (WeaponBaseRealRefactored weapon in activeWeapons)
            {
                if (weapon == null || weapon.data == null) continue;
                WeaponUpgradeProfile profile = GetProfile(weapon);

                foreach (UpgradeData upgrade in allAvailableUpgrades)
                {
                    if (upgrade == null) continue;

                    if (blacklistedUpgrades.TryGetValue(weapon, out UpgradeData blacklisted) && blacklisted == upgrade)
                        continue;

                    if (upgrade.type == UpgradeType.Heal)
                    {
                        if (!healRolled)
                        {
                            if (UnityEngine.Random.Range(0f, 100f) <= 15f)
                            {
                                validOptions.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
                            }
                            healRolled = true;
                        }
                        continue;
                    }

                    if (upgrade.exclusiveWeapon != null && upgrade.exclusiveWeapon != weapon.data)
                        continue;

                    int currentLevel = profile.levels.TryGetValue(upgrade, out int lvl) ? lvl : 0;
                    if (upgrade.levelValues != null && currentLevel >= upgrade.levelValues.Length)
                        continue;

                    if (currentLevel == 0)
                    {
                        if (upgrade.category == UpgradeCategory.Firing && profile.firingIcons.Count >= weapon.data.maxFiringUpgrades)
                            continue;
                        if (upgrade.category == UpgradeCategory.Impact && profile.impactIcons.Count >= weapon.data.maxImpactUpgrades)
                            continue;
                    }

                    validOptions.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
                }
            }

            List<UpgradeOptionRefactored> chosen = new List<UpgradeOptionRefactored>(amount);
            for (int i = 0; i < amount; i++)
            {
                if (validOptions.Count == 0) break;
                int rand = UnityEngine.Random.Range(0, validOptions.Count);
                chosen.Add(validOptions[rand]);
                validOptions.RemoveAt(rand);
            }

            return chosen;
        }

        public void ApplyUpgrade(UpgradeData data, WeaponBaseRealRefactored weapon)
        {
            if (data == null) return;

            if (data.type == UpgradeType.Heal)
            {
                float healVal = (data.levelValues != null && data.levelValues.Length > 0) ? data.levelValues[0] : 25f;
                WeaponUpgradeApplier.ApplyStatModification(data, weapon, healVal, false);
                return;
            }

            if (weapon == null) return;

            if (!choosenUpgrades.ContainsKey(data)) choosenUpgrades[data] = 0;
            choosenUpgrades[data]++;

            WeaponUpgradeProfile profile = GetProfile(weapon);

            if (!profile.levels.TryGetValue(data, out int currentLevel))
            {
                currentLevel = 0;
                profile.levels[data] = 0;

                if (data.category == UpgradeCategory.Firing) profile.firingIcons.Add(data.icon);
                else if (data.category == UpgradeCategory.Impact) profile.impactIcons.Add(data.icon);
            }

            float valueToApply = (data.levelValues != null && currentLevel < data.levelValues.Length)
                ? data.levelValues[currentLevel]
                : 0f;

            profile.levels[data] = currentLevel + 1;

            WeaponUpgradeApplier.ApplyStatModification(data, weapon, valueToApply, false);
        }

        public int GetPurgeCost(WeaponBaseRealRefactored weapon)
        {
            int count = weaponPurgeCounts.TryGetValue(weapon, out int c) ? c : 0;
            return 200 * (count + 1);
        }

        public void PurgeUpgrade(WeaponBaseRealRefactored weapon, UpgradeData upgradeToPurge)
        {
            if (weapon == null || upgradeToPurge == null) return;

            WeaponUpgradeProfile profile = GetProfile(weapon);

            if (profile.levels.TryGetValue(upgradeToPurge, out int purgedLevels))
            {
                if (!weaponPurgeCounts.ContainsKey(weapon)) weaponPurgeCounts[weapon] = 0;
                weaponPurgeCounts[weapon]++;

                blacklistedUpgrades[weapon] = upgradeToPurge;

                profile.levels.Remove(upgradeToPurge);
                profile.firingIcons.RemoveAll(icon => icon == upgradeToPurge.icon);
                profile.impactIcons.RemoveAll(icon => icon == upgradeToPurge.icon);

 
                if (choosenUpgrades.TryGetValue(upgradeToPurge, out int totalCount))
                {
                    int newCount = totalCount - purgedLevels;
                    if (newCount <= 0) choosenUpgrades.Remove(upgradeToPurge);
                    else choosenUpgrades[upgradeToPurge] = newCount;
                }

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

                if (uData == null || uData.levelValues == null) continue;

                for (int i = 0; i < levelReached && i < uData.levelValues.Length; i++)
                {
                    WeaponUpgradeApplier.ApplyStatModification(uData, weapon, uData.levelValues[i], true);
                }
            }
        }
    }
}