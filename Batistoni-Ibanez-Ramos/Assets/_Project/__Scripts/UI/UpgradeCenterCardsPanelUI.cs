using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PatronesAplicados
{

    public struct UpgradeCardViewData
    {
        public Sprite mainIcon;
        public string title;
        public Color titleColor;
        public string levelLabel;
        public Color levelColor;
        public string damageTypeText;
        public string inlineDescription;
        public bool showWeaponBadge;
        public Sprite weaponBadgeIcon;
        public string tooltipTitle;
        public string tooltipDescription;
    }

    public class UpgradeCenterCardsPanelUI : MonoBehaviour
    {
        [Header("Contenedor Principal de Cartas")]
        public GameObject panelMejoras;
        public TextMeshProUGUI mainTitleText;
        public GameObject closeButton;

        [Header("Elementos de las 3 Cartas")]
        public GameObject[] optionPanels;
        public Image[] optionIcons;
        public TextMeshProUGUI[] optionTitles;
        public TextMeshProUGUI[] optionLevels;
        public TextMeshProUGUI[] damageTypes;
        public TextMeshProUGUI[] optionDescriptions;
        public Image[] armaSlot;
        public Image[] iconosBotonesArmaFijo;

        private TooltipTrigger[] _cachedTooltips;

        private void Awake()
        {
            CacheTooltips();
        }

        private void CacheTooltips()
        {
            if (optionPanels == null) return;
            if (_cachedTooltips != null && _cachedTooltips.Length == optionPanels.Length) return;

            _cachedTooltips = new TooltipTrigger[optionPanels.Length];
            for (int i = 0; i < optionPanels.Length; i++)
            {
                if (optionPanels[i] == null) continue;
                TooltipTrigger tt = optionPanels[i].GetComponent<TooltipTrigger>();
                if (tt == null) tt = optionPanels[i].AddComponent<TooltipTrigger>();
                _cachedTooltips[i] = tt;
            }
        }

        public void RenderCards(string headerTitle, IReadOnlyList<UpgradeCardViewData> cardsData)
        {
            CacheTooltips();

            if (panelMejoras != null) panelMejoras.SetActive(true);
            if (mainTitleText != null) mainTitleText.text = headerTitle;

            int validCount = cardsData != null ? cardsData.Count : 0;
            if (closeButton != null) closeButton.SetActive(validCount == 0);

            if (optionPanels == null) return;

            for (int i = 0; i < optionPanels.Length; i++)
            {
                if (optionPanels[i] == null) continue;

                if (i < validCount)
                {
                    optionPanels[i].SetActive(true);
                    UpgradeCardViewData card = cardsData[i];

                    if (optionIcons != null && i < optionIcons.Length && optionIcons[i] != null)
                        optionIcons[i].sprite = card.mainIcon;

                    if (optionTitles != null && i < optionTitles.Length && optionTitles[i] != null)
                    {
                        optionTitles[i].text = card.title;
                        optionTitles[i].color = card.titleColor;
                    }

                    if (optionLevels != null && i < optionLevels.Length && optionLevels[i] != null)
                    {
                        optionLevels[i].text = card.levelLabel;
                        optionLevels[i].color = card.levelColor;
                    }

                    if (damageTypes != null && i < damageTypes.Length && damageTypes[i] != null)
                        damageTypes[i].text = card.damageTypeText;

                    if (optionDescriptions != null && i < optionDescriptions.Length && optionDescriptions[i] != null)
                        optionDescriptions[i].text = card.inlineDescription;

                    if (armaSlot != null && i < armaSlot.Length && armaSlot[i] != null)
                        armaSlot[i].gameObject.SetActive(card.showWeaponBadge);

                    if (card.showWeaponBadge && iconosBotonesArmaFijo != null && i < iconosBotonesArmaFijo.Length && iconosBotonesArmaFijo[i] != null)
                        iconosBotonesArmaFijo[i].sprite = card.weaponBadgeIcon;

                    if (_cachedTooltips != null && i < _cachedTooltips.Length && _cachedTooltips[i] != null)
                        _cachedTooltips[i].SetupTooltip(card.tooltipTitle, card.tooltipDescription);
                }
                else
                {
                    optionPanels[i].SetActive(false);
                }
            }
        }
    }
}