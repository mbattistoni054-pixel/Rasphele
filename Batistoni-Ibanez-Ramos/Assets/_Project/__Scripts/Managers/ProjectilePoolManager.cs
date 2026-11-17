using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }

        private readonly Dictionary<int, Queue<GameObject>> _pool = new Dictionary<int, Queue<GameObject>>(32);
        private readonly Dictionary<GameObject, int> _instanceToPrefabId = new Dictionary<GameObject, int>(256);
        private readonly HashSet<GameObject> _pooledObjects = new HashSet<GameObject>(256);
        private readonly Dictionary<GameObject, Rigidbody> _rbCache = new Dictionary<GameObject, Rigidbody>(256);
        private readonly Dictionary<GameObject, Component> _componentCache = new Dictionary<GameObject, Component>(256);
        private readonly Dictionary<GameObject, IProjectile> _projectileInterfaceCache = new Dictionary<GameObject, IProjectile>(256);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            int prefabId = prefab.GetInstanceID();

            if (!_pool.TryGetValue(prefabId, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>(32);
                _pool.Add(prefabId, queue);
            }

            GameObject projObj = null;

            while (queue.Count > 0 && projObj == null)
            {
                GameObject candidate = queue.Dequeue();
                if (candidate != null)
                {
                    _pooledObjects.Remove(candidate);
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
                _instanceToPrefabId[projObj] = prefabId;

                if (projObj.TryGetComponent(out Rigidbody rb))
                {
                    _rbCache[projObj] = rb;
                }

                if (projObj.TryGetComponent(out IProjectile projInterface))
                {
                    _projectileInterfaceCache[projObj] = projInterface;
                }
            }

            return projObj;
        }

        public IProjectile GetProjectileInterface(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GetProjectile(prefab, position, rotation);
            if (obj == null) return null;

            if (_projectileInterfaceCache.TryGetValue(obj, out IProjectile cached))
            {
                return cached;
            }

            if (obj.TryGetComponent(out IProjectile projInterface))
            {
                _projectileInterfaceCache[obj] = projInterface;
                return projInterface;
            }

            return null;
        }

        public T GetProjectile<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject obj = GetProjectile(prefab, position, rotation);
            if (obj == null) return null;

            if (_componentCache.TryGetValue(obj, out Component cached) && cached is T typedCached)
            {
                return typedCached;
            }

            if (obj.TryGetComponent(out T comp))
            {
                _componentCache[obj] = comp;
                return comp;
            }

            return null;
        }

        public void ReturnProjectile(GameObject projObj)
        {
            if (projObj == null) return;
            if (!_pooledObjects.Add(projObj)) return;

            projObj.SetActive(false);

            if (_rbCache.TryGetValue(projObj, out Rigidbody rb) && rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (!_instanceToPrefabId.TryGetValue(projObj, out int prefabId))
            {
                prefabId = projObj.GetInstanceID();
                _instanceToPrefabId[projObj] = prefabId;
            }

            if (!_pool.TryGetValue(prefabId, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>(32);
                _pool.Add(prefabId, queue);
            }

            queue.Enqueue(projObj);
        }
    }
}