using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance { get; private set; }

        [Header("Dependencias")]
        [Tooltip("Arrastra la EnemyFactory aquí. Si lo dejas vacío, la buscará automáticamente en la escena.")]
        [SerializeField] private EnemyFactory enemyFactory;

        private readonly Dictionary<string, Queue<EnemyBaseRefactored>> poolDictionary = new Dictionary<string, Queue<EnemyBaseRefactored>>();
        private readonly HashSet<EnemyBaseRefactored> pooledSet = new HashSet<EnemyBaseRefactored>();
        private readonly Dictionary<GameObject, EnemyBaseRefactored> componentCache = new Dictionary<GameObject, EnemyBaseRefactored>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (enemyFactory == null)
                {
                    enemyFactory = GetComponent<EnemyFactory>();
                    if (enemyFactory == null) enemyFactory = FindFirstObjectByType<EnemyFactory>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public EnemyBaseRefactored GetEnemy(GameObject prefab, Vector3 spawnPosition = default)
        {
            if (prefab == null) return null;

            string enemyType = prefab.name;

            if (!poolDictionary.TryGetValue(enemyType, out Queue<EnemyBaseRefactored> queue))
            {
                queue = new Queue<EnemyBaseRefactored>();
                poolDictionary.Add(enemyType, queue);
            }

            while (queue.Count > 0)
            {
                EnemyBaseRefactored candidate = queue.Dequeue();
                if (candidate != null)
                {
                    pooledSet.Remove(candidate);
                    return candidate;
                }
            }

            if (enemyFactory != null)
            {
                EnemyBaseRefactored newEnemy = enemyFactory.CreateEnemyInstance(prefab, transform, spawnPosition);
                if (newEnemy != null)
                {
                    componentCache[newEnemy.gameObject] = newEnemy;
                }
                return newEnemy;
            }

            Debug.LogError("[EnemyPool] No hay EnemyFactory en la escena.");
            return null;
        }

        public void ReturnEnemy(EnemyBaseRefactored enemyToReturn, string enemyType)
        {
            if (enemyToReturn == null || string.IsNullOrEmpty(enemyType)) return;

            // Evita que un enemigo sea encolado dos veces por error
            if (!pooledSet.Add(enemyToReturn)) return;

            enemyToReturn.gameObject.SetActive(false);

            if (!poolDictionary.TryGetValue(enemyType, out Queue<EnemyBaseRefactored> queue))
            {
                queue = new Queue<EnemyBaseRefactored>();
                poolDictionary.Add(enemyType, queue);
            }

            queue.Enqueue(enemyToReturn);
        }

        public void ReturnEnemy(GameObject enemyObjToReturn, string enemyType)
        {
            if (enemyObjToReturn == null) return;

            if (!componentCache.TryGetValue(enemyObjToReturn, out EnemyBaseRefactored enemyScript))
            {
                if (enemyObjToReturn.TryGetComponent(out enemyScript))
                {
                    componentCache[enemyObjToReturn] = enemyScript;
                }
            }

            if (enemyScript != null)
            {
                ReturnEnemy(enemyScript, enemyType);
            }
            else
            {
                enemyObjToReturn.SetActive(false);
            }
        }
    }
}