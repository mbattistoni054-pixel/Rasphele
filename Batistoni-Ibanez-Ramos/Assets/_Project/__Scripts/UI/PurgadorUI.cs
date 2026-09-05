using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct PurgadorWeaponPanel
{
    public WeaponData weaponData;
    public GameObject panelObject; 
    public Button[] botonesMejoras;  // Solo los botones dentro de ESTE panel
    public Image[] iconosMejoras;    // Las imágenes de esos mismos botones
}

public class PurgadorUI : MonoBehaviour
{
    [Header("Referencias Generales")]
    public GameObject panelPrincipal;
    public TextMeshProUGUI textoCosto;

    [Header("Botones de Armas (Izquierda)")]
    public Button[] botonesArmas;
    public Image[] iconosBotonesArmas;

    [Header("Paneles de Mejoras (Medio)")]
    public PurgadorWeaponPanel[] panelesArmas;

    private WeaponBase selectedWeapon;
    private WeaponBase[] playerActiveWeapons;

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.EnablePause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerActiveWeapons = player.GetComponentsInChildren<WeaponBase>();
        }

        // Ocultamos todos los paneles del medio al iniciar
        foreach (var panel in panelesArmas)
        {
            if (panel.panelObject != null) panel.panelObject.SetActive(false);
        }

        CargarArmas();

        // Protección contra el NullReferenceException
        if (textoCosto != null) textoCosto.text = "Selecciona un arma";
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.DisablePause();
    }

    private void CargarArmas()
    {
        for (int i = 0; i < botonesArmas.Length; i++)
        {
            if (playerActiveWeapons != null && i < playerActiveWeapons.Length)
            {
                botonesArmas[i].gameObject.SetActive(true);
                WeaponBase w = playerActiveWeapons[i];

                if (iconosBotonesArmas[i] != null && w.data != null)
                    iconosBotonesArmas[i].sprite = w.data.weaponIcon;

                // Asignación de función por código invisible
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

    public void OnWeaponSelected(WeaponBase weapon)
    {
        selectedWeapon = weapon;

        if (UpgradeManager.Instance != null && textoCosto != null)
        {
            int costo = UpgradeManager.Instance.GetPurgeCost(weapon);
            textoCosto.text = $"Costo de Purgado: $ {costo}";
        }

        MostrarMejorasInstaladas();
    }

    private void MostrarMejorasInstaladas()
    {
        if (selectedWeapon == null || UpgradeManager.Instance == null) return;

        WeaponUpgradeProfile profile = UpgradeManager.Instance.GetProfile(selectedWeapon);

        // Apagamos todos los paneles y encendemos solo el del arma seleccionada
        PurgadorWeaponPanel panelActivo = new PurgadorWeaponPanel();
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

        // Limpiamos (apagamos) todos los botones de ese panel específico
        foreach (var btn in panelActivo.botonesMejoras)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }

        // Llenamos los botones que sí tienen mejoras instaladas
        int i = 0;
        foreach (var kvp in profile.levels)
        {
            if (i >= panelActivo.botonesMejoras.Length) break;

            UpgradeData upgradeToPurge = kvp.Key;

            panelActivo.botonesMejoras[i].gameObject.SetActive(true);

            if (panelActivo.iconosMejoras[i] != null)
                panelActivo.iconosMejoras[i].sprite = upgradeToPurge.icon;

            // ¡AQUÍ SE ASIGNA LA FUNCIÓN DE BORRAR POR CÓDIGO!
            panelActivo.botonesMejoras[i].onClick.RemoveAllListeners();
            panelActivo.botonesMejoras[i].onClick.AddListener(() => IntentarPurgar(upgradeToPurge));

            i++;
        }
    }

    private void IntentarPurgar(UpgradeData upgrade)
    {
        if (UpgradeManager.Instance == null || PlayerStats.Instance == null) return;

        int costo = UpgradeManager.Instance.GetPurgeCost(selectedWeapon);

        if (PlayerStats.Instance.SpendMoney(costo))
        {
            UpgradeManager.Instance.PurgeUpgrade(selectedWeapon, upgrade);
            CerrarMenu();
        }
        else
        {
            if (textoCosto != null) textoCosto.text = "<color=red>¡No tienes suficiente oro!</color>";
        }
    }

    public void CerrarMenu()
    {
        // Limpiamos errores y estados
        if (textoCosto != null) textoCosto.text = "Selecciona un arma";

        // Apagamos todo
        foreach (var panel in panelesArmas)
        {
            if (panel.panelObject != null) panel.panelObject.SetActive(false);
        }

        // Cerramos el objeto padre del UI
        if (panelPrincipal != null) panelPrincipal.SetActive(false);

        // Reactivamos el movimiento del jugador
        if (GameManager.Instance != null) GameManager.Instance.DisablePause();
    }


}