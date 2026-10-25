using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyFactory : MonoBehaviour
    {
        public EnemyBaseRefactored CreateEnemyInstance(GameObject prefab, Transform parent, Vector3 spawnPosition = default)
        {
            if (prefab == null) return null;

            GameObject newEnemyObj = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            newEnemyObj.SetActive(false);

            if (!newEnemyObj.TryGetComponent(out EnemyBaseRefactored enemyScript))
            {
                Debug.LogError($"[EnemyFactory] El prefab '{prefab.name}' no tiene el componente EnemyBaseRefactored.");
            }

            return enemyScript;
        }

        public void SetupEnemy(
            EnemyBaseRefactored enemyScript,
            Vector3 spawnPosition,
            Transform playerTarget,
            float healthMultiplier,
            float damageMultiplier,
            string poolKey)
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