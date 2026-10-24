using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class WeaponFactory : MonoBehaviour
    {
        public static WeaponFactory Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public WeaponBaseRealRefactored GiveWeaponToPlayer(GameObject weaponPrefab, Transform weaponHolder)
        {
            if (weaponPrefab == null || weaponHolder == null) return null;

            GameObject newWeaponObj = Instantiate(weaponPrefab, weaponHolder.position, Quaternion.identity, weaponHolder);
            
            WeaponBaseRealRefactored weaponScript = newWeaponObj.GetComponent<WeaponBaseRealRefactored>();
            
            if (weaponScript != null)
            {
                Debug.Log($"WeaponFactory: Arma {weaponPrefab.name} creada y asignada al jugador.");
            }
            else
            {
                Debug.LogError($"WeaponFactory: El prefab {weaponPrefab.name} no tiene un WeaponBaseRealRefactored.");
            }

            return weaponScript;
        }
    }
}
