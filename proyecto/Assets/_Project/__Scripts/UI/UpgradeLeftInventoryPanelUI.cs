using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public struct WeaponUIPanelRefactored
    {
        public WeaponData weaponData;
        public GameObject panelObject;
        public Image iconoArma;
        public Image[] fireSlots;
        public Image[] impactSlots;
    }

    public class UpgradeLeftInventoryPanelUI : MonoBehaviour
    {
        [Header("Contenedor del Panel Izquierdo")]
        public GameObject panelSeleccionArma;
        public WeaponUIPanelRefactored[] panelesIzquierdos;

        [Header("Botones de Armas Equipadas")]
        public Button[] botonesArmas;
        public Image[] iconosBotonesArmas;
        public TextMeshProUGUI[] textosBotonesArmas;

        public void SetPanelActive(bool isActive)
        {
            if (panelSeleccionArma != null)
            {
                panelSeleccionArma.SetActive(isActive);
            }

            if (!isActive && panelesIzquierdos != null)
            {
                for (int i = 0; i < panelesIzquierdos.Length; i++)
                {
                    if (panelesIzquierdos[i].panelObject != null)
                        panelesIzquierdos[i].panelObject.SetActive(false);
                }
            }
        }

        public void RenderEquippedWeaponButtons(WeaponBaseRealRefactored[] activeWeapons)
        {
            if (botonesArmas == null) return;

            for (int i = 0; i < botonesArmas.Length; i++)
            {
                if (botonesArmas[i] != null)
                    botonesArmas[i].gameObject.SetActive(false);
            }

            if (activeWeapons == null) return;

            for (int i = 0; i < activeWeapons.Length; i++)
            {
                WeaponBaseRealRefactored weapon = activeWeapons[i];
                if (weapon == null || weapon.data == null) continue;

                if (i < botonesArmas.Length && botonesArmas[i] != null)
                {
                    botonesArmas[i].gameObject.SetActive(true);

                    if (iconosBotonesArmas != null && i < iconosBotonesArmas.Length && iconosBotonesArmas[i] != null)
                        iconosBotonesArmas[i].sprite = weapon.data.weaponIcon;

                    if (textosBotonesArmas != null && i < textosBotonesArmas.Length && textosBotonesArmas[i] != null)
                        textosBotonesArmas[i].text = weapon.data.weaponName;
                }
            }
        }

        public void RenderWeaponSlots(WeaponBaseRealRefactored selectedWeapon, WeaponUpgradeProfile profile)
        {
            if (panelesIzquierdos == null) return;

            for (int i = 0; i < panelesIzquierdos.Length; i++)
            {
                WeaponUIPanelRefactored wp = panelesIzquierdos[i];
                if (wp.panelObject == null) continue;

                bool isMatch = selectedWeapon != null && selectedWeapon.data != null && wp.weaponData == selectedWeapon.data;
                wp.panelObject.SetActive(isMatch);

                if (isMatch && profile != null)
                {
                    if (wp.iconoArma != null) wp.iconoArma.sprite = selectedWeapon.data.weaponIcon;
                    UpdateSlotIcons(wp.fireSlots, profile.firingIcons);
                    UpdateSlotIcons(wp.impactSlots, profile.impactIcons);
                }
            }
        }

        private static void UpdateSlotIcons(Image[] slots, List<Sprite> icons)
        {
            if (slots == null) return;

            for (int j = 0; j < slots.Length; j++)
            {
                if (slots[j] == null) continue;

                if (icons != null && j < icons.Count)
                {
                    slots[j].gameObject.SetActive(true);
                    slots[j].sprite = icons[j];
                    slots[j].color = Color.white;
                }
                else
                {
                    slots[j].gameObject.SetActive(false);
                }
            }
        }
    }
}