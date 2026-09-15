using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    [System.Serializable]
    public struct WeaponUIPanelRefactored
    {
        public WeaponData weaponData;
        public GameObject panelObject;
        public Image iconoArma;
        public Image[] fireSlots;
        public Image[] impactSlots;
    }

    public class MenuMejorasRefactored : MonoBehaviour
    {
        [Header("Paneles Principales")]
        public GameObject panelSeleccionArma;
        public GameObject panelMejoras;
        public GameObject panelDerecho;

        [Header("Paneles de Armas (Izquierda)")]
        public WeaponUIPanelRefactored[] panelesIzquierdos;

        [Header("Elementos del Panel 1 (Inventario)")]
        public Button[] botonesArmas;
        public Image[] iconosBotonesArmas;
        public Image[] iconosBotonesArmaFijo;
        public Image[] armaSlot;
        public TextMeshProUGUI[] textosBotonesArmas;

        [Header("Elementos de Vista de Stats")]
        public WeaponBaseRealRefactored currentWeapon;
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

        public List<WeaponBaseRealRefactored> buttonWeapons = new List<WeaponBaseRealRefactored>();

        [Header("Nuevas Armas (Recompensas de Nivel)")]
        public List<GameObject> allWeaponPrefabs;
        public List<int> newWeaponLevels = new List<int> { 10, 15 };

        private bool isWeaponMode = false;
        private List<GameObject> currentWeaponChoices;
        private List<UpgradeOptionRefactored> currentGlobalChoices;
        private WeaponBaseRealRefactored[] playerActiveWeapons;

        private List<UpgradeData> cachedAllAvailableUpgrades = new List<UpgradeData>();
        private Dictionary<UpgradeData, int> cachedChosenUpgrades = new Dictionary<UpgradeData, int>();
        private PlayerStatsRefactored cachedPlayerStats;

        void OnEnable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("PauseRequested");
                
                EventManager.Instance.TriggerEvent<System.Action<List<UpgradeData>>>("RequestAllAvailableUpgrades", (data) => cachedAllAvailableUpgrades = data);
                EventManager.Instance.TriggerEvent<System.Action<Dictionary<UpgradeData, int>>>("RequestChosenUpgrades", (data) => cachedChosenUpgrades = data);
                EventManager.Instance.TriggerEvent<System.Action<PlayerStatsRefactored>>("RequestCurrentStats", (stats) => cachedPlayerStats = stats);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerActiveWeapons = player.GetComponentsInChildren<WeaponBaseRealRefactored>();
            }

            PlayerExperienceRefactored xp = Object.FindFirstObjectByType<PlayerExperienceRefactored>();

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

            RefreshRightStats();
        }

        void OnDisable()
        {
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ResumeRequested");
            buttonWeapons.Clear();
        }

        private void ShowNewWeaponSelection()
        {
            if (mainTitleText != null) mainTitleText.text = "ELIGE UN ARMA NUEVA";

            if (panelDerecho != null) panelDerecho.SetActive(false);
            
            if (closeButton != null) closeButton.SetActive(false);

            List<GameObject> validWeapons = new List<GameObject>();
            foreach (var wpPrefab in allWeaponPrefabs)
            {
                WeaponBaseRealRefactored wpScript = wpPrefab.GetComponent<WeaponBaseRealRefactored>();
                if (wpScript == null) continue;
                bool alreadyHas = false;
                foreach (var active in playerActiveWeapons)
                {
                    if (active.data == wpScript.data)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
                if (!alreadyHas) validWeapons.Add(wpPrefab);
            }

            currentWeaponChoices = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                if (validWeapons.Count == 0) break;
                int rand = Random.Range(0, validWeapons.Count);
                currentWeaponChoices.Add(validWeapons[rand]);
                validWeapons.RemoveAt(rand);
            }

            for (int i = 0; i < optionPanels.Length; i++)
            {
                if (i < currentWeaponChoices.Count)
                {
                    optionPanels[i].SetActive(true);
                    WeaponBaseRealRefactored weaponScript = currentWeaponChoices[i].GetComponent<WeaponBaseRealRefactored>();
                    
                    optionIcons[i].sprite = weaponScript.data.weaponIcon;
                    optionTitles[i].text = weaponScript.data.weaponName;
                    optionLevels[i].text = "NUEVA";
                    optionLevels[i].color = Color.yellow;
                    
                    armaSlot[i].gameObject.SetActive(false);
                    damageTypes[i].text = weaponScript.data.damageType.ToString();
                    AddTooltip(optionPanels[i], weaponScript.data.weaponName, "Obtienes una nueva arma que orbitara o disparara automaticamente.");
                }
                else
                {
                    optionPanels[i].SetActive(false);
                }
            }
        }

        private void ShowMainScreen()
        {
            if (mainTitleText != null) mainTitleText.text = "ELIGE UNA MEJORA";
            if (panelMejoras != null) panelMejoras.SetActive(true);
            if (panelDerecho != null) panelDerecho.SetActive(true);
        }

        public void SelectWeaponOption(int index)
        {
            if (index < playerActiveWeapons.Length)
            {
                currentWeapon = playerActiveWeapons[index];
                RefreshRightStats();
                RefreshLeftPanel();
            }
        }

        public void RefreshRightStats()
        {
            if (cachedPlayerStats != null)
            {
                textStatsRight.text = 
                    $"MAX HP: {cachedPlayerStats.baseMaxHealth + cachedPlayerStats.itemHealthFlat}\n" +
                    $"SPEED: {cachedPlayerStats.baseSpeed}\n" +
                    $"ATK MULT: x{cachedPlayerStats.globalDamageMultiplier:F2}\n" +
                    $"FIRE RATE: x{cachedPlayerStats.globalFireRateMultiplier:F2}\n" +
                    $"RANGE: x{cachedPlayerStats.globalRangeMultiplier:F2}\n";
            }

            if (currentWeapon == null)
            {
                textStatsWeapon.text = "";
                textStatsWeaponVar.text = "";
                weaponTitle.text = "Ningun arma seleccionada";
                return;
            }

            weaponTitle.text = currentWeapon.data.weaponName;

            textStatsWeaponVar.text = 
                $"{currentWeapon.CurrentBaseDamage}\n" +
                $"{currentWeapon.CurrentBaseCooldown:F2}s\n" +
                $"{currentWeapon.CurrentRange}\n" +
                $"{currentWeapon.CurrentMultipleShots}\n" +
                $"{currentWeapon.CurrentChainBounces}\n" +
                $"{currentWeapon.CurrentExplosiveRadius}\n";

            textStatsWeapon.text = "ESTADO DE MEJORAS:\n\n";

            foreach (var upgrade in cachedAllAvailableUpgrades)
            {
                if (upgrade.exclusiveWeapon == currentWeapon.data)
                {
                    if (upgrade.category != UpgradeCategory.Firing) continue;

                    if (cachedChosenUpgrades.ContainsKey(upgrade))
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

            foreach (var upgrade in cachedAllAvailableUpgrades)
            {
                if (upgrade.exclusiveWeapon == currentWeapon.data)
                {
                    if (upgrade.category != UpgradeCategory.Impact) continue;

                    if (cachedChosenUpgrades.ContainsKey(upgrade))
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
            if (playerActiveWeapons == null || EventManager.Instance == null) return;

            for (int i = 0; i < botonesArmas.Length; i++) 
            {
                if (botonesArmas[i] != null) botonesArmas[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < playerActiveWeapons.Length; i++)
            {
                WeaponBaseRealRefactored activeWep = playerActiveWeapons[i];
                int index = i;

                if (index < botonesArmas.Length && botonesArmas[index] != null)
                {
                    botonesArmas[index].gameObject.SetActive(true);
                    if (index < iconosBotonesArmas.Length && iconosBotonesArmas[index] != null)
                        iconosBotonesArmas[index].sprite = activeWep.data.weaponIcon;
                    if (index < textosBotonesArmas.Length && textosBotonesArmas[index] != null)
                        textosBotonesArmas[index].text = activeWep.data.weaponName;
                }
            }

            if (currentWeapon == null || !System.Array.Exists(playerActiveWeapons, w => w == currentWeapon))
            {
                if (playerActiveWeapons.Length > 0) currentWeapon = playerActiveWeapons[0];
            }

            RefreshLeftPanel();
        }


        private void RefreshLeftPanel()
        {
            if (currentWeapon == null || EventManager.Instance == null) return;

            for (int i = 0; i < panelesIzquierdos.Length; i++)
            {
                var wp = panelesIzquierdos[i];
                if (wp.panelObject == null) continue;

                if (wp.weaponData == currentWeapon.data)
                {
                    wp.panelObject.SetActive(true);
                    
                    EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", currentWeapon, (profile) => 
                    {
                        if (wp.iconoArma != null) wp.iconoArma.sprite = currentWeapon.data.weaponIcon;

                        if (wp.fireSlots != null)
                        {
                            for (int j = 0; j < wp.fireSlots.Length; j++)
                            {
                                if (wp.fireSlots[j] == null) continue;
                                if (j < profile.firingIcons.Count)
                                {
                                    wp.fireSlots[j].gameObject.SetActive(true);
                                    wp.fireSlots[j].sprite = profile.firingIcons[j];
                                    wp.fireSlots[j].color = Color.white;
                                }
                                else
                                {
                                    wp.fireSlots[j].gameObject.SetActive(false);
                                }
                            }
                        }

                        if (wp.impactSlots != null)
                        {
                            for (int j = 0; j < wp.impactSlots.Length; j++)
                            {
                                if (wp.impactSlots[j] == null) continue;
                                if (j < profile.impactIcons.Count)
                                {
                                    wp.impactSlots[j].gameObject.SetActive(true);
                                    wp.impactSlots[j].sprite = profile.impactIcons[j];
                                    wp.impactSlots[j].color = Color.white;
                                }
                                else
                                {
                                    wp.impactSlots[j].gameObject.SetActive(false);
                                }
                            }
                        }
                    });
                }
                else
                {
                    wp.panelObject.SetActive(false);
                }
            }
        }

        private void GenerateGlobalChoices()
        {
            if (EventManager.Instance == null) return;


            EventManager.Instance.TriggerEvent<int, WeaponBaseRealRefactored[], System.Action<List<UpgradeOptionRefactored>>>("RequestGlobalRandomUpgrades", 3, playerActiveWeapons, (choices) =>
            {
                currentGlobalChoices = choices;


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
                        WeaponBaseRealRefactored weaponForCard = currentGlobalChoices[i].weapon;

                        buttonWeapons.Add(weaponForCard);

                        if (data.type == UpgradeType.Heal)
                        {
                            optionIcons[i].sprite = data.icon;
                            optionTitles[i].text = data.baseName;
                            optionLevels[i].text = "Consumible";

                            if (optionDescriptions[i] != null) optionDescriptions[i].text = "Restaura la salud.";
                            string desc = data.levelDescriptions.Length > 0 ? data.levelDescriptions[0] : "";
                            AddTooltip(optionPanels[i], data.baseName, desc);

                        }
                        else
                        {
                            EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", weaponForCard, (profile) =>
                            {
                                int nextLevel = 1;
                                if (profile.levels.ContainsKey(data))
                                {
                                    nextLevel = profile.levels[data] + 1;
                                }

                                optionIcons[i].sprite = data.icon;
                                optionTitles[i].text = data.baseName;

                                armaSlot[i].gameObject.SetActive(true);
                                iconosBotonesArmaFijo[i].sprite = weaponForCard.data.weaponIcon;

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

                                string desc = nextLevel - 1 < data.levelDescriptions.Length ? data.levelDescriptions[nextLevel - 1] : "";
                                AddTooltip(optionPanels[i], data.baseName, desc);

                            });
                        }
                    }
                    else
                    {

                        optionPanels[i].SetActive(false);
                    }
                }
            });
        }

        private void AddTooltip(GameObject obj, string title, string description)
        {
            TooltipTrigger tt = obj.GetComponent<TooltipTrigger>();
            if (tt == null) tt = obj.AddComponent<TooltipTrigger>();

            tt.SetupTooltip(title, description);
        }

        public void SelectOption1() { if (isWeaponMode) ApplyNewWeapon(0); else ApplyAndClose(0); }
        public void SelectOption2() { if (isWeaponMode) ApplyNewWeapon(1); else ApplyAndClose(1); }
        public void SelectOption3() { if (isWeaponMode) ApplyNewWeapon(2); else ApplyAndClose(2); }

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

            UpgradeOptionRefactored chosen = currentGlobalChoices[index];

            
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("ApplyUpgrade", chosen.data, chosen.weapon);
            }

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
}
