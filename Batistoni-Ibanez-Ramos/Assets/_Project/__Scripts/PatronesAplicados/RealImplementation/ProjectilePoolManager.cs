using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// ProjectilePoolManager: Gestiona los "cascarones vacos" de los proyectiles 
    /// (Balas de dron, nubes, meteoros) para evitar instanciarlos constantemente.
    /// </summary>
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance;

        // Diccionario para mltiples tipos de proyectiles (llave: nombre del prefab)
        private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Obtiene un proyectil del pool o lo instancia si no hay disponibles.
        /// </summary>
        public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            string key = prefab.name;

            if (!pool.ContainsKey(key))
            {
                pool.Add(key, new Queue<GameObject>());
            }

            GameObject projObj;

            if (pool[key].Count > 0)
            {
                projObj = pool[key].Dequeue();
                projObj.transform.position = position;
                projObj.transform.rotation = rotation;
                projObj.SetActive(true);
            }
            else
            {
                // Si no hay, creamos uno nuevo.
                projObj = Instantiate(prefab, position, rotation);
                projObj.name = key; // Limpiamos el nombre para que el ReturnProjectile funcione bien
            }

            return projObj;
        }

        /// <summary>
        /// Devuelve el proyectil al pool (llamado por el proyectil al impactar o morir).
        /// </summary>
        public void ReturnProjectile(GameObject projObj)
        {
            projObj.SetActive(false);
            
            // El nombre debe coincidir con la key del diccionario
            string key = projObj.name;
            
            // Re-centramos variables fsicas si tuviera Rigidbody (opcional pero recomendado)
            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (!pool.ContainsKey(key))
            {
                pool.Add(key, new Queue<GameObject>());
            }

            pool[key].Enqueue(projObj);
        }
    }
}
