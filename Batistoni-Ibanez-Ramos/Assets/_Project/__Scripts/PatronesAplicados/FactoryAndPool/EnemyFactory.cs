using UnityEngine;

namespace PatronesAplicados
{
    /// <summary>
    /// Factory: Encargada UNICAMENTE de crear o inicializar los enemigos.
    /// SRP: El Spawner decide cundo y dnde, la Factory sabe cmo instanciarlos o resetearlos.
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        public static EnemyFactory Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Crea una nueva instancia (GameObject) del enemigo si el pool est vaco.
        /// </summary>
        public GameObject CreateEnemyInstance(GameObject prefab, Transform parent, Vector3 spawnPosition = default)
        {
            GameObject newEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            newEnemy.SetActive(false); // Lo instanciamos apagado por defecto
            return newEnemy;
        }

        /// <summary>
        /// Configura un enemigo existente (sacado del Pool) con los nuevos datos antes de activarlo.
        /// </summary>
        public void SetupEnemy(GameObject enemy, Vector3 spawnPosition, Transform playerTarget, float healthMultiplier, float damageMultiplier, string poolKey)
        {
            enemy.transform.position = spawnPosition;
            enemy.SetActive(true);

            EnemyBaseRefactored enemyScript = enemy.GetComponent<EnemyBaseRefactored>();
            if (enemyScript != null)
            {
                enemyScript.poolKey = poolKey;
                enemyScript.SetTarget(playerTarget);
                enemyScript.ApplyDifficulty(healthMultiplier, damageMultiplier);
                enemyScript.ResetStats();
            }
            else
            {
                // Fallback por si algn enemigo an usa el script viejo
                EnemyBase oldScript = enemy.GetComponent<EnemyBase>();
                if (oldScript != null)
                {
                    oldScript.SetTarget(playerTarget);
                    oldScript.ApplyDifficulty(healthMultiplier, damageMultiplier);
                }
            }
        }
    }
}
