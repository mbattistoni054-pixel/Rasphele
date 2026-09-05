using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct WeaponUIPanel
{
    public WeaponData weaponData;
    public GameObject panelObject;
    public Image iconoArma;
    public Image[] fireSlots;
    public Image[] impactSlots;
}

public class MenuMejoras : MonoBehaviour
{
    [Header("Paneles Principales")]
    public GameObject panelSeleccionArma;
    public GameObject panelMejoras;
    public GameObject panelDerecho;

    [Header("Paneles de Armas (Izquierda)")]
    public WeaponUIPanel[] panelesIzquierdos;

    [Header("Elementos del Panel 1 (Inventario)")]
    public Button[] botonesArmas;
    public Image[] iconosBotonesArmas;
    public Image[] iconosBotonesArmaFijo;
    public Image[] armaSlot;
    public TextMeshProUGUI[] textosBotonesArmas;

    [Header("Elementos de Vista de Stats")]
    public WeaponBase currentWeapon;
    public PlayerStats playerStats;
    public TextMeshProUGUI textStatsRight;
    public TextMeshProUGUI textStatsWeapon;
    public TextMeshProUGUI textStatsWeaponVar;
    public TextMeshProUGUI weaponTitle;

    [Header("Elementos del Panel 2 (Las 3 Cartas)")]
    public GameObject[] optionPanels;
    public Image[] optionIcons;
    public TextMeshProUGUI[] optionTitles;
    public TextMeshProUGUI[] optionLevels;
    public TextMeshProUGUI[] damageTypes;
    public TextMeshProUGUI[] optionDescriptions;
    public GameObject closeButton;
    public TextMeshProUGUI mainTitleText;

    public List<WeaponBase> buttonWeapons = new List<WeaponBase>();

    [Header("Nuevas Armas (Recompensas de Nivel)")]
    public List<GameObject> allWeaponPrefabs;
    public List<int> newWeaponLevels = new List<int> { 10, 15 };

    private bool isWeaponMode = false;
    private List<GameObject> currentWeaponChoices;
    private List<UpgradeOption> currentGlobalChoices;
    private WeaponBase[] playerActiveWeapons;

    void OnEnable()
    {
        GameManager.Instance.EnablePause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshRightStats();

        if (playerStats == null) playerStats = PlayerStats.Instance;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerActiveWeapons = player.GetComponentsInChildren<WeaponBase>();
        }

        PlayerExperience xp = Object.FindFirstObjectByType<PlayerExperience>();

        if (xp != null && newWeaponLevels.Contains(xp.currentLevel))
        {
            isWeaponMode = true;
            ShowNewWeaponSelection();
        }
        else
        {
            isWeaponMode = false;
            GenerateGlobalChoices();
            SyncAllPanels();
            ShowMainScreen();
        }
    }

    void OnDisable()
    {
        GameManager.Instance.DisablePause();
        buttonWeapons.Clear();
    }

    private void ShowNewWeaponSelection()
    {
        if (panelSeleccionArma != null) panelSeleccionArma.SetActive(false);

        foreach (var wp in panelesIzquierdos)
        {
            if (wp.panelObject != null) wp.panelObject.SetActive(false);
        }

        if (panelMejoras != null) panelMejoras.SetActive(true);
        if (closeButton != null) closeButton.SetActive(false);

        List<GameObject> availableWeapons = new List<GameObject>();
        foreach (GameObject prefab in allWeaponPrefabs)
        {
            WeaponBase prefabWeapon = prefab.GetComponent<WeaponBase>();
            if (prefabWeapon == null || prefabWeapon.data == null) continue;

            bool alreadyOwned = false;
            if (playerActiveWeapons != null)
            {
                foreach (WeaponBase activeWeapon in playerActiveWeapons)
                {
                    if (activeWeapon.data == prefabWeapon.data)
                    {
                        alreadyOwned = true;
                        break;
                    }
                }
            }

            if (!alreadyOwned) availableWeapons.Add(prefab);
        }

        currentWeaponChoices = new List<GameObject>();
        int amountToPick = Mathf.Min(3, availableWeapons.Count);

        for (int i = 0; i < amountToPick; i++)
        {
            int rand = Random.Range(0, availableWeapons.Count);
            currentWeaponChoices.Add(availableWeapons[rand]);
            availableWeapons.RemoveAt(rand);
        }

        for (int i = 0; i < optionPanels.Length; i++)
        {
            if (i < currentWeaponChoices.Count)
            {
                optionPanels[i].SetActive(true);
                GameObject choice = currentWeaponChoices[i];
                WeaponBase weaponScript = choice.GetComponent<WeaponBase>();

                buttonWeapons.Add(weaponScript);

                optionIcons[i].sprite = weaponScript.data.weaponIcon;
                optionTitles[i].text = weaponScript.data.weaponName;
                optionTitles[i].color = Color.yellow;
                optionLevels[i].text = null;
                damageTypes[i].text = weaponScript.data.damageType.ToString();

                switch (weaponScript.data.damageType)
                {
                    case DamageType.Veneno: damageTypes[i].color = new Color32(255, 71, 133, 255); break;
                    case DamageType.Magico: damageTypes[i].color = Color.magenta; break;
                    case DamageType.Fuego: damageTypes[i].color = Color.red; break;
                    case DamageType.Fisico: damageTypes[i].color = new Color32(197, 165, 88, 255); break;
                    case DamageType.Electrico: damageTypes[i].color = Color.cyan; break;
                    case DamageType.Agua: damageTypes[i].color = Color.blue; break;
                }

                // Vaciamos el texto de la UI normal
                if (optionDescriptions[i] != null) optionDescriptions[i].text = "";
                // Le inyectamos el script del cartel negro al panel de la carta
                AddTooltip(optionPanels[i], weaponScript.data.weaponName, "Añade esta arma a tu arsenal.");

                armaSlot[i].gameObject.SetActive(false);
            }
            else
            {
                optionPanels[i].SetActive(false);
            }
        }

        if (currentWeaponChoices.Count == 0)
        {
            if (closeButton != null) closeButton.SetActive(true);
        }
    }


    public void ShowMainScreen()
    {
        if (panelSeleccionArma != null) panelSeleccionArma.SetActive(true);
        if (panelMejoras != null) panelMejoras.SetActive(true);

        foreach (var wp in panelesIzquierdos)
        {
            if (wp.panelObject != null) wp.panelObject.SetActive(false);
        }

        for (int i = 0; i < botonesArmas.Length; i++)
        {
            if (playerActiveWeapons != null && i < playerActiveWeapons.Length)
            {
                botonesArmas[i].gameObject.SetActive(true);
                WeaponBase w = playerActiveWeapons[i];

                if (w.data != null)
                {
                    if (iconosBotonesArmas[i] != null) iconosBotonesArmas[i].sprite = w.data.weaponIcon;
                }

                int index = i;
                botonesArmas[i].onClick.RemoveAllListeners();
                botonesArmas[i].onClick.AddListener(() => OnWeaponSelected(playerActiveWeapons[index]));
            }
            else
            {
                if (botonesArmas[i] != null) botonesArmas[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnWeaponSelected(WeaponBase selected)
    {
        currentWeapon = selected;

        foreach (var wp in panelesIzquierdos)
        {
            if (wp.weaponData == currentWeapon.data)
            {
                wp.panelObject.SetActive(true);
            }
            else
            {
                wp.panelObject.SetActive(false);
            }
        }

        RefreshRightStats();
    }

    public void OnBackButtonClicked()
    {
        ShowMainScreen();
    }

    private void RefreshRightStats()
    {
        if (playerStats == null) return;

        weaponTitle.text = null;

        float maxHP = playerStats.GetTotalMaxHealth();
        float regenHP = playerStats.itemRegenMoving;
        float speed = playerStats.baseSpeed;
        var xp = playerStats.gameObject.GetComponent<PlayerExperience>().currentXP;
        var xpNext = playerStats.gameObject.GetComponent<PlayerExperience>().xpToNextLevel;
        var xpMult = playerStats.itemXpMultiplier;

        string weak1 = LevelResistanceManager.Instance.weaknesses[0].ToString();
        string weak2 = LevelResistanceManager.Instance.weaknesses[1].ToString();
        string res1 = LevelResistanceManager.Instance.resistances[0].ToString();
        string res2 = LevelResistanceManager.Instance.resistances[1].ToString();

        textStatsRight.text = $"{maxHP}\n{regenHP}\n{speed}\n{xp}/{xpNext}\n{xpMult}\n\n<color=green>{weak1} {weak2}</color>\n<color=red>{res1} {res2}</color>";

        if (currentWeapon == null) return;

        weaponTitle.text = $"\n{currentWeapon.data.weaponName}\nPotential Upgrades";

        foreach (var upgrade in UpgradeManager.Instance.allAvailableUpgrades)
        {
            if (upgrade.exclusiveWeapon == currentWeapon.data)
            {
                if (upgrade.category != UpgradeCategory.Standard) continue;
                textStatsWeapon.text = "\n\n\n\n";
                break;
            }
        }

        foreach (var upgrade in UpgradeManager.Instance.allAvailableUpgrades)
        {
            if (upgrade.exclusiveWeapon == currentWeapon.data)
            {
                if (upgrade.category != UpgradeCategory.Firing) continue;

                if (UpgradeManager.Instance.choosenUpgrades.ContainsKey(upgrade))
                {
                    textStatsWeapon.text += upgrade.baseName + "\n";
                }
                else
                {
                    textStatsWeapon.text += "<color=#848484>" + upgrade.baseName + "</color>" + "\n";
                }
            }
        }

        textStatsWeapon.text += "\n";

        foreach (var upgrade in UpgradeManager.Instance.allAvailableUpgrades)
        {
            if (upgrade.exclusiveWeapon == currentWeapon.data)
            {
                if (upgrade.category != UpgradeCategory.Impact) continue;

                if (UpgradeManager.Instance.choosenUpgrades.ContainsKey(upgrade))
                {
                    textStatsWeapon.text += upgrade.baseName + "\n";
                }
                else
                {
                    textStatsWeapon.text += "<color=#848484>" + upgrade.baseName + "</color>" + "\n";
                }
            }
        }
    }

    private void SyncAllPanels()
    {
        if (UpgradeManager.Instance == null || playerActiveWeapons == null) return;

        foreach (var wp in panelesIzquierdos)
        {
            WeaponBase matchedWeapon = null;
            foreach (var activeWep in playerActiveWeapons)
            {
                if (activeWep.data == wp.weaponData)
                {
                    matchedWeapon = activeWep;
                    break;
                }
            }

            if (matchedWeapon != null)
            {
                WeaponUpgradeProfile profile = UpgradeManager.Instance.GetProfile(matchedWeapon);

                if (wp.iconoArma != null) wp.iconoArma.sprite = matchedWeapon.data.weaponIcon;

                for (int i = 0; i < wp.fireSlots.Length; i++)
                {
                    if (i < profile.firingIcons.Count)
                    {
                        wp.fireSlots[i].gameObject.SetActive(true);
                        wp.fireSlots[i].sprite = profile.firingIcons[i];
                        wp.fireSlots[i].color = Color.white;
                    }
                    else
                    {
                        wp.fireSlots[i].gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < wp.impactSlots.Length; i++)
                {
                    if (i < profile.impactIcons.Count)
                    {
                        wp.impactSlots[i].gameObject.SetActive(true);
                        wp.impactSlots[i].sprite = profile.impactIcons[i];
                        wp.impactSlots[i].color = Color.white;
                    }
                    else
                    {
                        wp.impactSlots[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void GenerateGlobalChoices()
    {
        currentGlobalChoices = UpgradeManager.Instance.GetGlobalRandomUpgrades(3, playerActiveWeapons);

        if (currentGlobalChoices.Count == 0)
        {
            if (closeButton != null) closeButton.SetActive(true);
        }
        else
        {
            if (closeButton != null) closeButton.SetActive(false);
        }

        for (int i = 0; i < optionPanels.Length; i++)
        {
            damageTypes[i].text = null;

            if (i < currentGlobalChoices.Count)
            {
                optionPanels[i].SetActive(true);
                UpgradeData data = currentGlobalChoices[i].data;
                WeaponBase weaponForCard = currentGlobalChoices[i].weapon;

                buttonWeapons.Add(weaponForCard);

                if (data.type == UpgradeType.Heal)
                {
                    optionIcons[i].sprite = data.icon;
                    optionTitles[i].text = data.baseName;
                    optionLevels[i].text = "Consumible";

                    if (optionDescriptions[i] != null) optionDescriptions[i].text = "<color=#00FF00>hola</color> <color=#FF0000>mundo</color>";
                    string desc = data.levelDescriptions.Length > 0 ? data.levelDescriptions[0] : "";
                    AddTooltip(optionPanels[i], data.baseName, desc);
                }
                else
                {
                    WeaponUpgradeProfile profile = UpgradeManager.Instance.GetProfile(weaponForCard);
                    int nextLevel = 1;
                    if (profile.levels.ContainsKey(data))
                    {
                        nextLevel = profile.levels[data] + 1;
                    }

                    optionIcons[i].sprite = data.icon;
                    optionTitles[i].text = data.baseName;

                    armaSlot[i].gameObject.SetActive(true);
                    iconosBotonesArmaFijo[i].sprite = data.exclusiveWeapon.weaponIcon;

                    if (data.category == UpgradeCategory.Standard)
                    {
                        optionLevels[i].text = "BASE";
                        optionLevels[i].color = Color.white;
                        optionTitles[i].color = Color.white;
                    }
                    else if (nextLevel == 1)
                    {
                        optionLevels[i].text = "NEW";
                        optionLevels[i].color = Color.yellow;
                        optionTitles[i].color = Color.yellow;
                    }
                    else
                    {
                        optionLevels[i].text = $"LVL {nextLevel}";
                        optionLevels[i].color = Color.cyan;
                        optionTitles[i].color = Color.cyan;
                    }

                    if (optionDescriptions[i] != null) optionDescriptions[i].text = "1 <color=#00EA0B>> 2</color>"; 
                    string desc = nextLevel - 1 < data.levelDescriptions.Length ? data.levelDescriptions[nextLevel - 1] : "";
                    AddTooltip(optionPanels[i], data.baseName, desc);
                }
            }
            else
            {
                optionPanels[i].SetActive(false);
            }
        }
    }

    private void AddTooltip(GameObject obj, string title, string description)
    {
        TooltipTrigger tt = obj.GetComponent<TooltipTrigger>();
        if (tt == null) tt = obj.AddComponent<TooltipTrigger>();

        tt.SetupTooltip(title, description);
    }

    public void SelectOption1()
    {
        if (isWeaponMode) ApplyNewWeapon(0);
        else ApplyAndClose(0);
    }

    public void SelectOption2()
    {
        if (isWeaponMode) ApplyNewWeapon(1);
        else ApplyAndClose(1);
    }

    public void SelectOption3()
    {
        if (isWeaponMode) ApplyNewWeapon(2);
        else ApplyAndClose(2);
    }

    public void CloseMenu()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        gameObject.SetActive(false);
    }

    private void ApplyNewWeapon(int index)
    {
        if (index >= currentWeaponChoices.Count) return;

        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        GameObject chosenWeaponPrefab = currentWeaponChoices[index];
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Instantiate(chosenWeaponPrefab, player.transform.position, Quaternion.identity, player.transform);
        }

        gameObject.SetActive(false);
    }


    private void ApplyAndClose(int index)
    {
        if (index >= currentGlobalChoices.Count) return;

        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        UpgradeOption chosen = currentGlobalChoices[index];
        UpgradeManager.Instance.ApplyUpgrade(chosen.data, chosen.weapon);

        gameObject.SetActive(false);
    }


    public void PointerOnUpgrade(int index)
    {
        currentWeapon = buttonWeapons[index];
        RefreshRightStats();
    }


    public void PointerOffUpgrade()
    {
        textStatsWeapon.text = null;
        weaponTitle.text = null;
    }
}