using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

[System.Serializable]
public struct PauseWeaponPanel
{
    public GameObject panelObject;
    public Image weaponIcon;
    public Image[] fireSlots;
    public Image[] impactSlots;
    public Image[] bgFire;
    public Image[] bgImpact;
}

public class PauseInventoryUI : MonoBehaviour
{
    [Header("Configuración de Ítems (Abajo)")]
    public Transform itemsContainer;
    public GameObject itemPrefab;

    [Header("Configuración de Armas (Arriba)")]
    public PauseWeaponPanel[] weaponPanels;

    private void OnEnable()
    {
        RefreshItems();
        RefreshWeapons();
    }

    private void RefreshItems()
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv == null) return;

        foreach (var kvp in inv.collectedItems)
        {
            ItemData itemData = kvp.Key;
            int cantidad = kvp.Value;

            GameObject iconObj = Instantiate(itemPrefab, itemsContainer);

            Image img = iconObj.GetComponent<Image>();
            if (img != null) img.sprite = itemData.icon;

            TextMeshProUGUI cantText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (cantText != null) cantText.text = "x" + cantidad;

            AddTooltip(iconObj, itemData.itemName, itemData.description);
        }
    }

    private void RefreshWeapons()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        WeaponBaseRealRefactored[] playerWeapons = player.GetComponentsInChildren<WeaponBaseRealRefactored>();

        for (int i = 0; i < weaponPanels.Length; i++)
        {

            if (i < playerWeapons.Length)
            {
                weaponPanels[i].panelObject.SetActive(true);
                WeaponBaseRealRefactored weapon = playerWeapons[i];
                int captureIndex = i; 


                for (int j = 0; j < weapon.data.maxFiringUpgrades; j++)
                {
                    weaponPanels[captureIndex].bgFire[j].gameObject.SetActive(true);
                }
                for (int j = 0; j < weapon.data.maxImpactUpgrades; j++)
                {
                    weaponPanels[captureIndex].bgImpact[j].gameObject.SetActive(true);
                }

                print(weapon.data.maxFiringUpgrades);
                print(weapon.data.maxImpactUpgrades);   

                weaponPanels[captureIndex].weaponIcon.sprite = weapon.data.weaponIcon;

                float finalDamage = weapon.CurrentBaseDamage;
                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent<System.Action<PlayerStatsRefactored>>("RequestCurrentStats", (stats) => 
                    {
                        if (stats != null) finalDamage *= stats.globalDamageMultiplier;
                    });
                }

                string statsText = $"Daño: {finalDamage}\nTipo: {weapon.CurrentDamageType}\nProb. Crítico: {weapon.CurrentEffects.critChance}%";
                AddTooltip(weaponPanels[captureIndex].weaponIcon.gameObject, weapon.data.weaponName, statsText);

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", weapon, (profile) => 
                    {
                        if (profile != null)
                        {
                            List<UpgradeData> fireUpgrades = new List<UpgradeData>();
                            List<UpgradeData> impactUpgrades = new List<UpgradeData>();

                            foreach (var kvp in profile.levels)
                            {
                                if (kvp.Key.category == UpgradeCategory.Firing) fireUpgrades.Add(kvp.Key);
                                else if (kvp.Key.category == UpgradeCategory.Impact) impactUpgrades.Add(kvp.Key);
                            }

                            FillUpgradeSlots(weaponPanels[captureIndex].fireSlots, profile.firingIcons, fireUpgrades, profile);

                            FillUpgradeSlots(weaponPanels[captureIndex].impactSlots, profile.impactIcons, impactUpgrades, profile);
                        }
                    });
                }
            }
            else
            {
                weaponPanels[i].panelObject.SetActive(false);
            }
        }
    }

    private void FillUpgradeSlots(Image[] slots, List<Sprite> icons, List<UpgradeData> upgradesData, WeaponUpgradeProfile profile)
    {
        for (int f = 0; f < slots.Length; f++)
        {
            if (f < icons.Count)
            {
                slots[f].gameObject.SetActive(true); 
                slots[f].sprite = icons[f];
                slots[f].color = Color.white;

                UpgradeData data = upgradesData[f];
                int currentLevel = profile.levels[data];

                string desc = "Mejora al máximo";
                if (currentLevel - 1 >= 0 && currentLevel - 1 < data.levelDescriptions.Length)
                {
                    desc = data.levelDescriptions[currentLevel - 1];
                }

                AddTooltip(slots[f].gameObject, data.baseName + $" (Lvl {currentLevel})", desc);
            }
            else
            {
                slots[f].gameObject.SetActive(false);
            }
        }
    }

    private void AddTooltip(GameObject obj, string title, string description)
    {
        TooltipTrigger tt = obj.GetComponent<TooltipTrigger>();
        if (tt == null) tt = obj.AddComponent<TooltipTrigger>();

        tt.SetupTooltip(title, description);
    }
}
