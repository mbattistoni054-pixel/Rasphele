using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{

    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance;

        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }


        public GameObject GetEnemy(GameObject prefab, Vector3 spawnPosition = default)
        {
            string enemyType = prefab.name;

            if (!poolDictionary.ContainsKey(enemyType))
            {
                poolDictionary.Add(enemyType, new Queue<GameObject>());
            }

            if (poolDictionary[enemyType].Count > 0)
            {
                GameObject enemyToReuse = poolDictionary[enemyType].Dequeue();
                return enemyToReuse;
            }
            else
            {
                if (EnemyFactory.Instance != null)
                {
                    return EnemyFactory.Instance.CreateEnemyInstance(prefab, transform, spawnPosition);
                }
                else
                {
                    Debug.LogError("No hay EnemyFactory en la escena.");
                    return null;
                }
            }
        }


        public void ReturnEnemy(GameObject enemyToReturn, string enemyType)
        {
            enemyToReturn.SetActive(false);

            if (!poolDictionary.ContainsKey(enemyType))
            {
                poolDictionary.Add(enemyType, new Queue<GameObject>());
            }

            poolDictionary[enemyType].Enqueue(enemyToReturn);
        }
    }
}
