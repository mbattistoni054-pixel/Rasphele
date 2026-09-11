using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PatronesAplicados.RealImplementation; // Necesario para WeaponBaseRealRefactored

namespace PatronesAplicados
{
    [System.Serializable]
    public struct PurgadorWeaponPanelRefactored
    {
        public WeaponData weaponData;
        public GameObject panelObject; 
        public Button[] botonesMejoras;
        public Image[] iconosMejoras;
    }

    public class PurgadorUIRefactored : MonoBehaviour
    {
        [Header("Referencias Generales")]
        public GameObject panelPrincipal;
        public TextMeshProUGUI textoCosto;

        [Header("Botones de Armas (Izquierda)")]
        public Button[] botonesArmas;
        public Image[] iconosBotonesArmas;

        [Header("Paneles de Mejoras (Medio)")]
        public PurgadorWeaponPanelRefactored[] panelesArmas;

        private WeaponBaseRealRefactored selectedWeapon;
        private WeaponBaseRealRefactored[] playerActiveWeapons;

        void OnEnable()
        {
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("PauseRequested");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerActiveWeapons = player.GetComponentsInChildren<WeaponBaseRealRefactored>();
            }

            foreach (var panel in panelesArmas)
            {
                if (panel.panelObject != null) panel.panelObject.SetActive(false);
            }

            CargarArmas();

            if (textoCosto != null) textoCosto.text = "Selecciona un arma";
        }

        void OnDisable()
        {
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ResumeRequested");
        }

        private void CargarArmas()
        {
            for (int i = 0; i < botonesArmas.Length; i++)
            {
                if (playerActiveWeapons != null && i < playerActiveWeapons.Length)
                {
                    botonesArmas[i].gameObject.SetActive(true);
                    WeaponBaseRealRefactored w = playerActiveWeapons[i];

                    if (iconosBotonesArmas[i] != null && w.data != null)
                        iconosBotonesArmas[i].sprite = w.data.weaponIcon;

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

        public void OnWeaponSelected(WeaponBaseRealRefactored weapon)
        {
            selectedWeapon = weapon;

            if (EventManager.Instance != null && textoCosto != null)
            {
                // UI Tonta: Solo solicita el costo mediante un callback
                EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<int>>("RequestPurgeCost", weapon, (costo) => 
                {
                    textoCosto.text = $"Costo de Purgado: $ {costo}";
                });
            }

            MostrarMejorasInstaladas();
        }

        private void MostrarMejorasInstaladas()
        {
            if (selectedWeapon == null || EventManager.Instance == null) return;

            // Solicitar Profile
            EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<WeaponUpgradeProfile>>("RequestUpgradeProfile", selectedWeapon, (profile) => 
            {
                PurgadorWeaponPanelRefactored panelActivo = new PurgadorWeaponPanelRefactored();
                bool panelEncontrado = false;

                foreach (var panel in panelesArmas)
                {
                    if (panel.weaponData == selectedWeapon.data)
                    {
                        if (panel.panelObject != null) panel.panelObject.SetActive(true);
                        panelActivo = panel;
                        panelEncontrado = true;
                    }
                    else
                    {
                        if (panel.panelObject != null) panel.panelObject.SetActive(false);
                    }
                }

                if (!panelEncontrado) return;

                foreach (var btn in panelActivo.botonesMejoras)
                {
                    if (btn != null) btn.gameObject.SetActive(false);
                }

                int i = 0;
                foreach (var kvp in profile.levels)
                {
                    if (i >= panelActivo.botonesMejoras.Length) break;

                    UpgradeData upgradeToPurge = kvp.Key;

                    panelActivo.botonesMejoras[i].gameObject.SetActive(true);

                    if (panelActivo.iconosMejoras[i] != null)
                        panelActivo.iconosMejoras[i].sprite = upgradeToPurge.icon;

                    panelActivo.botonesMejoras[i].onClick.RemoveAllListeners();
                    panelActivo.botonesMejoras[i].onClick.AddListener(() => IntentarPurgar(upgradeToPurge));

                    i++;
                }
            });
        }

        private void IntentarPurgar(UpgradeData upgrade)
        {
            if (EventManager.Instance == null || selectedWeapon == null) return;

            EventManager.Instance.TriggerEvent<WeaponBaseRealRefactored, System.Action<int>>("RequestPurgeCost", selectedWeapon, (costo) => 
            {
                // callback de economia asincrono
                EventManager.Instance.TriggerEvent<int, System.Action<bool>>("RequestSpendMoney", costo, (success) => 
                {
                    if (success)
                    {
                        EventManager.Instance.TriggerEvent("PurgeUpgrade", selectedWeapon, upgrade);
                        CerrarMenu();
                    }
                    else
                    {
                        if (textoCosto != null) textoCosto.text = "<color=red>No tienes suficiente oro!</color>";
                    }
                });
            });
        }

        public void CerrarMenu()
        {
            if (textoCosto != null) textoCosto.text = "Selecciona un arma";

            foreach (var panel in panelesArmas)
            {
                if (panel.panelObject != null) panel.panelObject.SetActive(false);
            }

            if (panelPrincipal != null) panelPrincipal.SetActive(false);
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ResumeRequested");
        }
    }
}
