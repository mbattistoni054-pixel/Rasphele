using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{

    public class WeaponRewardService
    {
        private readonly List<GameObject> _validWeaponPrefabs = new List<GameObject>(8);
        private readonly List<GameObject> _currentChoices = new List<GameObject>(3);

        public IReadOnlyList<GameObject> CurrentChoices => _currentChoices;

        public bool IsWeaponRewardLevel(int currentLevel, List<int> rewardLevels)
        {
            return rewardLevels != null && rewardLevels.Contains(currentLevel);
        }

        public void RollAvailableWeapons(
            List<GameObject> allWeaponPrefabs,
            WeaponBaseRealRefactored[] playerActiveWeapons,
            int maxChoices,
            List<WeaponBaseRealRefactored> outCardWeapons,
            List<UpgradeCardViewData> outCardsViewData)
        {
            _validWeaponPrefabs.Clear();
            _currentChoices.Clear();
            outCardWeapons.Clear();
            outCardsViewData.Clear();

            if (allWeaponPrefabs == null) return;

            for (int i = 0; i < allWeaponPrefabs.Count; i++)
            {
                GameObject wpPrefab = allWeaponPrefabs[i];
                if (wpPrefab == null) continue;

                if (!wpPrefab.TryGetComponent(out WeaponBaseRealRefactored wpScript) || wpScript.data == null)
                    continue;

                if (!PlayerAlreadyOwnsWeapon(wpScript.data, playerActiveWeapons))
                {
                    _validWeaponPrefabs.Add(wpPrefab);
                }
            }

            for (int i = 0; i < maxChoices && _validWeaponPrefabs.Count > 0; i++)
            {
                int randIndex = Random.Range(0, _validWeaponPrefabs.Count);
                GameObject chosenPrefab = _validWeaponPrefabs[randIndex];
                _validWeaponPrefabs.RemoveAt(randIndex);

                _currentChoices.Add(chosenPrefab);
                WeaponBaseRealRefactored weaponScript = chosenPrefab.GetComponent<WeaponBaseRealRefactored>();
                outCardWeapons.Add(weaponScript);

                outCardsViewData.Add(new UpgradeCardViewData
                {
                    mainIcon = weaponScript.data.weaponIcon,
                    title = weaponScript.data.weaponName,
                    titleColor = Color.white,
                    levelLabel = "NUEVA",
                    levelColor = Color.yellow,
                    damageTypeText = weaponScript.data.damageType.ToString(),
                    inlineDescription = string.Empty,
                    showWeaponBadge = false,
                    weaponBadgeIcon = null,
                    tooltipTitle = weaponScript.data.weaponName,
                    tooltipDescription = "Obtienes una nueva arma que orbitará o disparará automáticamente."
                });
            }
        }

        public void EquipChosenWeapon(int choiceIndex, Transform playerTransform)
        {
            if (choiceIndex < 0 || choiceIndex >= _currentChoices.Count) return;

            GameObject chosenWeaponPrefab = _currentChoices[choiceIndex];
            if (playerTransform != null && chosenWeaponPrefab != null)
            {
                Object.Instantiate(chosenWeaponPrefab, playerTransform.position, Quaternion.identity, playerTransform);
            }
        }

        private static bool PlayerAlreadyOwnsWeapon(WeaponData candidateData, WeaponBaseRealRefactored[] activeWeapons)
        {
            if (activeWeapons == null) return false;

            for (int i = 0; i < activeWeapons.Length; i++)
            {
                if (activeWeapons[i] != null && activeWeapons[i].data == candidateData)
                    return true;
            }
            return false;
        }
    }
}