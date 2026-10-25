using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class WeaponFactory : MonoBehaviour
    {
        public WeaponBaseRealRefactored GiveWeaponToPlayer(GameObject weaponPrefab, Transform weaponHolder)
        {
            if (weaponPrefab == null || weaponHolder == null) return null;

            GameObject newWeaponObj = Instantiate(weaponPrefab, weaponHolder.position, Quaternion.identity, weaponHolder);

            if (!newWeaponObj.TryGetComponent(out WeaponBaseRealRefactored weaponScript))
            {
                Debug.LogError($"[WeaponFactory] El prefab '{weaponPrefab.name}' no tiene un WeaponBaseRealRefactored.");
                return null;
            }

            weaponScript.EnsureInitialized();
            return weaponScript;
        }
    }
}