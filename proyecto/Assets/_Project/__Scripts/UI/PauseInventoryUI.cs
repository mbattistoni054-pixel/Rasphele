using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    private readonly List<UpgradeData> _fireUpgradesBuffer = new List<UpgradeData>(8);
    private readonly List<UpgradeData> _impactUpgradesBuffer = new List<UpgradeData>(8);

    private void OnEnable()
    {
        RefreshItems();
        RefreshWeapons();
    }

    private Transform GetPlayerTransform()
    {
        if (GameManagerRefactored.Instance != null && GameManagerRefactored.Instance.Player != null)
        {
            return GameManagerRefactored.Instance.Player;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private void RefreshItems()
    {
        if (itemsContainer == null || itemPrefab == null) return;

        for (int i = itemsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsContainer.GetChild(i).gameObject);
        }

        Transform player = GetPlayerTransform();
        if (player == null) return;

        if (!player.TryGetComponent(out PlayerInventory inv) || inv.collectedItems == null) return;

        foreach (var kvp in inv.collectedItems)
        {
            ItemData itemData = kvp.Key;
            if (itemData == null) continue;
            int cantidad = kvp.Value;

            GameObject iconObj = Instantiate(itemPrefab, itemsContainer);

            if (iconObj.TryGetComponent(out Image img))
            {
                img.sprite = itemData.icon;
            }

            TextMeshProUGUI cantText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (cantText != null) cantText.text = "x" + cantidad;

            AddTooltip(iconObj, itemData.itemName, itemData.description);
        }
    }

    private void RefreshWeapons()
    {
        if (weaponPanels == null) return;

        Transform player = GetPlayerTransform();
        if (player == null) return;

        WeaponBaseRealRefactored[] playerWeapons = player.GetComponentsInChildren<WeaponBaseRealRefactored>();

        for (int i = 0; i < weaponPanels.Length; i++)
        {
            if (weaponPanels[i].panelObject == null) continue;

            if (i < playerWeapons.Length && playerWeapons[i] != null && playerWeapons[i].data != null)
            {
                weaponPanels[i].panelObject.SetActive(true);
                WeaponBaseRealRefactored weapon = playerWeapons[i];
                int captureIndex = i;

                if (weaponPanels[captureIndex].bgFire != null)
                {
                    for (int j = 0; j < weaponPanels[captureIndex].bgFire.Length; j++)
                    {
                        if (weaponPanels[captureIndex].bgFire[j] != null)
                            weaponPanels[captureIndex].bgFire[j].gameObject.SetActive(j < weapon.data.maxFiringUpgrades);
                    }
                }

                if (weaponPanels[captureIndex].bgImpact != null)
                {
                    for (int j = 0; j < weaponPanels[captureIndex].bgImpact.Length; j++)
                    {
                        if (weaponPanels[captureIndex].bgImpact[j] != null)
                            weaponPanels[captureIndex].bgImpact[j].gameObject.SetActive(j < weapon.data.maxImpactUpgrades);
                    }
                }

                if (weaponPanels[captureIndex].weaponIcon != null)
                {
                    weaponPanels[captureIndex].weaponIcon.sprite = weapon.data.weaponIcon;

                    float finalDamage = weapon.CurrentBaseDamage;
                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerEvent<System.Action<PlayerStatsRefactored>>(GameEventKeys.RequestCurrentStats, stats =>
                        {
                            if (stats != null) finalDamage *= stats.globalDamageMultiplier;
                        });
                    }

                    string statsText = $"Daño: {finalDamage}\nTipo: {weapon.CurrentDamageType}\nProb. Crítico: {weapon.CurrentEffects.critChance}%";
                    AddTooltip(weaponPanels[captureIndex].weaponIcon.gameObject, weapon.data.weaponName, statsText);
                }

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>(GameEventKeys.RequestUpgradeProfile, weapon, profile =>
                    {
                        if (profile != null && profile.levels != null)
                        {
                            _fireUpgradesBuffer.Clear();
                            _impactUpgradesBuffer.Clear();

                            foreach (var kvp in profile.levels)
                            {
                                if (kvp.Key == null) continue;
                                if (kvp.Key.category == UpgradeCategory.Firing) _fireUpgradesBuffer.Add(kvp.Key);
                                else if (kvp.Key.category == UpgradeCategory.Impact) _impactUpgradesBuffer.Add(kvp.Key);
                            }

                            FillUpgradeSlots(weaponPanels[captureIndex].fireSlots, profile.firingIcons, _fireUpgradesBuffer, profile);
                            FillUpgradeSlots(weaponPanels[captureIndex].impactSlots, profile.impactIcons, _impactUpgradesBuffer, profile);
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
        if (slots == null) return;

        for (int f = 0; f < slots.Length; f++)
        {
            if (slots[f] == null) continue;

            if (icons != null && f < icons.Count && f < upgradesData.Count)
            {
                slots[f].gameObject.SetActive(true);
                slots[f].sprite = icons[f];
                slots[f].color = Color.white;

                UpgradeData data = upgradesData[f];
                int currentLevel = profile.levels[data];

                string desc = "Mejora al máximo";
                if (data.levelDescriptions != null && currentLevel - 1 >= 0 && currentLevel - 1 < data.levelDescriptions.Length)
                {
                    desc = data.levelDescriptions[currentLevel - 1];
                }

                AddTooltip(slots[f].gameObject, $"{data.baseName} (Lvl {currentLevel})", desc);
            }
            else
            {
                slots[f].gameObject.SetActive(false);
            }
        }
    }

    private void AddTooltip(GameObject obj, string title, string description)
    {
        if (obj == null) return;
        if (!obj.TryGetComponent(out TooltipTrigger tt))
        {
            tt = obj.AddComponent<TooltipTrigger>();
        }

        tt.SetupTooltip(title, description);
    }
}