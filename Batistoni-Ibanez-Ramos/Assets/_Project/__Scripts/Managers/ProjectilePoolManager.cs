using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{

    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }

        private readonly Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
        private readonly HashSet<GameObject> pooledObjects = new HashSet<GameObject>();
        private readonly Dictionary<GameObject, Rigidbody> rbCache = new Dictionary<GameObject, Rigidbody>();
        private readonly Dictionary<GameObject, Component> componentCache = new Dictionary<GameObject, Component>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            string key = prefab.name;

            if (!pool.TryGetValue(key, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pool.Add(key, queue);
            }

            GameObject projObj = null;

            while (queue.Count > 0 && projObj == null)
            {
                GameObject candidate = queue.Dequeue();
                if (candidate != null)
                {
                    pooledObjects.Remove(candidate);
                    projObj = candidate;
                }
            }

            if (projObj != null)
            {
                projObj.transform.SetPositionAndRotation(position, rotation);
                projObj.SetActive(true);
            }
            else
            {
                projObj = Instantiate(prefab, position, rotation);
                projObj.name = key;

                if (projObj.TryGetComponent(out Rigidbody rb))
                {
                    rbCache[projObj] = rb;
                }
            }

            return projObj;
        }


        public T GetProjectile<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject obj = GetProjectile(prefab, position, rotation);
            if (obj == null) return null;

            if (componentCache.TryGetValue(obj, out Component cached) && cached is T typedCached)
            {
                return typedCached;
            }

            if (obj.TryGetComponent(out T comp))
            {
                componentCache[obj] = comp;
                return comp;
            }

            return null;
        }

        public void ReturnProjectile(GameObject projObj)
        {
            if (projObj == null) return;

            if (!pooledObjects.Add(projObj)) return;

            projObj.SetActive(false);

            string key = projObj.name;

            if (!rbCache.TryGetValue(projObj, out Rigidbody rb))
            {
                projObj.TryGetComponent(out rb);
                rbCache[projObj] = rb;
            }

            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (!pool.TryGetValue(key, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pool.Add(key, queue);
            }

            queue.Enqueue(projObj);
        }
    }
}