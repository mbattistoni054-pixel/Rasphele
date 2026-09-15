using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyFactory : MonoBehaviour
    {
        public static EnemyFactory Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject CreateEnemyInstance(GameObject prefab, Transform parent, Vector3 spawnPosition = default)
        {
            GameObject newEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            newEnemy.SetActive(false);
            return newEnemy;
        }

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
