using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    /// <summary>
    /// Object Pool: Mantiene una reserva de enemigos apagados para evitar el costo de Instantiate/Destroy en loop.
    /// Funciona en conjunto con la Factory.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance;

        // Un diccionario para manejar mltiples pools (una cola por cada tipo de Prefab)
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Solicita un enemigo del pool. Si no hay, le pide a la Factory que lo cree.
        /// </summary>
        public GameObject GetEnemy(GameObject prefab, Vector3 spawnPosition = default)
        {
            string enemyType = prefab.name;

            // Si el pool de este tipo no existe, lo creamos
            if (!poolDictionary.ContainsKey(enemyType))
            {
                poolDictionary.Add(enemyType, new Queue<GameObject>());
            }

            // Si hay enemigos en la cola, sacamos uno
            if (poolDictionary[enemyType].Count > 0)
            {
                GameObject enemyToReuse = poolDictionary[enemyType].Dequeue();
                return enemyToReuse;
            }
            else
            {
                // Si la cola está vacía, delegamos la creación física a la Factory
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

        /// <summary>
        /// Devuelve un enemigo al pool en lugar de destruirlo.
        /// Debes llamar a este mtodo desde EnemyBase.Die() (ej. EnemyPool.Instance.ReturnEnemy(gameObject, prefab.name))
        /// en lugar de usar Destroy(gameObject).
        /// </summary>
        public void ReturnEnemy(GameObject enemyToReturn, string enemyType)
        {
            enemyToReturn.SetActive(false); // Apagamos el enemigo

            if (!poolDictionary.ContainsKey(enemyType))
            {
                poolDictionary.Add(enemyType, new Queue<GameObject>());
            }

            // Lo metemos de vuelta a la cola
            poolDictionary[enemyType].Enqueue(enemyToReturn);
        }
    }
}
