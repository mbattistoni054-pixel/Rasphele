using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance { get; private set; }

        [Header("Dependencias (SOLID - Inversión de Dependencias)")]
        [Tooltip("Arrastra la EnemyFactory aquí. Si lo dejas vacío, la buscará automáticamente.")]
        [SerializeField] private EnemyFactory enemyFactory;

        private readonly Dictionary<string, Queue<EnemyBaseRefactored>> poolDictionary = new Dictionary<string, Queue<EnemyBaseRefactored>>();

        private readonly Dictionary<GameObject, EnemyBaseRefactored> componentCache = new Dictionary<GameObject, EnemyBaseRefactored>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (enemyFactory == null) enemyFactory = EnemyFactory.Instance;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (enemyFactory == null) enemyFactory = EnemyFactory.Instance;
        }

        public EnemyBaseRefactored GetEnemy(GameObject prefab, Vector3 spawnPosition = default)
        {
            string enemyType = prefab.name;

            if (!poolDictionary.TryGetValue(enemyType, out Queue<EnemyBaseRefactored> queue))
            {
                queue = new Queue<EnemyBaseRefactored>();
                poolDictionary.Add(enemyType, queue);
            }

            if (queue.Count > 0)
            {
                EnemyBaseRefactored enemyToReuse = queue.Dequeue();
                return enemyToReuse;
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

            Debug.LogError("EnemyPool: No hay EnemyFactory asignada ni en la escena.");
            return null;
        }

        public void ReturnEnemy(EnemyBaseRefactored enemyToReturn, string enemyType)
        {
            if (enemyToReturn == null) return;

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
                enemyScript = enemyObjToReturn.GetComponent<EnemyBaseRefactored>();
                if (enemyScript != null)
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