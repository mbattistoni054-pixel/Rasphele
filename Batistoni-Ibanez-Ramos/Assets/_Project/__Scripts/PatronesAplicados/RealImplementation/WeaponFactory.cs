using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// WeaponFactory: Encargada de instanciar y configurar las armas (disparo y auras) 
    /// en el jugador, respetando el Principio de Responsabilidad nica.
    /// Reemplaza la lgica suelta de instanciacin en cofres o managers.
    /// </summary>
    public class WeaponFactory : MonoBehaviour
    {
        public static WeaponFactory Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Instancia el arma, la hace hija del transform indicado (usualmente el Player o un pivot de disparo),
        /// y devuelve la clase base para poder inicializarla.
        /// </summary>
        public WeaponBaseRealRefactored GiveWeaponToPlayer(GameObject weaponPrefab, Transform weaponHolder)
        {
            if (weaponPrefab == null || weaponHolder == null) return null;

            // Instanciar el prefab real
            GameObject newWeaponObj = Instantiate(weaponPrefab, weaponHolder.position, Quaternion.identity, weaponHolder);
            
            WeaponBaseRealRefactored weaponScript = newWeaponObj.GetComponent<WeaponBaseRealRefactored>();
            
            if (weaponScript != null)
            {
                // Aqu se podran aplicar inicializaciones base antes de que UpgradeManager la toque.
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
