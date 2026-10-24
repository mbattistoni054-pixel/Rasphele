using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyFactory : MonoBehaviour
    {
        public static EnemyFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public EnemyBaseRefactored CreateEnemyInstance(GameObject prefab, Transform parent, Vector3 spawnPosition = default)
        {
            GameObject newEnemyObj = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            newEnemyObj.SetActive(false);

            EnemyBaseRefactored enemyScript = newEnemyObj.GetComponent<EnemyBaseRefactored>();
            if (enemyScript == null)
            {
                Debug.LogError($"EnemyFactory: El prefab '{prefab.name}' no tiene el componente EnemyBaseRefactored.");
            }

            return enemyScript;
        }


        public void SetupEnemy(EnemyBaseRefactored enemyScript, Vector3 spawnPosition, Transform playerTarget, float healthMultiplier, float damageMultiplier, string poolKey)
        {
            if (enemyScript == null) return;

            enemyScript.transform.position = spawnPosition;
            enemyScript.poolKey = poolKey;
            enemyScript.SetTarget(playerTarget);
            enemyScript.ApplyDifficulty(healthMultiplier, damageMultiplier);

            enemyScript.gameObject.SetActive(true);
            enemyScript.ResetStats();
        }

       
    }
}