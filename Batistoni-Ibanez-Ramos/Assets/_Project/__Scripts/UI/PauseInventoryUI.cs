using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        // Limpiamos los iconos viejos para que no se dupliquen al pausar varias veces
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Buscamos el inventario del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv == null) return;

        // 3. Por cada objeto en su diccionario 'collectedItems', creamos un icono
        foreach (var kvp in inv.collectedItems)
        {
            ItemData itemData = kvp.Key;
            int cantidad = kvp.Value;

            // Clonamos el Prefab adentro del GridLayout
            GameObject iconObj = Instantiate(itemPrefab, itemsContainer);

            // Asignamos la foto
            Image img = iconObj.GetComponent<Image>();
            if (img != null) img.sprite = itemData.icon;

            // Asignamos el texto de cantidad ("x3")
            TextMeshProUGUI cantText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (cantText != null) cantText.text = "x" + cantidad;

            // ¡MAGIA! Le agregamos el Tooltip por código y le pasamos la descripción
            AddTooltip(iconObj, itemData.itemName, itemData.description);
        }
    }

    private void RefreshWeapons()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        WeaponBase[] playerWeapons = player.GetComponentsInChildren<WeaponBase>();

        for (int i = 0; i < weaponPanels.Length; i++)
        {
            // Si el jugador tiene un arma para este hueco (0, 1 o 2)
            if (i < playerWeapons.Length)
            {
                weaponPanels[i].panelObject.SetActive(true);
                WeaponBase weapon = playerWeapons[i];


                for (int j = 0; j < weapon.data.maxFiringUpgrades; j++)
                {
                    weaponPanels[i].bgFire[j].gameObject.SetActive(true);
                }
                for (int j = 0; j < weapon.data.maxImpactUpgrades; j++)
                {
                    weaponPanels[i].bgImpact[j].gameObject.SetActive(true);
                }

                print(weapon.data.maxFiringUpgrades);
                print(weapon.data.maxImpactUpgrades);   

                // Configurar Icono del Arma y su Tooltip (Estadísticas dinámicas)
                weaponPanels[i].weaponIcon.sprite = weapon.data.weaponIcon;

                // Calculamos el daño final (Base + Buffs Globales)
                float finalDamage = weapon.CurrentBaseDamage;
                if (PlayerStats.Instance != null) finalDamage *= PlayerStats.Instance.globalDamageMultiplier;

                string statsText = $"Daño: {finalDamage}\nTipo: {weapon.DamageType}\nProb. Crítico: {weapon.CurrentEffects.critChance}%";
                AddTooltip(weaponPanels[i].weaponIcon.gameObject, weapon.data.weaponName, statsText);

                // Configurar los iconos de las mejoras
                if (UpgradeManager.Instance != null)
                {
                    WeaponUpgradeProfile profile = UpgradeManager.Instance.GetProfile(weapon);

                    // Extraemos los datos de las mejoras para saber sus nombres y descripciones
                    List<UpgradeData> fireUpgrades = new List<UpgradeData>();
                    List<UpgradeData> impactUpgrades = new List<UpgradeData>();

                    foreach (var kvp in profile.levels)
                    {
                        if (kvp.Key.category == UpgradeCategory.Firing) fireUpgrades.Add(kvp.Key);
                        else if (kvp.Key.category == UpgradeCategory.Impact) impactUpgrades.Add(kvp.Key);
                    }

                    // Llenamos Huecos de Disparo (Azules)
                    FillUpgradeSlots(weaponPanels[i].fireSlots, profile.firingIcons, fireUpgrades, profile);

                    // Llenamos Huecos de Impacto (Rojos)
                    FillUpgradeSlots(weaponPanels[i].impactSlots, profile.impactIcons, impactUpgrades, profile);
                }
            }
            else
            {
                // Si no tiene 2da o 3ra arma, apagamos ese panel
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

                // Le pasamos el nombre de la mejora y su descripción actual según el nivel
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