using System;
using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class MenuMejorasRefactored : MonoBehaviour
    {
        [Header("Sub-Paneles de UI (Vistas Modulares)")]
        public UpgradeLeftInventoryPanelUI leftPanelUI;
        public UpgradeCenterCardsPanelUI centerCardsUI;
        public UpgradeRightStatsPanelUI rightStatsUI;

        [Header("Configuración de Armas Nuevas por Nivel")]
        public List<GameObject> allWeaponPrefabs;
        public List<int> newWeaponLevels = new List<int> { 10, 15 };

        [Header("Estado de Selección")]
        public WeaponBaseRealRefactored currentWeapon;
        public List<WeaponBaseRealRefactored> buttonWeapons = new List<WeaponBaseRealRefactored>(3);

        private readonly WeaponRewardService _weaponRewardService = new WeaponRewardService();
        private readonly UpgradeMenuSessionLogic _sessionLogic = new UpgradeMenuSessionLogic();
        private readonly List<UpgradeCardViewData> _cardsViewBuffer = new List<UpgradeCardViewData>(3);

        private WeaponBaseRealRefactored[] _playerActiveWeapons = Array.Empty<WeaponBaseRealRefactored>();
        private Transform _playerTransform;
        private PlayerExperienceRefactored _cachedPlayerExperience;
        private bool _isWeaponMode;
        private bool _hasRequestedPause;

        private void OnEnable()
        {
            buttonWeapons.Clear();

            RequestPauseOnce();

            _sessionLogic.FetchSessionDataFromEvents();
            CachePlayerAndWeapons();

            if (_cachedPlayerExperience != null && _weaponRewardService.IsWeaponRewardLevel(_cachedPlayerExperience.currentLevel, newWeaponLevels))
            {
                _isWeaponMode = true;
                OpenNewWeaponMode();
            }
            else
            {
                _isWeaponMode = false;
                OpenUpgradeMode();
            }

            RefreshRightStats();
        }

        private void OnDisable()
        {
            ReleasePauseOnce();
            buttonWeapons.Clear();
        }

        public void RequestPauseOnce()
        {
            if (_hasRequestedPause) return;
            _hasRequestedPause = true;

            PauseManager.RequestPause();
        }

        public void ReleasePauseOnce()
        {
            if (!_hasRequestedPause) return;
            _hasRequestedPause = false;

            PauseManager.RequestResume();
        }

        private void CachePlayerAndWeapons()
        {
            if (_playerTransform == null)
            {
                _playerTransform =  GameManagerRefactored.Instance.Player;
            }

            if (_cachedPlayerExperience == null)
            {
                _cachedPlayerExperience = UnityEngine.Object.FindFirstObjectByType<PlayerExperienceRefactored>();
            }

            _playerActiveWeapons = _playerTransform != null ? _playerTransform.GetComponentsInChildren<WeaponBaseRealRefactored>() : Array.Empty<WeaponBaseRealRefactored>();
        }

        private void OpenNewWeaponMode()
        {
            leftPanelUI.SetPanelActive(false);
            rightStatsUI.SetPanelActive(false);

            _weaponRewardService.RollAvailableWeapons(allWeaponPrefabs, _playerActiveWeapons, 3, buttonWeapons, _cardsViewBuffer);

            centerCardsUI.RenderCards("ELIGE UN ARMA NUEVA", _cardsViewBuffer);
        }

        private void OpenUpgradeMode()
        {
            leftPanelUI.SetPanelActive(true);
            rightStatsUI.SetPanelActive(true);

            _sessionLogic.GenerateGlobalUpgradeCards(3, _playerActiveWeapons, buttonWeapons, _cardsViewBuffer);

            centerCardsUI.RenderCards("ELIGE UNA MEJORA", _cardsViewBuffer);

            leftPanelUI.RenderEquippedWeaponButtons(_playerActiveWeapons);

            if (currentWeapon == null || !IsWeaponEquipped(currentWeapon))
            {
                currentWeapon = _playerActiveWeapons.Length > 0 ? _playerActiveWeapons[0] : null;
            }

            RefreshLeftPanel();
        }

        private bool IsWeaponEquipped(WeaponBaseRealRefactored targetWeapon)
        {
            if (_playerActiveWeapons == null || targetWeapon == null) return false;

            for (int i = 0; i < _playerActiveWeapons.Length; i++)
            {
                if (_playerActiveWeapons[i] == targetWeapon) return true;
            }
            return false;
        }

        public void SelectWeaponOption(int index)
        {
            if (_playerActiveWeapons != null && index >= 0 && index < _playerActiveWeapons.Length)
            {
                currentWeapon = _playerActiveWeapons[index];
                RefreshRightStats();
                RefreshLeftPanel();
            }
        }

        public void RefreshRightStats()
        {

            rightStatsUI.RenderPlayerStats(_sessionLogic.PlayerStats);
            rightStatsUI.RenderWeaponStats(currentWeapon, _sessionLogic.AllAvailableUpgrades, _sessionLogic.ChosenUpgrades);
        }

        private void RefreshLeftPanel()
        {

            WeaponUpgradeProfile profile = _sessionLogic.RequestWeaponProfile(currentWeapon);
            leftPanelUI.RenderWeaponSlots(currentWeapon, profile);
        }

        public void SelectOption1() => HandleCardSelection(0);
        public void SelectOption2() => HandleCardSelection(1);
        public void SelectOption3() => HandleCardSelection(2);

        private void HandleCardSelection(int index)
        {
            TooltipManager.Instance.HideTooltip();

            if (_isWeaponMode)
            {
                _weaponRewardService.EquipChosenWeapon(index, _playerTransform);
            }
            else
            {
                _sessionLogic.ApplySelectedUpgrade(index);
            }

            ReleasePauseOnce();
            gameObject.SetActive(false);
        }

        public void CloseMenu()
        {
            TooltipManager.Instance.HideTooltip();
            ReleasePauseOnce();
            gameObject.SetActive(false);
        }

        public void PointerOnUpgrade(int index)
        {
            if (index >= 0 && index < buttonWeapons.Count && buttonWeapons[index] != null)
            {
                currentWeapon = buttonWeapons[index];
                RefreshRightStats();
                RefreshLeftPanel();
            }
        }

        public void PointerOffUpgrade()
        {
            if (rightStatsUI != null)
            {
                rightStatsUI.ClearWeaponStats();
            }
        }
    }
}