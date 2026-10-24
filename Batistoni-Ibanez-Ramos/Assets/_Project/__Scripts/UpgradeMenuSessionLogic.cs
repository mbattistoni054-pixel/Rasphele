using System;
using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public class UpgradeMenuSessionLogic
    {
        public List<UpgradeData> AllAvailableUpgrades { get; private set; } = new List<UpgradeData>();
        public Dictionary<UpgradeData, int> ChosenUpgrades { get; private set; } = new Dictionary<UpgradeData, int>();
        public PlayerStatsRefactored PlayerStats { get; private set; }
        public List<UpgradeOptionRefactored> CurrentGlobalChoices { get; private set; } = new List<UpgradeOptionRefactored>();

        public void FetchSessionDataFromEvents()
        {
            if (EventManager.Instance == null) return;

            EventManager.Instance.TriggerEvent<Action<List<UpgradeData>>>(
                "RequestAllAvailableUpgrades", data => AllAvailableUpgrades = data);

            EventManager.Instance.TriggerEvent<Action<Dictionary<UpgradeData, int>>>(
                "RequestChosenUpgrades", data => ChosenUpgrades = data);

            EventManager.Instance.TriggerEvent<Action<PlayerStatsRefactored>>(
                "RequestCurrentStats", stats => PlayerStats = stats);
        }

        public void GenerateGlobalUpgradeCards(
            int count,
            WeaponBaseRealRefactored[] activeWeapons,
            List<WeaponBaseRealRefactored> outCardWeapons,
            List<UpgradeCardViewData> outCardsViewData)
        {
            outCardWeapons.Clear();
            outCardsViewData.Clear();
            CurrentGlobalChoices.Clear();

            if (EventManager.Instance == null) return;

            EventManager.Instance.TriggerEvent<int, WeaponBaseRealRefactored[], Action<List<UpgradeOptionRefactored>>>(
                "RequestGlobalRandomUpgrades",
                count,
                activeWeapons,
                choices =>
                {
                    if (choices != null)
                    {
                        CurrentGlobalChoices = choices;
                    }

                    for (int i = 0; i < CurrentGlobalChoices.Count; i++)
                    {
                        UpgradeOptionRefactored option = CurrentGlobalChoices[i];
                        outCardWeapons.Add(option.weapon);
                        outCardsViewData.Add(BuildCardViewData(option.data, option.weapon));
                    }
                });
        }

        public WeaponUpgradeProfile RequestWeaponProfile(WeaponBaseRealRefactored weapon)
        {
            if (weapon == null || EventManager.Instance == null) return null;

            WeaponUpgradeProfile result = null;
            EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, Action<WeaponUpgradeProfile>>(
                "RequestUpgradeProfile",
                weapon,
                profile => result = profile);

            return result;
        }

        public void ApplySelectedUpgrade(int index)
        {
            if (CurrentGlobalChoices == null || index < 0 || index >= CurrentGlobalChoices.Count) return;

            UpgradeOptionRefactored chosen = CurrentGlobalChoices[index];
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("ApplyUpgrade", chosen.data, chosen.weapon);
            }
        }

        private UpgradeCardViewData BuildCardViewData(UpgradeData data, WeaponBaseRealRefactored weapon)
        {
            if (data.type == UpgradeType.Heal)
            {
                string healTooltip = (data.levelDescriptions != null && data.levelDescriptions.Length > 0)
                    ? data.levelDescriptions[0]
                    : string.Empty;

                return new UpgradeCardViewData
                {
                    mainIcon = data.icon,
                    title = data.baseName,
                    titleColor = Color.white,
                    levelLabel = "Consumible",
                    levelColor = Color.green,
                    damageTypeText = null,
                    inlineDescription = "Restaura la salud.",
                    showWeaponBadge = false,
                    weaponBadgeIcon = null,
                    tooltipTitle = data.baseName,
                    tooltipDescription = healTooltip
                };
            }

            WeaponUpgradeProfile profile = RequestWeaponProfile(weapon);
            int nextLevel = (profile != null && profile.levels.TryGetValue(data, out int currentLvl))
                ? currentLvl + 1
                : 1;

            string levelLabel;
            Color highlightColor;

            if (data.category == UpgradeCategory.Standard)
            {
                levelLabel = "BASE";
                highlightColor = Color.white;
            }
            else if (nextLevel == 1)
            {
                levelLabel = "NEW";
                highlightColor = Color.yellow;
            }
            else
            {
                levelLabel = $"LVL {nextLevel}";
                highlightColor = Color.cyan;
            }

            string desc = (data.levelDescriptions != null && nextLevel - 1 >= 0 && nextLevel - 1 < data.levelDescriptions.Length)
                ? data.levelDescriptions[nextLevel - 1]
                : string.Empty;

            return new UpgradeCardViewData
            {
                mainIcon = data.icon,
                title = data.baseName,
                titleColor = highlightColor,
                levelLabel = levelLabel,
                levelColor = highlightColor,
                damageTypeText = null,
                inlineDescription = string.Empty,
                showWeaponBadge = true,
                weaponBadgeIcon = (weapon != null && weapon.data != null) ? weapon.data.weaponIcon : null,
                tooltipTitle = data.baseName,
                tooltipDescription = desc
            };
        }
    }
}