using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public class UpgradeRightStatsPanelUI : MonoBehaviour
    {
        [Header("Contenedor y Textos del Panel Derecho")]
        public GameObject panelDerecho;
        public TextMeshProUGUI textStatsRight;
        public TextMeshProUGUI weaponTitle;
        public TextMeshProUGUI textStatsWeaponVar;
        public TextMeshProUGUI textStatsWeapon;

        private readonly StringBuilder _statsBuilder = new StringBuilder(256);

        public void SetPanelActive(bool isActive)
        {
            if (panelDerecho != null)
            {
                panelDerecho.SetActive(isActive);
            }
        }

        public void RenderPlayerStats(PlayerStatsRefactored playerStats)
        {
            if (playerStats == null || textStatsRight == null) return;

            textStatsRight.text =
                $"MAX HP: {playerStats.GetTotalMaxHealth():F0}\n" +
                $"SPEED: {playerStats.baseSpeed}\n" +
                $"ATK MULT: x{playerStats.globalDamageMultiplier:F2}\n" +
                $"FIRE RATE: x{playerStats.globalFireRateMultiplier:F2}\n" +
                $"RANGE: x{playerStats.globalRangeMultiplier:F2}\n";
        }

        public void RenderWeaponStats(
            WeaponBaseRealRefactored weapon,
            List<UpgradeData> allAvailableUpgrades,
            Dictionary<UpgradeData, int> chosenUpgrades)
        {
            if (weapon == null || weapon.data == null)
            {
                ClearWeaponStats("Ningún arma seleccionada");
                return;
            }

            if (weaponTitle != null) weaponTitle.text = weapon.data.weaponName;

            if (textStatsWeaponVar != null)
            {
                textStatsWeaponVar.text =
                    $"{weapon.CurrentBaseDamage}\n" +
                    $"{weapon.CurrentBaseCooldown:F2}s\n" +
                    $"{weapon.CurrentRange}\n" +
                    $"{weapon.CurrentMultipleShots}\n" +
                    $"{weapon.CurrentChainBounces}\n" +
                    $"{weapon.CurrentExplosiveRadius}\n";
            }

            if (textStatsWeapon == null || allAvailableUpgrades == null) return;

            _statsBuilder.Clear();
            _statsBuilder.Append("ESTADO DE MEJORAS:\n\n");

            AppendCategoryUpgrades(weapon.data, UpgradeCategory.Firing, allAvailableUpgrades, chosenUpgrades);
            _statsBuilder.Append('\n');
            AppendCategoryUpgrades(weapon.data, UpgradeCategory.Impact, allAvailableUpgrades, chosenUpgrades);

            textStatsWeapon.text = _statsBuilder.ToString();
        }

        public void ClearWeaponStats(string emptyTitle = "")
        {
            if (textStatsWeapon != null) textStatsWeapon.text = string.Empty;
            if (textStatsWeaponVar != null) textStatsWeaponVar.text = string.Empty;
            if (weaponTitle != null) weaponTitle.text = emptyTitle;
        }

        private void AppendCategoryUpgrades(
            WeaponData weaponData,
            UpgradeCategory category,
            List<UpgradeData> allAvailableUpgrades,
            Dictionary<UpgradeData, int> chosenUpgrades)
        {
            for (int i = 0; i < allAvailableUpgrades.Count; i++)
            {
                UpgradeData upgrade = allAvailableUpgrades[i];
                if (upgrade == null || upgrade.exclusiveWeapon != weaponData || upgrade.category != category)
                    continue;

                if (chosenUpgrades != null && chosenUpgrades.ContainsKey(upgrade))
                {
                    _statsBuilder.Append(upgrade.baseName).Append('\n');
                }
                else
                {
                    _statsBuilder.Append("<color=#848484>").Append(upgrade.baseName).Append("</color>\n");
                }
            }
        }
    }
}