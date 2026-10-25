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

        private readonly Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile> _weaponProfiles = new Dictionary<WeaponBaseRealRefactored, WeaponUpgradeProfile>();
        public Dictionary<WeaponBaseRealRefactored, int> weaponPurgeCounts = new Dictionary<WeaponBaseRealRefactored, int>();
        public Dictionary<WeaponBaseRealRefactored, UpgradeData> blacklistedUpgrades = new Dictionary<WeaponBaseRealRefactored, UpgradeData>();
        public Dictionary<UpgradeData, int> choosenUpgrades = new Dictionary<UpgradeData, int>();

        private readonly List<UpgradeOptionRefactored> _validOptionsBuffer = new List<UpgradeOptionRefactored>(32);
        private readonly List<UpgradeOptionRefactored> _chosenOptionsBuffer = new List<UpgradeOptionRefactored>(8);

        private void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, Action<WeaponUpgradeProfile>>(GameEventKeys.RequestUpgradeProfile, OnRequestUpgradeProfile);
                EventManager.Instance.StartListening<int, WeaponBaseRealRefactored[], Action<List<UpgradeOptionRefactored>>>(GameEventKeys.RequestGlobalRandomUpgrades, OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StartListening<UpgradeData, WeaponBaseRealRefactored>(GameEventKeys.ApplyUpgrade, ApplyUpgrade);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, Action<int>>(GameEventKeys.RequestPurgeCost, OnRequestPurgeCost);
                EventManager.Instance.StartListening<WeaponBaseRealRefactored, UpgradeData>(GameEventKeys.PurgeUpgrade, PurgeUpgrade);
                EventManager.Instance.StartListening<Action<List<UpgradeData>>>(GameEventKeys.RequestAllAvailableUpgrades, OnRequestAllAvailableUpgrades);
                EventManager.Instance.StartListening<Action<Dictionary<UpgradeData, int>>>(GameEventKeys.RequestChosenUpgrades, OnRequestChosenUpgrades);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, Action<WeaponUpgradeProfile>>(GameEventKeys.RequestUpgradeProfile, OnRequestUpgradeProfile);
                EventManager.Instance.StopListening<int, WeaponBaseRealRefactored[], Action<List<UpgradeOptionRefactored>>>(GameEventKeys.RequestGlobalRandomUpgrades, OnRequestGlobalRandomUpgrades);
                EventManager.Instance.StopListening<UpgradeData, WeaponBaseRealRefactored>(GameEventKeys.ApplyUpgrade, ApplyUpgrade);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, Action<int>>(GameEventKeys.RequestPurgeCost, OnRequestPurgeCost);
                EventManager.Instance.StopListening<WeaponBaseRealRefactored, UpgradeData>(GameEventKeys.PurgeUpgrade, PurgeUpgrade);
                EventManager.Instance.StopListening<Action<List<UpgradeData>>>(GameEventKeys.RequestAllAvailableUpgrades, OnRequestAllAvailableUpgrades);
                EventManager.Instance.StopListening<Action<Dictionary<UpgradeData, int>>>(GameEventKeys.RequestChosenUpgrades, OnRequestChosenUpgrades);
            }
        }

        private void OnRequestUpgradeProfile(WeaponBaseRealRefactored weapon, Action<WeaponUpgradeProfile> callback) => callback?.Invoke(GetProfile(weapon));
        private void OnRequestGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons, Action<List<UpgradeOptionRefactored>> callback) => callback?.Invoke(GetGlobalRandomUpgrades(amount, activeWeapons));
        private void OnRequestPurgeCost(WeaponBaseRealRefactored weapon, Action<int> callback) => callback?.Invoke(GetPurgeCost(weapon));
        private void OnRequestAllAvailableUpgrades(Action<List<UpgradeData>> callback) => callback?.Invoke(allAvailableUpgrades);
        private void OnRequestChosenUpgrades(Action<Dictionary<UpgradeData, int>> callback) => callback?.Invoke(choosenUpgrades);

        public WeaponUpgradeProfile GetProfile(WeaponBaseRealRefactored weapon)
        {
            if (weapon == null) return null;

            if (!_weaponProfiles.TryGetValue(weapon, out WeaponUpgradeProfile profile))
            {
                profile = new WeaponUpgradeProfile();
                _weaponProfiles[weapon] = profile;
            }
            return profile;
        }

        public List<UpgradeOptionRefactored> GetGlobalRandomUpgrades(int amount, WeaponBaseRealRefactored[] activeWeapons)
        {
            _validOptionsBuffer.Clear();
            _chosenOptionsBuffer.Clear();

            if (activeWeapons == null || allAvailableUpgrades == null) return _chosenOptionsBuffer;

            bool healRolled = false;

            for (int w = 0; w < activeWeapons.Length; w++)
            {
                WeaponBaseRealRefactored weapon = activeWeapons[w];
                if (weapon == null || weapon.data == null) continue;

                WeaponUpgradeProfile profile = GetProfile(weapon);

                for (int u = 0; u < allAvailableUpgrades.Count; u++)
                {
                    UpgradeData upgrade = allAvailableUpgrades[u];
                    if (upgrade == null) continue;

                    if (blacklistedUpgrades.TryGetValue(weapon, out UpgradeData blacklisted) && blacklisted == upgrade)
                        continue;

                    if (upgrade.type == UpgradeType.Heal)
                    {
                        if (!healRolled)
                        {
                            if (UnityEngine.Random.Range(0f, 100f) <= 15f)
                            {
                                _validOptionsBuffer.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
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

                    _validOptionsBuffer.Add(new UpgradeOptionRefactored { data = upgrade, weapon = weapon });
                }
            }

            for (int i = 0; i < amount && _validOptionsBuffer.Count > 0; i++)
            {
                int rand = UnityEngine.Random.Range(0, _validOptionsBuffer.Count);
                _chosenOptionsBuffer.Add(_validOptionsBuffer[rand]);

                int lastIndex = _validOptionsBuffer.Count - 1;
                _validOptionsBuffer[rand] = _validOptionsBuffer[lastIndex];
                _validOptionsBuffer.RemoveAt(lastIndex);
            }

            return _chosenOptionsBuffer;
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

            if (choosenUpgrades.TryGetValue(data, out int currentCount))
            {
                choosenUpgrades[data] = currentCount + 1;
            }
            else
            {
                choosenUpgrades[data] = 1;
            }

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
            if (weapon == null) return 200;
            int count = weaponPurgeCounts.TryGetValue(weapon, out int c) ? c : 0;
            return 200 * (count + 1);
        }

        public void PurgeUpgrade(WeaponBaseRealRefactored weapon, UpgradeData upgradeToPurge)
        {
            if (weapon == null || upgradeToPurge == null) return;

            WeaponUpgradeProfile profile = GetProfile(weapon);

            if (profile.levels.TryGetValue(upgradeToPurge, out int purgedLevels))
            {
                if (weaponPurgeCounts.TryGetValue(weapon, out int purgeCount))
                {
                    weaponPurgeCounts[weapon] = purgeCount + 1;
                }
                else
                {
                    weaponPurgeCounts[weapon] = 1;
                }

                blacklistedUpgrades[weapon] = upgradeToPurge;

                profile.levels.Remove(upgradeToPurge);
                RemoveIconFromList(profile.firingIcons, upgradeToPurge.icon);
                RemoveIconFromList(profile.impactIcons, upgradeToPurge.icon);

                if (choosenUpgrades.TryGetValue(upgradeToPurge, out int totalCount))
                {
                    int newCount = totalCount - purgedLevels;
                    if (newCount <= 0) choosenUpgrades.Remove(upgradeToPurge);
                    else choosenUpgrades[upgradeToPurge] = newCount;
                }

                RecalculateWeaponStats(weapon, profile);
            }
        }

        private static void RemoveIconFromList(List<Sprite> icons, Sprite targetIcon)
        {
            if (icons == null) return;
            for (int i = icons.Count - 1; i >= 0; i--)
            {
                if (icons[i] == targetIcon)
                {
                    icons.RemoveAt(i);
                }
            }
        }

        private void RecalculateWeaponStats(WeaponBaseRealRefactored weapon, WeaponUpgradeProfile profile)
        {
            weapon.ResetWeaponStats();

            foreach (KeyValuePair<UpgradeData, int> kvp in profile.levels)
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