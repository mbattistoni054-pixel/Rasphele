using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance;

        private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }


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
                projObj = Instantiate(prefab, position, rotation);
                projObj.name = key;
            }

            return projObj;
        }


        public void ReturnProjectile(GameObject projObj)
        {
            projObj.SetActive(false);
            
            string key = projObj.name;
            
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
